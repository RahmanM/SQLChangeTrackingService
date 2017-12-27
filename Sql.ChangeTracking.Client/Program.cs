using Sql.ChangeTracking.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
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
            DuplexChannelFactory<IChangeTrackingSubscriptions> channelFactory = null;

            try
            {
                // Basic A, B of the wcf service
                BasicHttpBinding binding = new BasicHttpBinding();
                //EndpointAddress endpointAddress = new EndpointAddress("net.tcp://localhost:9002/Sql.ChangeTracking.Wcf/Wcf/");
                EndpointAddress endpointAddress = new EndpointAddress("net.tcp://localhost:9002/Sql.ChangeTracking.Wcf/wcf/SqlChangeTrackingWcfService");

                // Pass Binding and EndPoint address to ChannelFactory, NB: it has to be DuplexChannel as we expect to be called back 
                channelFactory  =
                  new DuplexChannelFactory<IChangeTrackingSubscriptions>(
                      new InstanceContext(new CallbackCleint()),
                      new NetTcpBinding(),
                      endpointAddress);

                IChangeTrackingSubscriptions channel = channelFactory.CreateChannel();

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
            }
            catch (FaultException ex)
            {
                if (channelFactory != null)
                    channelFactory.Abort();

                Console.WriteLine(ex.Message);
            }
            catch (CommunicationException ex)
            {
                //Communication error  
                if (channelFactory != null)
                    channelFactory.Abort();

                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                if (channelFactory != null)
                    channelFactory.Abort();

                Console.WriteLine(ex.Message);
            }

            Console.Read();
        }

       
    }

    public class CallbackCleint : IEventNotificationCallback, IDisposable
    {
        public void Dispose()
        {
            Console.WriteLine("Disposed");
        }

        public void PublishTableChange(string tableName)
        {
            Console.WriteLine($"Table {tableName} was changed.");
        }
    }
}
