using System;
using System.ServiceModel;
using System.Threading;

namespace Sql.ChangeTracking.Client
{
    public class DuplexChannelFactoryClient<T,U> 
        where T : class
        where U : class
    {

        public DuplexChannelFactoryClient(U callback, string serviceAddress)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            Callback = callback;
            ServiceAddress = serviceAddress;
        }

        public U Callback { get; set; }
        public string ServiceAddress { get; set; }

        public T CreateChannel()
        {

            DuplexChannelFactory<T> channelFactory = null;

            try
            {

                // Basic A, B of the wcf service
                var binding = new BasicHttpBinding();
                var endpointAddress = new EndpointAddress(ServiceAddress);

                // Pass Binding and EndPoint address to ChannelFactory, NB: it has to be DuplexChannel as we expect to be called back 
                channelFactory =
                  new DuplexChannelFactory<T>(
                      new InstanceContext(Callback),
                      new NetTcpBinding(),
                      endpointAddress);

                channelFactory.Faulted += ChannelFactory_Faulted;
                channelFactory.Closed += ChannelFactory_Closed;

                T channel = channelFactory.CreateChannel();
                return channel;

            }
            catch (TimeoutException ex)
            {
                //Timeout error  
                if (channelFactory != null)
                    channelFactory.Abort();

                throw;
            }
            catch (FaultException ex)
            {
                if (channelFactory != null)
                    channelFactory.Abort();

                throw;
            }
            catch (CommunicationException ex)
            {
                //Communication error  
                if (channelFactory != null)
                    channelFactory.Abort();

                throw;
            }
            catch (Exception ex)
            {
                if (channelFactory != null)
                    channelFactory.Abort();

                throw;
            }
        }
            
        private void ChannelFactory_Closed(object sender, EventArgs e)
        {
            HandleExceptionAndRetry(CreateChannel);
        }

        private void ChannelFactory_Faulted(object sender, EventArgs e)
        {
            HandleExceptionAndRetry(CreateChannel);
        }

        private void HandleExceptionAndRetry(Func<T> connectToService)
        {
            // TODO: use polly or something
            Thread.Sleep(1000);
            connectToService();
        }
    }
}
