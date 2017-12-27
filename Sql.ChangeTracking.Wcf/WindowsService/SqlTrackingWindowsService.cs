using Serilog;
using SqlChangeTrackingProducerConsumer.DI;
using SimpleInjector.Integration.Wcf;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using SimpleInjector;
using Sql.ChangeTracking.Common;

namespace SqlChangeTrackingProducerConsumer
{
    public class SqlTrackingWindowsService : WindowsServiceBase
    {
        public SqlTrackingWindowsService(ILogger logger)
        {
            Logger = logger;
        }

        private CancellationTokenSource cancellationTokenSource;
        private List<Task> Tasks { get; set; }
        public ILogger Logger { get; }

        public override bool OnContinue()
        {
            Logger.Information("OnContinue");
            return true;
        }

        public override bool OnPause()
        {
            Logger.Information("OnPause");
            return true;
        }

        public override void OnShutdown()
        {
            Logger.Information("OnShutdown [S]");
            cancellationTokenSource.Cancel();
            if (_ServiceHost != null)
            {
                _ServiceHost.Close();
                _ServiceHost = null;
            }
            Logger.Information("OnShutdown [E]");
        }

        public override void OnStart()
        {
            // Will be called the first time windows service is started!
            Tasks = new List<Task>();

            // Configure the DI and dependencies and intitialize the Manager
            Logger.Information("OnStart [S]" + DateTime.Now);
            var container = ConfigureDependency.Configure();
            SimpleInjectorServiceHostFactory.SetContainer(container);


            Logger.Information("Configuring dependencies");
            var changeTrackingManager = container.GetInstance<ISqlTrackingManager>();
            changeTrackingManager.logger = this.Logger;

            Logger.Information("Initialsing the change tracking manager!");
            cancellationTokenSource = new CancellationTokenSource();
            Tasks.Add(changeTrackingManager.ProcessChangedTables(cancellationTokenSource.Token));

            // Initialise wcf service host
            // Wcf Service Instance
            var wcfService = container.GetInstance<IChangeTrackingSubscriptions>();
            wcfService.Logger = this.Logger;
            _ServiceHost = WcfServiceHost.GetServiceHost(wcfService);

            Logger.Information("OnStart [E]" + DateTime.Now);
        }
       
        private ServiceHost _ServiceHost = null;

        public override void OnStop()
        {
            Logger.Information("OnStop [S]");
            cancellationTokenSource.Cancel();

            if (_ServiceHost != null)
            {
                _ServiceHost.Close();
                _ServiceHost = null;
            }

            Logger.Information("OnStop [E]");

        }
    }

}