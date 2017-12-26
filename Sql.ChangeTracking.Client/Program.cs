using Sql.ChangeTracking.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Sql.ChangeTracking.Client
{
    class Program 
    {
        static void Main(string[] args)
        {
            DuplexChannelFactory<IChangeTrackingSubscriptions> channelFactory = null;

            try
            {
                //Create a binding of the type exposed by service  
                BasicHttpBinding binding = new BasicHttpBinding();

                //Create EndPoint address  
                EndpointAddress endpointAddress = new EndpointAddress("net.tcp://localhost:9002/Sql.ChangeTracking.Wcf/Wcf/");

                //Pass Binding and EndPoint address to ChannelFactory  
                channelFactory  =
                  new DuplexChannelFactory<IChangeTrackingSubscriptions>(
                      new InstanceContext(new CallbackCleint()),
                      new NetTcpBinding(),
                      endpointAddress);

                //Now create the new channel as below  
                IChangeTrackingSubscriptions channel = channelFactory.CreateChannel();

                //Call the service method on this channel as below  
                channel.Subscribe("Client1", "Customer");

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

            Console.ReadLine();
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
            Console.WriteLine("Got callback");
        }
    }
}
