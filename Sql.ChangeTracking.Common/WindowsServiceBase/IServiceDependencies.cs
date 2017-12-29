using Serilog;

namespace SqlChangeTrackingProducerConsumer
{
    public interface IServiceDependencies
    {
        ILogger Logger { get; set; }
        ServiceInfo ServiceInfo { get; set; }
    }
}