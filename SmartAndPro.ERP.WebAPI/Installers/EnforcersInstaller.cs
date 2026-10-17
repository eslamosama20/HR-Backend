using System.Reflection;
using SmartAndPro.ERP.Domains.BusinessRulesEnforcer;
using SmartAndPro.ERP.WebAPI.Installers.@base;

namespace SmartAndPro.ERP.WebAPI.Installers;

public class EnforcersInstaller : IServiceInstaller
{
    public void Install(WebApplicationBuilder application)
    {
        var assembly = Assembly.GetExecutingAssembly(); // Change to the appropriate assembly if necessary
        var enforcerBaseType = typeof(EnforcerBase<>);
        var enforcerTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && t.BaseType != null && t.BaseType.IsGenericType &&
                        t.BaseType.GetGenericTypeDefinition() == enforcerBaseType);
        
        foreach (var enforcerType in enforcerTypes)
        {
            var interfaceType = enforcerType.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnforcer<>));

            if (interfaceType != null)
            {
                application.Services.AddTransient(interfaceType, enforcerType);
            }
        }
    }
}