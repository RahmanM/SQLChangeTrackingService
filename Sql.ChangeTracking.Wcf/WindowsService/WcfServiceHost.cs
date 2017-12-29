using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using Sql.ChangeTracking.Common;

namespace SqlChangeTrackingProducerConsumer
{
    public static class WcfServiceHost
    {

        private static string tcpBaseAddress = "net.tcp://localhost:9002/Sql.ChangeTracking.Wcf/wcf/SqlChangeTrackingWcfService";
        private static ServiceHost _ServiceHost = null;

        public static ServiceHost GetServiceHost(IChangeTrackingSubscriptions service)
        {
            if (_ServiceHost != null) _ServiceHost.Close();          

            Uri[] adrbase = { new Uri(tcpBaseAddress) };

            // initialize the WCF service using DI and inject it into hose
            _ServiceHost = new ServiceHost(service, adrbase);
            // NB: important be able to inject service instance above
            ((ServiceBehaviorAttribute)_ServiceHost.Description.Behaviors[typeof(ServiceBehaviorAttribute)]).InstanceContextMode = InstanceContextMode.Single;

            ServiceMetadataBehavior serviceBehaviour = new ServiceMetadataBehavior();
            if (!_ServiceHost.Description.Behaviors.Contains(serviceBehaviour))
                _ServiceHost.Description.Behaviors.Add(serviceBehaviour);

            NetTcpBinding tcpBinding = new NetTcpBinding();
            // NB: important to keep the duplex session open for long, though should be some reasonable number?
            tcpBinding.OpenTimeout = TimeSpan.MaxValue;
            tcpBinding.CloseTimeout = TimeSpan.MaxValue;
            tcpBinding.ReceiveTimeout = TimeSpan.MaxValue;

            _ServiceHost.AddServiceEndpoint(typeof(IChangeTrackingSubscriptions), tcpBinding, tcpBaseAddress);
            _ServiceHost.AddServiceEndpoint(typeof(IMetadataExchange),
            MetadataExchangeBindings.CreateMexTcpBinding(), "mex");

            _ServiceHost.Open();

            return _ServiceHost;
        }
    }

}