using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.WebAPI.Installers.@base;

namespace SmartAndPro.ERP.WebAPI.Installers
{
    public class InternalServicesInstaller : IServiceInstaller
    {
        public void Install(WebApplicationBuilder application)
        {
            var appSettingsSection = application.Configuration.GetSection("AppSettings");
            if (appSettingsSection is not null)
            {
                application.Services.Configure<AppSettings>(appSettingsSection);
                AppSettings applicationSettings = appSettingsSection.Get<AppSettings>();
                application.Services.Add(new ServiceDescriptor(typeof(AppSettings), applicationSettings));
            }
            application.Services.ConfigureHttpJsonOptions((opt) =>
            {
                opt.SerializerOptions.PropertyNameCaseInsensitive = true;
                opt.SerializerOptions.PropertyNamingPolicy = null;
            });

            application.Services.AddMemoryCache();
        }
    }
}
