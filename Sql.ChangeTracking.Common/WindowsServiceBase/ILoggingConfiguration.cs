using Serilog;

namespace SqlChangeTrackingProducerConsumer
{
    public interface ILoggingConfiguration
    {
        LogginInfo LoggingInfo { get; set; }

        ILogger ConfigureLogger();
    }
}