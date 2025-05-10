using System;
using System.IO;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace MaiksMixer.Core.Logging
{
    /// <summary>
    /// Provides logging functionality for the application.
    /// </summary>
    public class LoggingService
    {
        private static readonly Lazy<LoggingService> _instance = new Lazy<LoggingService>(() => new LoggingService());
        private readonly Logger _logger;

        /// <summary>
        /// Gets the singleton instance of the LoggingService.
        /// </summary>
        public static LoggingService Instance => _instance.Value;

        /// <summary>
        /// Initializes a new instance of the LoggingService class.
        /// </summary>
        private LoggingService()
        {
            // Configure NLog
            var config = new LoggingConfiguration();

            // Create targets
            var fileTarget = new FileTarget("file")
            {
                FileName = Path.Combine(GetLogDirectory(), "MaiksMixer_${shortdate}.log"),
                Layout = "${longdate} | ${level:uppercase=true:padding=-5} | ${logger} | ${message} ${exception:format=tostring}",
                ArchiveFileName = Path.Combine(GetLogDirectory(), "archives", "MaiksMixer_{#}.log"),
                ArchiveNumbering = ArchiveNumberingMode.Date,
                ArchiveEvery = FileArchivePeriod.Day,
                MaxArchiveFiles = 7,
                ConcurrentWrites = true,
                KeepFileOpen = false
            };

            var consoleTarget = new ConsoleTarget("console")
            {
                Layout = "${time} | ${level:uppercase=true:padding=-5} | ${message} ${exception:format=tostring}"
            };

            // Add targets to configuration
            config.AddTarget(fileTarget);
            config.AddTarget(consoleTarget);

            // Add rules
            config.AddRule(LogLevel.Info, LogLevel.Fatal, consoleTarget);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, fileTarget);

            // Apply configuration
            LogManager.Configuration = config;

            // Get logger
            _logger = LogManager.GetCurrentClassLogger();
            
            Info("Logging service initialized");
        }

        /// <summary>
        /// Gets the directory where log files are stored.
        /// </summary>
        private string GetLogDirectory()
        {
            var logDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MaiksMixer",
                "Logs");

            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            var archiveDirectory = Path.Combine(logDirectory, "archives");
            if (!Directory.Exists(archiveDirectory))
            {
                Directory.CreateDirectory(archiveDirectory);
            }

            return logDirectory;
        }

        /// <summary>
        /// Logs a debug message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void Debug(string message)
        {
            _logger.Debug(message);
        }

        /// <summary>
        /// Logs a debug message with an exception.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="exception">The exception to log.</param>
        public void Debug(string message, Exception exception)
        {
            _logger.Debug(exception, message);
        }

        /// <summary>
        /// Logs an info message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void Info(string message)
        {
            _logger.Info(message);
        }

        /// <summary>
        /// Logs an info message with an exception.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="exception">The exception to log.</param>
        public void Info(string message, Exception exception)
        {
            _logger.Info(exception, message);
        }

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void Warn(string message)
        {
            _logger.Warn(message);
        }

        /// <summary>
        /// Logs a warning message with an exception.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="exception">The exception to log.</param>
        public void Warn(string message, Exception exception)
        {
            _logger.Warn(exception, message);
        }

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void Error(string message)
        {
            _logger.Error(message);
        }

        /// <summary>
        /// Logs an error message with an exception.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="exception">The exception to log.</param>
        public void Error(string message, Exception exception)
        {
            _logger.Error(exception, message);
        }

        /// <summary>
        /// Logs a fatal message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void Fatal(string message)
        {
            _logger.Fatal(message);
        }

        /// <summary>
        /// Logs a fatal message with an exception.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="exception">The exception to log.</param>
        public void Fatal(string message, Exception exception)
        {
            _logger.Fatal(exception, message);
        }
    }
}
