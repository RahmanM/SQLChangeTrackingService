namespace ServiceTopShelf
{
    public interface ILoggingConfiguration
    {
        string LogFile { get; set; }
        string LogFolder { get; set; }
    }
}