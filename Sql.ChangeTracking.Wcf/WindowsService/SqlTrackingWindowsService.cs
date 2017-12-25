using ServiceTopShelf.DI;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceTopShelf
{
    public class SqlTrackingWindowsService : WindowsServiceBase
    {
        private CancellationTokenSource cancellationTokenSource;
        private List<Task> Tasks { get; set; }

        public override bool OnContinue()
        {
            BaseLogger.Information("OnContinue");
            return true;
        }

        public override bool OnPause()
        {
            BaseLogger.Information("OnPause");
            return true;
        }

        public override void OnShutdown()
        {
            BaseLogger.Information("OnShutdown [S]");
            cancellationTokenSource.Cancel();
            BaseLogger.Information("OnShutdown [E]");
        }

        public override void OnStart()
        {
            // Will be called the first time windows service is started!
            Tasks = new List<Task>();

            HostWcfService();


            //// Configure the DI and dependencies and intitialize the Manager
            //BaseLogger.Information("OnStart [S]" + DateTime.Now);
            //var container = ConfigureDependency.Configure();

            //BaseLogger.Information("Configuring dependencies");
            //var invoiceManager = container.GetInstance<ISqlTrackingManager>();
            //invoiceManager.ServiceLogger = this.BaseLogger;

            //BaseLogger.Information("Initialsing the invoice manager!");
            //cancellationTokenSource = new CancellationTokenSource();
            //Tasks.Add(invoiceManager.ProcessInvoices(cancellationTokenSource.Token));

            BaseLogger.Information("OnStart [E]" + DateTime.Now);
        }

        private ServiceHost _ServiceHost = null;

        private void HostWcfService()
        {
            if (_ServiceHost != null) _ServiceHost.Close();

            //string httpBaseAddress = "http://localhost:9001/Sql.ChangeTracking.Wcf/Wcf";
            string tcpBaseAddress = "net.tcp://localhost:9002/Sql.ChangeTracking.Wcf/Wcf";

            Uri[] adrbase = {  new Uri(tcpBaseAddress)};
            _ServiceHost = new ServiceHost(typeof(Sql.ChangeTracking.Wcf.SqlChangeTrackingWcfService), adrbase);

            ServiceMetadataBehavior serviceBehaviour = new ServiceMetadataBehavior();
            if (!_ServiceHost.Description.Behaviors.Contains(serviceBehaviour))
                _ServiceHost.Description.Behaviors.Add(serviceBehaviour);

            //WSDualHttpBinding httpBinding = new WSDualHttpBinding();
            //_ServiceHost.AddServiceEndpoint(typeof(Sql.ChangeTracking.Common.IChangeTrackingSubscriptions), httpBinding, httpBaseAddress);
            //_ServiceHost.AddServiceEndpoint(typeof(IMetadataExchange),
            //MetadataExchangeBindings.CreateMexHttpBinding(), "mex");

            NetTcpBinding tcpBinding = new NetTcpBinding();
            _ServiceHost.AddServiceEndpoint(typeof(Sql.ChangeTracking.Common.IChangeTrackingSubscriptions), tcpBinding, tcpBaseAddress);
            _ServiceHost.AddServiceEndpoint(typeof(IMetadataExchange),
            MetadataExchangeBindings.CreateMexTcpBinding(), "mex");

            _ServiceHost.Open();

        }

        public override void OnStop()
        {
            BaseLogger.Information("OnStop [S]");
            cancellationTokenSource.Cancel();
            BaseLogger.Information("OnStop [E]");

            if (_ServiceHost != null)
            {
                _ServiceHost.Close();
                _ServiceHost = null;
            }
        }
    }
}