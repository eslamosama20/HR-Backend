using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.Application.Authorization;
using SmartAndPro.ERP.Application.Services.Auth;
using SmartAndPro.ERP.Application.Services.Auth.Interfaces;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Infrastructure.Contexts.Utility;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.WebAPI.Installers.@base;

namespace SmartAndPro.ERP.WebAPI.Installers;

public class ScoppedServicesInstaller : IServiceInstaller
{
    public void Install(WebApplicationBuilder application)
    {
        application.Services.AddHttpContextAccessor();
        //application.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        application.Services.AddScoped<RepositoriesFactory>();

        application.Services.AddScoped<ServiceBaseFactory>();
        application.Services.AddScoped<ServicesFactory>();
        
        
        application.Services.AddScoped<IAuthService, AuthService>();
        application.Services.AddScoped<IAuthorizationPolicyService, AuthorizationPolicyService>();
        application.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        application.Services.AddScoped<IAuthorizationHandler, RoleAuthorizationHandler>();
        
        application.Services.AddScoped<IRoleService, RoleService>();
        application.Services.AddScoped<IPermissionService, PermissionService>();
        application.Services.AddScoped<IDomainDetailsService, DomainDetailsService>();
        
        application.Services.AddScoped<IAutoNumberGenerator, AutoNumberGenerator>();

        // HR & Biometric Services
        application.Services.AddScoped<SmartAndPro.ERP.WebAPI.Services.ZkTecoAttendanceFetcher>();
        application.Services.AddScoped<Attendance_ZKTeco_Service.Interfaces.IZKTecoAttendance_DataFetchBL, Attendance_ZKTeco_Service.Logics.ZKTecoAttendance_DataFetchBL>();
        application.Services.AddScoped<SmartAndPro.ERP.WebAPI.Services.IHangfireSchedulerService, SmartAndPro.ERP.WebAPI.Services.HangfireSchedulerService>();
        application.Services.AddScoped<SmartAndPro.ERP.Application.Services.HR.AttendanceService>();
    }
}