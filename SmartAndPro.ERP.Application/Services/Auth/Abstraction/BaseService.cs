using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;

namespace SmartAndPro.ERP.Application.Services.Auth.Abstraction
{
    public abstract class BaseService
    {
        private readonly RepositoriesFactory _repositoriesFactory;
        private readonly AppSettings _appSettings;

        public BaseService(RepositoriesFactory repositoriesFactory, AppSettings appSettings)
        {
            _repositoriesFactory = repositoriesFactory;
            _appSettings = appSettings;
        }
    }
}