using Horizon.GameEntity;
using Horizon.GameEntity.Components;
using Horizon.Rendering;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Horizon.Logging;

/// <summary>
/// The Logger class is responsible for handling logging messages.
/// </summary>
public class Logger : IGameComponent
{
    private readonly object locker = new();

    private static readonly Dictionary<LogLevel, ConsoleColor> Colors =
        new()
        {
            { LogLevel.Debug, ConsoleColor.Gray },
            { LogLevel.Info, ConsoleColor.White },
            { LogLevel.Warning, ConsoleColor.Yellow },
            { LogLevel.Error, ConsoleColor.Red },
            { LogLevel.Fatal, ConsoleColor.DarkRed }
        };

    private static readonly Dictionary<LogLevel, string> Prefixes =
        new()
        {
            { LogLevel.Debug, "[DEBUG] " },
            { LogLevel.Info, "[INFO] " },
            { LogLevel.Warning, "[WARNING] " },
            { LogLevel.Error, "[ERROR] " },
            { LogLevel.Fatal, "[FATAL] " }
        };

    private static readonly Dictionary<LogLevel, string> Suffixes =
        new()
        {
            { LogLevel.Debug, "" },
            { LogLevel.Info, "" },
            { LogLevel.Warning, "" },
            { LogLevel.Error, "" },
            { LogLevel.Fatal, "" }
        };

    /// <summary>
    /// Gets or sets the output destination for the logs.
    /// </summary>
    public LogOutput Output { get; private set; }
    public string Name { get; set; } = "Logger";
    public Entity Parent { get; set; }

    private readonly TextWriter textWriter;
    private ConcurrentQueue<LogEntry> logQueue = new();
    private bool isProcessingLogs = false;

    /// <summary>
    /// Initializes a new instance of the Logger class with the specified output destination.
    /// </summary>
    /// <param name="output">The output destination for the logs.</param>
    public Logger(LogOutput output = LogOutput.Console)
    {
        Output = output;
        if (output > 0)
            textWriter = new StreamWriter("log.txt", true);
    }

    public void UpdatePhysics(float dt) { }

    /// <summary>
    /// Logs a message with the specified log level.
    /// </summary>
    /// <param name="level">The log level of the message.</param>
    /// <param name="message">The message to log.</param>
    public void Log(LogLevel level, string message)
    {
        //if (level == LogLevel.Debug && !Debugger.IsAttached)
        //    return;

        string prefix = Prefixes[level];
        string suffix = Suffixes[level];
        ConsoleColor color = Colors[level];

        string log = $"{prefix}{message}{suffix}";
        //lock (locker)
        {
            logQueue.Enqueue(new LogEntry(log, color));
        }
    }

    /// <summary>
    /// Disposes the resources used by the Logger.
    /// </summary>
    public void Dispose()
    {
        // :-)
        while (isProcessingLogs)
        {
            Thread.Sleep(10);
        }

        if (Output > 0)
        {
            textWriter.Flush();
            textWriter.Dispose();
        }

        GC.SuppressFinalize(this);
    }

    public void Initialize() { }

    public void UpdateState(float dt)
    {
        if (!logQueue.IsEmpty)
        {
            lock (locker)
            {
                isProcessingLogs = true;
                while (logQueue.TryDequeue(out LogEntry logEntry))
                {
                    Console.ForegroundColor = logEntry.Color;
                    Console.WriteLine(logEntry.Message);
                    Console.ResetColor();

                    try
                    {
                        if (Output > 0)
                            textWriter.WriteLine(logEntry.Message);
                    }
                    catch { }
                }
                isProcessingLogs = false;
            }
        }
    }

    public void Render(float dt, ref RenderOptions options) { }

    // Helper struct to store log entry information
    private readonly struct LogEntry
    {
        public readonly string Message { get; }
        public readonly ConsoleColor Color { get; }

        public LogEntry(string message, ConsoleColor color)
        {
            Message = message;
            Color = color;
        }
    }
}
