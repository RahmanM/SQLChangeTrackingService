using Polly;
using Sql.ChangeTracking.Common;
using System;
using System.ServiceModel;
using System.Threading;

namespace Sql.ChangeTracking.Client
{
    /// <summary>
    /// Test client that using the change tracking service
    /// </summary>
    class Program 
    {

        static void Main(string[] args)
        {
            try
            {
                Policy.Handle<Exception>()
                   .RetryForever()
                   .Execute(() =>
                   {
                       Execute();
                   }
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.Read();
        }

        private static void Execute()
        {
            var client = new DuplexChannelFactoryClient<IChangeTrackingSubscriptions, IEventNotificationCallback>
                (new CallbackCleint(), "net.tcp://localhost:9002/Sql.ChangeTracking.Wcf/wcf/SqlChangeTrackingWcfService");

            var channel = client.CreateChannel();
            ((ICommunicationObject) channel).Opened += Channel_Opned;
            ((ICommunicationObject) channel).Closed += Channel_Closed;
            ((ICommunicationObject) channel).Faulted += Channel_Faulted;

            // Subscribe as "Client1" and wait for changes to "Customer" table
            channel.Subscribe("Client1", "Customer");
            channel.Subscribe("Client1", "Employee");
           
        }

        private static void Channel_Faulted(object sender, EventArgs e)
        {
            Console.WriteLine("Channel Faulted");
            Execute();
        }

        private static void Channel_Closed(object sender, EventArgs e)
        {
            Console.WriteLine("Channel Closed");
            Execute();
        }

        private static void Channel_Opned(object sender, EventArgs e)
        {
            Console.WriteLine($"Channel Opened {DateTime.Now}");
        }

        private void HandleExceptionAndRetry(Func<ICommunicationObject> connectToService)
        {
            Thread.Sleep(1000);
            connectToService();
        }
    }

    public class CallbackCleint : IEventNotificationCallback, IDisposable
    {
        private static int Counter = 0;

        public void Dispose()
        {
            Console.WriteLine("Disposed");
        }

        public void PublishTableChange(string tableName)
        {
            Counter++;
            Console.WriteLine($"{DateTime.Now} - Table {tableName} was changed. Total messages: {Counter}");
        }
    }
}
