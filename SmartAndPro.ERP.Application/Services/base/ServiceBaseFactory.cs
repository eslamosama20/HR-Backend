using AutoMapper;
using Microsoft.AspNetCore.Http;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Utilities;

namespace SmartAndPro.ERP.Application.Services.@base
{
    public class ServiceBaseFactory
    {
        Dictionary<Type, object> store = new Dictionary<Type, object>();
        RepositoriesFactory repositoriesFactory;
        AppSettings appSettings;
        IMapper mapper;
        IHttpContextAccessor httpContextAccessor;

        public ServiceBaseFactory(RepositoriesFactory repositoriesFactory, AppSettings appSettings, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            this.repositoriesFactory = repositoriesFactory;
            this.appSettings = appSettings;
            this.mapper = mapper;
            this.httpContextAccessor = httpContextAccessor;
        }

        public T GetService<T>() where T : class
        {
            try
            {
                bool serviceFoundInStore = store.ContainsKey(typeof(T));
                if (serviceFoundInStore)
                    return (T)store[typeof(T)];
                try
                {
                    // Try to create the service using constructor with HttpContextAccessor (5 parameters)
                    var httpConstructor = typeof(T).GetConstructor(new[] { typeof(RepositoriesFactory), typeof(AppSettings), typeof(IMapper), typeof(ServiceBaseFactory), typeof(IHttpContextAccessor) });
                    if (httpConstructor != null)
                    {
                        var service = (T)httpConstructor.Invoke(new object[] { repositoriesFactory, appSettings, mapper, this, httpContextAccessor });
                        store.Add(typeof(T), service);
                        return service;
                    }

                    // Try to create the service using the new constructor (RepositoriesFactory, AppSettings, IMapper)
                    var newConstructor = typeof(T).GetConstructor(new[] { typeof(RepositoriesFactory), typeof(AppSettings), typeof(IMapper) });
                    if (newConstructor != null)
                    {
                        var service = (T)newConstructor.Invoke(new object[] { repositoriesFactory, appSettings, mapper });
                        store.Add(typeof(T), service);
                        return service;
                    }

                    // Fallback: Try to create the service using the old constructor (RepositoriesFactory, AppSettings)
                    var oldConstructor = typeof(T).GetConstructor(new[] { typeof(RepositoriesFactory), typeof(AppSettings), typeof(IMapper), typeof(ServiceBaseFactory) });
                    if (oldConstructor != null)
                    {
                        var service = (T)oldConstructor.Invoke(new object[] { repositoriesFactory, appSettings, mapper, this });
                        store.Add(typeof(T), service);
                        return service;
                    }

                    throw new Exception($"No valid constructor found for type {typeof(T).FullName}");
                }
                catch (Exception ex)
                {
                    LogService.LogError(ex);
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
            }
            return null;
        }
    }
}
