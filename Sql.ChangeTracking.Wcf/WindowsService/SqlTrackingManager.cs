using Nerdle.AutoConfig;
using Polly;
using Serilog;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceTopShelf
{

    /// <summary>
    /// This is the class that actually does the work!!!
    /// </summary>
    public class SqlTrackingManager : ISqlTrackingManager
    {
        public Serilog.ILogger ServiceLogger { get; set; }
        public IDatabaseHelper DatabaseHelper { get; }

        public SqlTrackingManager(IDatabaseHelper databaseHelper)
        {
            DatabaseHelper = databaseHelper;
        }

        public Task ProcessInvoices(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                Policy.Handle<Exception>()
                    .WaitAndRetryForeverAsync(i => TimeSpan.FromSeconds(2), (ex, span) =>
                    {
                        ServiceLogger.Information("Error when polling for sql changes!" + ex.Message);
                    })

                    .ExecuteAsync(async ct =>
                    {
                        ServiceLogger.Information("Starting to poll for new sql changes.");
                        await PollOnInvoices(cancellationToken);
                    }, cancellationToken);
            }, cancellationToken);
        }

        private async Task PollOnInvoices(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {

                ServiceLogger.Information("Polling ...");
                // This logs the whole structured object
                ServiceLogger.Information("{@Invoice}", DatabaseHelper.GetData());

                await Task.Delay(2000);
            }
        }
    }
   
}
