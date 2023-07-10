using System.Diagnostics;
using System.Globalization;

namespace SkyBrigade.Engine.Logging;

public class Logger : IDisposable
{
    // the different colors for each level of logging
    public static Dictionary<LogLevel, ConsoleColor> Colors { get; } = new Dictionary<LogLevel, ConsoleColor>()
    {
        { LogLevel.Debug, ConsoleColor.Gray },
        { LogLevel.Info, ConsoleColor.White },
        { LogLevel.Warning, ConsoleColor.Yellow },
        { LogLevel.Error, ConsoleColor.Red },
        { LogLevel.Fatal, ConsoleColor.DarkRed }
    };

    // the different prefixes for each level of logging
    public static Dictionary<LogLevel, string> Prefixes { get; } = new Dictionary<LogLevel, string>()
    {
        { LogLevel.Debug, "[DEBUG] " },
        { LogLevel.Info, "[INFO] " },
        { LogLevel.Warning, "[WARNING] " },
        { LogLevel.Error, "[ERROR] " },
        { LogLevel.Fatal, "[FATAL] " }
    };

    // the different suffixes for each level of logging
    public static Dictionary<LogLevel, string> Suffixes { get; } = new Dictionary<LogLevel, string>()
    {
        { LogLevel.Debug, "" },
        { LogLevel.Info, "" },
        { LogLevel.Warning, "" },
        { LogLevel.Error, "" },
        { LogLevel.Fatal, "" }
    };

    // the different prefixes for each level of logging
    public static Dictionary<LogLevel, string> PrefixesNoColor { get; } = new Dictionary<LogLevel, string>()
    {
        { LogLevel.Debug, "[DEBUG] " },
        { LogLevel.Info, "[INFO] " },
        { LogLevel.Warning, "[WARNING] " },
        { LogLevel.Error, "[ERROR] " },
        { LogLevel.Fatal, "[FATAL] " }
    };

    public LogOutput Output {get; private set; }
    private TextWriter textWriter;

    public Logger(LogOutput output=LogOutput.Console)
    {
        Output = output;
        if (output == 0) return;

        textWriter = new StreamWriter("log.txt", true);
    }

    public void Log(LogLevel level, string message)
    {
        // only log debug level events if we are in debug mode
        if (level == LogLevel.Debug && !Debugger.IsAttached) return;

        string prefix = Prefixes[level];
        string suffix = Suffixes[level];
        ConsoleColor color = Colors[level];

        string log = $"{prefix}{message}{suffix}";
        Console.ForegroundColor = color;
        Console.WriteLine(log);
        Console.ResetColor();

        if (Output > 0)
            textWriter.WriteLine(log);        
    }

    public void Dispose()
    {
        if (Output == 0) return;

        textWriter.Dispose();
        GC.SuppressFinalize(this);
    }
}
