using Serilog;
using System.IO;
using System;

namespace SqlChangeTrackingProducerConsumer
{

    /// <summary>
    /// Information needed to configure the logger
    /// </summary>
    public class LoggingConfiguration : ILoggingConfiguration
    {

        public LoggingConfiguration(LogginInfo loggingInfo)
        {
            LoggingInfo = loggingInfo;
        }

        public LogginInfo LoggingInfo { get; set; }

        public ILogger ConfigureLogger()
        {

            var template = "{MachineName}:{EnvironmentUserName} {Timestamp:dd/MM/yyy HH:mm:ss:mm} [{Level}] ({ThreadId}) {Message}{NewLine}{Exception}";

            return new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.WithThreadId()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentUserName()
                 .WriteTo.Console(
                    outputTemplate: template)
                .WriteTo.File(
                                Path.Combine(LoggingInfo.LogFolder, LoggingInfo.LogFile),
                                rollingInterval: RollingInterval.Day,
                                outputTemplate: template
                              )
                .CreateLogger();
        }
    }

}
