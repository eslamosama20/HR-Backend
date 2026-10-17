//using Microsoft.IdentityModel.Tokens;
//using System.Text;
//using SmartAndPro.ERP.WebAPI.Installers._base;
//using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
//using Microsoft.AspNetCore.Authentication.JwtBearer;

//namespace SmartAndPro.ERP.WebAPI.Installers
//{
//    public class JWTInstaller : IServiceInstaller
//    {
//        public void Install(WebApplicationBuilder application)
//        {
//            var appSettingsSection = application.Configuration.GetSection("AppSettings");
//            application.Services.Configure<AppSettings>(appSettingsSection);

//            // configure jwt authentication
//            var appSettings = appSettingsSection.Get<AppSettings>();
//            application.Services.AddAuthentication(x =>
//            {
//                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//            })
//            .AddJwtBearer(x =>
//            {
//                x.RequireHttpsMetadata = false;
//                x.SaveToken = true;
//                x.TokenValidationParameters = new TokenValidationParameters
//                {
//                    ValidateIssuer = true,
//                    ValidateAudience = true,
//                    ValidateLifetime = true,
//                    ValidateIssuerSigningKey = true,

//                    ValidIssuer = appSettings.Token.Issuer,
//                    //ValidAudience = appSettings.TokenAudience,
//                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettings.Token.Key)),
//                    //TokenDecryptionKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettings.TokenEncryptionKey)),
//                    ClockSkew = TimeSpan.Zero,
//                };
//                x.Events = new JwtBearerEvents
//                {
//                    OnAuthenticationFailed = context =>
//                    {
//                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
//                        {
//                            context.Response.Headers.Add("Token-Expired", "true");
//                        }
//                        return Task.CompletedTask;
//                    }
//                };
//            });


//            application.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
//        }
//    }
//}
