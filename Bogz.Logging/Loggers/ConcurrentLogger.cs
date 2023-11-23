using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bogz.Logging.Loggers;

/// <summary>
/// Thread-safe logger.
/// </summary>
public class ConcurrentLogger : ILoggerDisposable
{
    private static readonly Lazy<ConcurrentLogger> _logger = new Lazy<ConcurrentLogger>(
        () => new ConcurrentLogger("debug.log"),
        true
    );

    public static ConcurrentLogger Instance
    {
        get => _logger.Value;
    }

    private ConcurrentQueue<LogMessage> logMessages;
    private StreamWriter? writer;
    private Task? task;

    public ConcurrentLogger(string logFile)
    {
        logMessages = new();

        if (logFile.Equals(string.Empty))
            return;
        if (File.Exists(logFile))
            File.Delete(logFile);

        writer = new StreamWriter(
            logFile,
            new FileStreamOptions()
            {
                Access = FileAccess.Write,
                Mode = FileMode.CreateNew,
                Options = FileOptions.Asynchronous
            }
        );
    }

    public virtual void Log(LogLevel level, string message)
    {
        try
        {
            logMessages.Enqueue(new(message, level));
            if (task?.Status == TaskStatus.Running)
                return;

            // TODO yk
            task ??= Task.Factory.StartNew(() => MessageLoggingCallback());
        }
        catch { }
    }

    private void MessageLoggingCallback()
    {
        StringBuilder sb = new();

        while (logMessages.Any())
        {
            if (logMessages.TryDequeue(out var msgQ))
            {
                string msg = $"[{msgQ.Level}] {msgQ.Message}";

                sb.AppendLine(msg);

                var color = msgQ.Level switch
                {
                    LogLevel.Info => ConsoleColor.Gray,
                    LogLevel.Warning => ConsoleColor.Yellow,
                    LogLevel.Error => ConsoleColor.Red,
                    LogLevel.Fatal => ConsoleColor.DarkRed,
                    LogLevel.Success => ConsoleColor.Green,
                    _ => Console.ForegroundColor,
                };

                Console.ForegroundColor = color;
                Console.WriteLine(msg);
                Console.ForegroundColor = ConsoleColor.White;

                if (sb.Length > 100)
                {
                    writer?.Write(sb.ToString());
                    sb.Clear();
                }
            }
        }
        if (sb.Length > 0)
            writer?.Write(sb.ToString());

        task = null;
    }

    public void Log(LogLevel level, object message)
    {
        Log(level, message);
    }

    public void Dispose()
    {
        Log(LogLevel.Info, "[Logger] Disposing self!");
        MessageLoggingCallback();

        Console.WriteLine("Flushing logger.");
        writer?.Flush();
        Console.WriteLine("Disposing logger.");
        writer?.Dispose();
        Console.WriteLine("Logger Finalized.");

        GC.SuppressFinalize(this);
    }
}
