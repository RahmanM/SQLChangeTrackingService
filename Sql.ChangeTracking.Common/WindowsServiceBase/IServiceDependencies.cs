using Serilog;

namespace ServiceTopShelf
{
    public interface IServiceDependencies
    {
        ILogger Logger { get; set; }
    }
}