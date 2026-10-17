namespace SmartAndPro.ERP.WebAPI.Installers.@base
{
    public static class ServicesInstaller
    {
        public static void InstallServices(this WebApplicationBuilder applicationBuilder)
        {
            var installers = typeof(IServiceInstaller).Assembly.ExportedTypes.Where(serviceInstaller =>
                typeof(IServiceInstaller).IsAssignableFrom(serviceInstaller) && !serviceInstaller.IsInterface && !serviceInstaller.IsAbstract)
                .Select(Activator.CreateInstance)
                .Cast<IServiceInstaller>().ToList();

            installers.ForEach(installer => installer.Install(applicationBuilder));
        }
    }
}
