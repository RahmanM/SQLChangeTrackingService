using Serilog;
using System.Threading;
using System.Threading.Tasks;

namespace SqlChangeTrackingProducerConsumer
{
    public interface ISqlTrackingManager
    {
        Task ProcessChangedTables(CancellationToken cancellationToken);
        ILogger logger { get; set; }
    }
}