using Polly;
using Sql.ChangeTracking.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
               .Execute(() => ConnectToService());
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message); ;
            }

            Console.Read();
        }

        private static void ConnectToService()
        {
            DuplexChannelFactory<IChangeTrackingSubscriptions> channelFactory = null;

            try
            {
                // Basic A, B of the wcf service
                BasicHttpBinding binding = new BasicHttpBinding();
                //EndpointAddress endpointAddress = new EndpointAddress("net.tcp://localhost:9002/Sql.ChangeTracking.Wcf/Wcf/");
                EndpointAddress endpointAddress = new EndpointAddress("net.tcp://localhost:9002/Sql.ChangeTracking.Wcf/wcf/SqlChangeTrackingWcfService");

                // Pass Binding and EndPoint address to ChannelFactory, NB: it has to be DuplexChannel as we expect to be called back 
                channelFactory =
                  new DuplexChannelFactory<IChangeTrackingSubscriptions>(
                      new InstanceContext(new CallbackCleint()),
                      new NetTcpBinding(),
                      endpointAddress);

                channelFactory.Faulted += ChannelFactory_Faulted;
                channelFactory.Closed += ChannelFactory_Closed;
                channelFactory.Opened += ChannelFactory_Opened;

                IChangeTrackingSubscriptions channel = channelFactory.CreateChannel();

                ((ICommunicationObject)channel).Opened += Channel_Opned;
                ((ICommunicationObject)channel).Closed += Channel_Closed;
                ((ICommunicationObject)channel).Faulted += Channel_Faulted;

                // Subscribe as "Client1" and wait for changes to "Customer" table
                channel.Subscribe("Client1", "Customer");
                channel.Subscribe("Client1", "Employee");

                Console.WriteLine("To unsubscribe press, 'u'");
                var input = Console.ReadLine();
                if (input == "u")
                {
                    channel.Unsubscribe("Client1", "Customer");
                    Console.WriteLine("Unsubscribed!");
                }

            }
            catch (TimeoutException ex)
            {
                //Timeout error  
                if (channelFactory != null)
                    channelFactory.Abort();

                Console.WriteLine(ex.Message);
                throw;
            }
            catch (FaultException ex)
            {
                if (channelFactory != null)
                    channelFactory.Abort();

                Console.WriteLine(ex.Message);
                throw;
            }
            catch (CommunicationException ex)
            {
                //Communication error  
                if (channelFactory != null)
                    channelFactory.Abort();

                Console.WriteLine(ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                if (channelFactory != null)
                    channelFactory.Abort();

                Console.WriteLine(ex.Message);
                throw;
            }
        }

        private static void Channel_Faulted(object sender, EventArgs e)
        {
            Console.WriteLine("Channel is Faulted.");
            HandleExceptionAndRetry(ConnectToService);
        }

     
        private static void HandleExceptionAndRetry(Action action)
        {
            // Todo: use polly etc
            Thread.Sleep(1000);
            action();
        }

        private static void Channel_Closed(object sender, EventArgs e)
        {
            Console.WriteLine("Channel is closed.");
            HandleExceptionAndRetry(ConnectToService);
        }

        private static void Channel_Opned(object sender, EventArgs e)
        {
            Console.WriteLine("Channel is opned.");
        }

        private static void ChannelFactory_Opened(object sender, EventArgs e)
        {
            Console.WriteLine("Channel Factory is Opened.");
        }

        private static void ChannelFactory_Closed(object sender, EventArgs e)
        {
            Console.WriteLine("Channel Factory is closed.");
            HandleExceptionAndRetry(ConnectToService);
        }

        private static void ChannelFactory_Faulted(object sender, EventArgs e)
        {
            Console.WriteLine("Channel is Faulted.");
            HandleExceptionAndRetry(ConnectToService);
        }
    }

    public class CallbackCleint : IEventNotificationCallback, IDisposable
    {
        private static int Counter = 0;

        public void Dispose()
        {
            Console.WriteLine("Disposed");
        }

        public  void PublishTableChange(string tableName)
        {
            Counter++;
            Console.WriteLine($"{DateTime.Now} - Table {tableName} was changed. Total messages: {Counter}");
        }
    }
}
