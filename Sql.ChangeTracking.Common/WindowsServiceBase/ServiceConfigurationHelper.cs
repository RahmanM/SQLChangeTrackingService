using Serilog;
using Topshelf;

namespace SqlChangeTrackingProducerConsumer
{

    public class ServiceConfigurationHelper
    {
        public WindowsServiceBase ServiceType { get; set; }
        public IServiceDependencies ServiceDependencies { get; }

        private ILogger Logger { get; set; }

        public ServiceConfigurationHelper(WindowsServiceBase windowsService)
        {
            ServiceType = windowsService;
        }

        public ServiceConfigurationHelper(WindowsServiceBase windowsService, IServiceDependencies serviceDependencies)
        {
            if (windowsService == null)
                throw new System.ArgumentNullException(nameof(windowsService));

            if (serviceDependencies == null)
                throw new System.ArgumentNullException(nameof(serviceDependencies));

            ServiceType = windowsService;
            ServiceDependencies = serviceDependencies;

            Logger = serviceDependencies.Logger;
            Logger.Information("Constructor initialised.");
        }

        public void Configure()
        {
            Logger.Information("Configuring the base service [Start]");

            HostFactory.Run(x =>
            {
                x.Service<WindowsServiceBase>(sc =>
                {
                    sc.ConstructUsing(() => ServiceType);

                    // the start and stop methods for the service
                    sc.WhenStarted(s => s.Start(this.Logger));
                    sc.WhenStopped(s => s.Stop());

                    // optional pause/continue methods if used
                    sc.WhenPaused(s => s.Pause());
                    sc.WhenContinued(s => s.Continue());

                    // optional, when shutdown is supported
                    sc.WhenShutdown(s => s.Shutdown());

                    Logger.Information("Configuring the base service [End]");
                });

                x.SetDescription(ServiceDependencies.ServiceInfo.Description);
                x.SetDisplayName(ServiceDependencies.ServiceInfo.ServiceDisplayName);
                x.SetServiceName(ServiceDependencies.ServiceInfo.ServiceName);

                x.EnableServiceRecovery(r =>
                {
                    r.RestartService(0);
                    r.RestartService(0);
                    r.OnCrashOnly();
                    // number of days until the error count resets
                    r.SetResetPeriod(1);
                });

                x.BeforeInstall(() => 
                {
                    Logger.Information("Before installing the service");
                });

                x.AfterInstall(() =>
                {
                    Logger.Information("After installing the service");
                });

                x.AfterUninstall(() =>
                {
                    Logger.Information("After un-installing the service");
                });

                x.DependsOnEventLog();
                x.RunAsLocalSystem();
                x.StartAutomatically();

                x.OnException(e =>
                {
                    Logger.Error(e, "Error in the service.");
                });

            });

        }

    }
}
