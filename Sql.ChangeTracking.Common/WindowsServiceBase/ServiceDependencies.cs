using System;
using Serilog;

namespace SqlChangeTrackingProducerConsumer
{
    /// <summary>
    /// Things that needs to be passed to base and are needed for configuration
    /// </summary>
    public class ServiceDependencies : IServiceDependencies
    {
        public ILogger Logger { get; set; }
        public ServiceInfo ServiceInfo { get; set; }
    }
}
