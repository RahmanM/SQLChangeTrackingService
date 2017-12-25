using Serilog;
using Serilog.Core;
using Serilog.Events;
using System.IO;
using System.Net;
using Topshelf;

namespace ServiceTopShelf
{

    public class ServiceConfigurationHelper
    {
        public WindowsServiceBase ServiceType { get; set; }
        public IServiceDependencies ServiceDependencies { get; }

        public Logger Logger { get; set; }

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

            Logger = ConfigureLogger(serviceDependencies);
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

                x.SetDescription("Topshelf Invoice Service");
                x.SetDisplayName("Topshelf Invoice Service");
                x.SetServiceName("TopshelfInvoiceService");

                x.EnableServiceRecovery(r =>
                {
                    r.RestartService(0);
                    r.RunProgram(7, "ping google.com");
                    r.RestartComputer(5, "message");
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

        public virtual Logger ConfigureLogger(IServiceDependencies serviceDependencies)
        {

            var template = "{MachineName}:{EnvironmentUserName} {Timestamp:dd/MM/yyy HH:mm:ss:mm} [{Level}] ({ThreadId}) {Message}{NewLine}{Exception}";

            return new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.WithThreadId()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentUserName()
                 .WriteTo.Console(
                    outputTemplate: template)
                .WriteTo.File(
                                Path.Combine(serviceDependencies.LoggingConfiguration?.LogFolder,  serviceDependencies.LoggingConfiguration?.LogFile),
                                rollingInterval: RollingInterval.Day,
                                outputTemplate: template
                              )
                .CreateLogger();
        }
    }
}
