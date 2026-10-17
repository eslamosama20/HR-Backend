using SmartAndPro.ERP.WebAPI.Installers.@base;

namespace SmartAndPro.ERP.WebAPI.Installers
{
    public class CORSInstaller : IServiceInstaller
    {
        public void Install(WebApplicationBuilder application)
        {
            string? corsDomains = application.Configuration.GetValue<string>("AppSettings:CorsDomains");
            string? tokenKey = application.Configuration.GetValue<string>("AppSettings:Token:Key");
            string? tokenIssuer = application.Configuration.GetValue<string>("AppSettings:Token:Issuer");
            string? tokenDurationInDays = application.Configuration.GetValue<string>("AppSettings:Token:DurationInDays");


            application.Services.AddCors(options =>
            {
                options.AddPolicy("AllowMyOrigin", builder =>
                {
                    builder.WithOrigins(corsDomains.Split(',').Select(domain => domain.Trim()).ToArray()).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                });
            });
        }
    }
}
