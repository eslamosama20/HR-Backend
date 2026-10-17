using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.Application.Authorization;
using SmartAndPro.ERP.Application.Services.Auth;
using SmartAndPro.ERP.Application.Services.Auth.Interfaces;
using SmartAndPro.ERP.WebAPI.Installers.@base;

namespace SmartAndPro.ERP.WebAPI.Installers;

public class AuthorizationInstaller : IServiceInstaller
{
    public void Install(WebApplicationBuilder application)
    {
        application.Services.AddScoped<IAuthorizationPolicyService, AuthorizationPolicyService>();
        application.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        application.Services.AddAuthorization();
    }
}