using Nerdle.AutoConfig;

namespace SqlChangeTrackingProducerConsumer
{
    class Program
    {

        static void Main(string[] args)
        {
            /*
                This is the standard console application and the starting point of the project
             */
            try
            {
                // This cool util, serializes the app.config configuration section to a class or interface
                var result = AutoConfig.Map<LogginInfo>();
                var logger = new LoggingConfiguration(result).ConfigureLogger();

                // Initialise the service using the Topshelf and configure logging
                // InvoiceWindowsService is the service that is inheriting from the base and doing cool stuff 
                new ServiceConfigurationHelper(new SqlTrackingWindowsService(logger), 
                    new ServiceDependencies
                    {
                         Logger = logger,
                         ServiceInfo = new ServiceInfo()
                         {
                             Description = "Sql Tracking Service",
                             ServiceDisplayName = "Sql Tracking Service",
                             ServiceName = "SqlTrackingService"
                         }
                    }
                    ).Configure();
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                throw;
            }
        }

    }
}
