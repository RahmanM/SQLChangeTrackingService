using Serilog;

namespace SqlChangeTrackingProducerConsumer
{
    public interface IServiceDependencies
    {
        ILogger Logger { get; set; }
    }
}