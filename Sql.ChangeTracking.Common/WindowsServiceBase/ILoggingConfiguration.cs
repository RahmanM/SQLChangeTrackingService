using Serilog;

namespace ServiceTopShelf
{
    public interface ILoggingConfiguration
    {
        LogginInfo LoggingInfo { get; set; }

        ILogger ConfigureLogger();
    }
}