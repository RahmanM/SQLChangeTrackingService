using Serilog;

namespace ServiceTopShelf
{
    /// <summary>
    /// Things that needs to be passed to base and are needed for configuration
    /// </summary>
    public class ServiceDependencies : IServiceDependencies
    {
        public ILogger Logger { get; set; }
    }
}
