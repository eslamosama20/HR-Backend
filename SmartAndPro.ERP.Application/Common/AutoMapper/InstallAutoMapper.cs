using Microsoft.Extensions.DependencyInjection;

namespace SmartAndPro.ERP.Application.Common.AutoMapper
{
    public static class DependencyInjection
    {
        public static IServiceCollection InstallAutoMapper(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(MappingProfiles), typeof(HRMappingProfiles));
            return services;
        }
    }
}