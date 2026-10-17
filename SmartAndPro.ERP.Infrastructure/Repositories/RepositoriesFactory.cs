using SmartAndPro.ERP.Domains.Entities.Reservation;
using SmartAndPro.ERP.Infrastructure.Contexts;
using SmartAndPro.ERP.Infrastructure.Repositories.Base;
using SmartAndPro.ERP.Infrastructure.Repositories.Reservation;
using SmartAndPro.ERP.SharedKernel.Utilities;

namespace SmartAndPro.ERP.Infrastructure.Repositories
{
    public class RepositoriesFactory
    {
        ERPContext? context;
        IServiceProvider? serviceProvider;
        private readonly bool _defaultUseTransaction;
        Dictionary<Type, object> store = new Dictionary<Type, object>();
        public RepositoriesFactory(ERPContext context, IServiceProvider serviceProvider,bool useTransaction = false)
        {
            this.context = context;
            this.serviceProvider = serviceProvider;
            _defaultUseTransaction = useTransaction;
        }
        public RepositoriesFactory() { }


        public virtual IRepository<T> GetRepository<T>(bool? useTransaction = null) where T : class
        {
            if (context == null) throw new InvalidOperationException("Context is not initialized");

            IRepository<T>? repository = null;

            bool transactionFlag = useTransaction ?? _defaultUseTransaction;
            try
            {
                bool repositoryFoundInStore = store.ContainsKey(typeof(T));
                if (repositoryFoundInStore)
                    return (IRepository<T>)store[typeof(T)];

                // Handle specific repositories
                if (typeof(T) == typeof(ReservationPrintTableTemplate))
                {
                    repository = (IRepository<T>)(object)new ReservationPrintTableTemplateRepository(context);
                }
                else
                {
                    repository = new RepositoryBase<T, ERPContext>(context,transactionFlag);
                }

                store.Add(typeof(T), repository);
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
            }
            return repository ?? new RepositoryBase<T, ERPContext>(context,transactionFlag);
        }

        // يتيح الوصول لـ DbContext لتنفيذ Raw SQL مثل إحصائيات SQL Server
        public ERPContext GetDbContext()
        {
            if (context == null) throw new InvalidOperationException("Context is not initialized");
            return context;
        }
    }
}
