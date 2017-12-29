using Nerdle.AutoConfig;
using Polly;
using Serilog;
using Sql.ChangeTracking.Common;
using Sql.ChangeTracking.Data;
using SqlChangeTrackingProducerConsumer.DI;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SqlChangeTrackingProducerConsumer
{

    /// <summary>
    /// This is the class that actually does the work!!!
    /// </summary>
    public class SqlTrackingManager : ISqlTrackingManager
    {
        public Serilog.ILogger logger { get; set; }
        public IDatabaseHelper DatabaseHelper { get; }
        public IChangeTrackingSubscriptions WcfService { get; }

        public SqlTrackingManager(IDatabaseHelper databaseHelper, IChangeTrackingSubscriptions wcfService)
        {
            DatabaseHelper = databaseHelper;
            WcfService = wcfService;
        }

        public Task ProcessChangedTables(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                Policy.Handle<Exception>()
                    .WaitAndRetryForeverAsync(i => TimeSpan.FromSeconds(2), (ex, span) =>
                    {
                        logger.Information("Error when polling for sql changes!" + ex.Message);
                    })

                    .ExecuteAsync(async ct =>
                    {
                        logger.Information("Starting to poll for new sql changes.");
                        await PollOnChangedTrackingTables(cancellationToken);
                    }, cancellationToken);
            }, cancellationToken);
        }

        private async Task PollOnChangedTrackingTables(CancellationToken cancellationToken)
        {
            var settings = AutoConfig.Map<ChangeTrackingAppSettings>();
            int degreeOfParallelism = settings.DegreeOfParallelism;
            var producerConsumer = new ProducerConsumerQueue<UspTableVersionChangeTrackingReturnModel>(WcfService.TableChanged, degreeOfParallelism, logger, cancellationToken);

            while (!cancellationToken.IsCancellationRequested)
            {

                Console.WriteLine($"Polling {DateTime.Now}...");

                var versionChanges = DatabaseHelper.GetData();

                foreach (var change in versionChanges)
                {
                    Console.WriteLine($"Change: {change.Id} -> {change.Name} - {change.SysChangeOperation} - {change.SysChangeVersion}");
                    producerConsumer.Produce(change);
                }

                await Task.Delay(settings.PollingFrequencyMilliSeconds);
            }
        }
    }

}
