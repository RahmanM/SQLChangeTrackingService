using Serilog;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceTopShelf
{
    public interface ISqlTrackingManager
    {
        Task ProcessInvoices(CancellationToken cancellationToken);
        ILogger ServiceLogger { get; set; }
    }
}