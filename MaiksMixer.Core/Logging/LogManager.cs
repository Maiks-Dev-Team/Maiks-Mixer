using System;
using System.IO;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace MaiksMixer.Core.Logging
{
    /// <summary>
    /// Manages application logging functionality.
    /// </summary>
    public static class LogManager
    {
        private static readonly Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private static bool _isInitialized;

        /// <summary>
        /// Initializes the logging system.
        /// </summary>
        /// <param name="logDirectory">Directory where log files will be stored.</param>
        public static void Initialize(string? logDirectory = null)
        {
            if (_isInitialized)
                return;

            try
            {
                // If no log directory is specified, use the default location
                if (string.IsNullOrEmpty(logDirectory))
                {
                    logDirectory = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "MaiksMixer",
                        "Logs");
                }

                // Ensure the log directory exists
                Directory.CreateDirectory(logDirectory);

                // Configure NLog programmatically
                var config = new LoggingConfiguration();

                // Create file target for all logs
                var fileTarget = new FileTarget("file")
                {
                    FileName = Path.Combine(logDirectory, "MaiksMixer_${shortdate}.log"),
                    Layout = "${longdate} | ${level:uppercase=true} | ${logger} | ${message} ${exception:format=tostring}",
                    ArchiveFileName = Path.Combine(logDirectory, "archives", "MaiksMixer_{#}.log"),
                    ArchiveNumbering = ArchiveNumberingMode.Date,
                    ArchiveEvery = FileArchivePeriod.Day,
                    MaxArchiveFiles = 7,
                    ConcurrentWrites = true,
                    KeepFileOpen = false
                };

                // Create console target for debugging
                var consoleTarget = new ConsoleTarget("console")
                {
                    Layout = "${time} | ${level:uppercase=true} | ${message} ${exception:format=tostring}"
                };

                // Add targets to configuration
                config.AddTarget(fileTarget);
                config.AddTarget(consoleTarget);

                // Define rules
                config.AddRule(LogLevel.Info, LogLevel.Fatal, fileTarget);
                config.AddRule(LogLevel.Debug, LogLevel.Fatal, consoleTarget);

                // Apply configuration
                NLog.LogManager.Configuration = config;

                _isInitialized = true;
                Info("Logging system initialized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialize logging system: {ex.Message}");
            }
        }

        /// <summary>
        /// Logs a debug message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void Debug(string message) => _logger.Debug(message);

        /// <summary>
        /// Logs an info message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void Info(string message) => _logger.Info(message);

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void Warn(string message) => _logger.Warn(message);

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="exception">The exception associated with the error.</param>
        public static void Error(string message, Exception? exception = null)
        {
            if (exception != null)
                _logger.Error(exception, message);
            else
                _logger.Error(message);
        }

        /// <summary>
        /// Logs a fatal error message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="exception">The exception associated with the fatal error.</param>
        public static void Fatal(string message, Exception? exception = null)
        {
            if (exception != null)
                _logger.Fatal(exception, message);
            else
                _logger.Fatal(message);
        }

        /// <summary>
        /// Shuts down the logging system.
        /// </summary>
        public static void Shutdown()
        {
            if (_isInitialized)
            {
                Info("Logging system shutting down");
                NLog.LogManager.Shutdown();
            }
        }
    }
}
