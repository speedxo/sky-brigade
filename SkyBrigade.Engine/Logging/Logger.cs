using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;

namespace SkyBrigade.Engine.Logging
{
    public class Logger : IDisposable
    {
        // Define a private object for thread synchronization
        private readonly object locker = new object();

        // the different colors for each level of logging
        private static readonly Dictionary<LogLevel, ConsoleColor> Colors = new Dictionary<LogLevel, ConsoleColor>()
        {
            { LogLevel.Debug, ConsoleColor.Gray },
            { LogLevel.Info, ConsoleColor.White },
            { LogLevel.Warning, ConsoleColor.Yellow },
            { LogLevel.Error, ConsoleColor.Red },
            { LogLevel.Fatal, ConsoleColor.DarkRed }
        };

        // the different prefixes for each level of logging
        private static readonly Dictionary<LogLevel, string> Prefixes = new Dictionary<LogLevel, string>()
        {
            { LogLevel.Debug, $"[DEBUG] " },
            { LogLevel.Info, "[INFO] " },
            { LogLevel.Warning, "[WARNING] " },
            { LogLevel.Error, "[ERROR] " },
            { LogLevel.Fatal, "[FATAL] " }
        };

        // the different suffixes for each level of logging
        private static readonly Dictionary<LogLevel, string> Suffixes = new Dictionary<LogLevel, string>()
        {
            { LogLevel.Debug, "" },
            { LogLevel.Info, "" },
            { LogLevel.Warning, "" },
            { LogLevel.Error, "" },
            { LogLevel.Fatal, "" }
        };

        // the different prefixes for each level of logging
        private static readonly Dictionary<LogLevel, string> PrefixesNoColor = new Dictionary<LogLevel, string>()
        {
            { LogLevel.Debug, "[DEBUG] " },
            { LogLevel.Info, "[INFO] " },
            { LogLevel.Warning, "[WARNING] " },
            { LogLevel.Error, "[ERROR] " },
            { LogLevel.Fatal, "[FATAL] " }
        };

        public LogOutput Output { get; private set; }
        private TextWriter textWriter;
        private ConcurrentQueue<LogEntry> logQueue = new ConcurrentQueue<LogEntry>();
        private bool isProcessingLogs = false;

        public Logger(LogOutput output = LogOutput.Console)
        {
            Output = output;
            if (output > 0)
                textWriter = new StreamWriter("log.txt", true);

            // Start a background thread to process logs from the queue
            // (i ADORE discards)
            _ = ThreadPool.QueueUserWorkItem(ProcessLogs);
        }

        public void Log(LogLevel level, string message)
        {
            // only log debug level events if we are in debug mode
            if (level == LogLevel.Debug && !Debugger.IsAttached) return;

            string prefix = Prefixes[level];
            string suffix = Suffixes[level];
            ConsoleColor color = Colors[level];

            string log = $"{prefix}{message}{suffix}";

            // Enqueue the log entry for processing
            logQueue.Enqueue(new LogEntry(log, color));

            if (level == LogLevel.Fatal)
            {
                Dispose();
                throw new Exception(message);
            }
        }

        private void ProcessLogs(object state)
        {
            // This method is executed on a background thread
            while (true)
            {
                // If there are logs in the queue, process them
                if (logQueue.Count > 0)
                {
                    lock (locker) // Ensure only one thread writes to the console or file
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
                            catch
                            {

                            }
                        }
                        isProcessingLogs = false;
                    }
                }
                else
                {
                    // If there are no logs to process, wait for a short duration
                    // to prevent busy-waiting and excessive CPU usage
                    Thread.Sleep(10);
                }
            }
        }

        public void Dispose()
        {
            // Wait for logs to finish processing before disposing the resources
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

        // Helper struct to store log entry information
        private readonly struct LogEntry
        {
            public string Message { get; }
            public ConsoleColor Color { get; }

            public LogEntry(string message, ConsoleColor color)
            {
                Message = message;
                Color = color;
            }
        }
    }
}
