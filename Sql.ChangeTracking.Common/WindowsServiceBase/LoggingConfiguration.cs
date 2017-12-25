namespace ServiceTopShelf
{

    /// <summary>
    /// Information needed to configure the logger
    /// </summary>
    public class LoggingConfiguration : ILoggingConfiguration
    {
        public string LogFile { get; set; }
        public string LogFolder { get; set; }
    }
   
}
