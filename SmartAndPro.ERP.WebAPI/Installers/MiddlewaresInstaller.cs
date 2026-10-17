using SmartAndPro.ERP.WebAPI.Installers.@base;
using SmartAndPro.ERP.WebAPI.Middlewares;

namespace SmartAndPro.ERP.WebAPI.Installers;

public class MiddlewaresInstaller : IServiceInstaller
{
    
    public void Install(WebApplicationBuilder application)
    {
        application.Services.AddTransient<ErrorHandlingMiddleware>();
    }
    
}