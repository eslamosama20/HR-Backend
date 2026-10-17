using AutoMapper;
using SmartAndPro.ERP.Application.Services.Auth.Abstraction;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Utilities;

namespace SmartAndPro.ERP.Application.Services.Auth
{
    public class ServicesFactory
    {
        private readonly RepositoriesFactory _repositoriesFactory;
        private readonly Dictionary<Type, BaseService> _store = new();
        private readonly AppSettings _appSettings;
        private readonly IMapper _mapper;
        
        public ServicesFactory(RepositoriesFactory repositoriesFactory , AppSettings appSettings , IMapper mapper)
        {
            _repositoriesFactory = repositoriesFactory;
            _appSettings = appSettings;
            _mapper = mapper;
        }

        public TService GetService<TService>() where TService : BaseService
        {
            try
            {
                if (_store.TryGetValue(typeof(TService), out var service))
                    return (TService)service;

                var serviceType = typeof(TService);
                var constructors = serviceType.GetConstructors();
                var parameters = constructors[0].GetParameters();

                TService newService;
                if (parameters.Length == 3 && parameters[2].ParameterType == typeof(IMapper))
                {
                    newService = (TService)Activator.CreateInstance(serviceType, _repositoriesFactory, _appSettings, _mapper)!;
                }
                else
                {
                    newService = (TService)Activator.CreateInstance(serviceType, _repositoriesFactory, _appSettings)!;
                }
                
                _store[serviceType] = newService;

                return newService;
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);

                throw;
            }
        }
    }
}
