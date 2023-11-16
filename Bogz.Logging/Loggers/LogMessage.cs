namespace Bogz.Logging.Loggers;

public struct LogMessage
{
    public LogMessage(string message, LogLevel level)
    {
        Message = message;
        Level = level;
    }

    public string Message { get; set; }
    public LogLevel Level { get; set; }
}
