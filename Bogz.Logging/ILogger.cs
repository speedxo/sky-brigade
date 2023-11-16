namespace Bogz.Logging;

public interface ILogger
{
    void Log(LogLevel level, string message);
    void Log(LogLevel level, object message);
}

public interface ILoggerDisposable : IDisposable, ILogger
{

}