using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using MaiksMixer.Core.Logging;

namespace MaiksMixer.Core.Diagnostics
{
    /// <summary>
    /// Provides diagnostic functionality for the application.
    /// </summary>
    public class DiagnosticService
    {
        private readonly string _applicationPath;
        private readonly string _diagnosticFilePath;

        /// <summary>
        /// Initializes a new instance of the DiagnosticService class.
        /// </summary>
        public DiagnosticService()
        {
            _applicationPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
            _diagnosticFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MaiksMixer",
                "Diagnostics");

            // Ensure the diagnostic directory exists
            Directory.CreateDirectory(_diagnosticFilePath);
        }

        /// <summary>
        /// Collects system information and logs it.
        /// </summary>
        public void CollectSystemInfo()
        {
            try
            {
                var info = new StringBuilder();
                info.AppendLine("=== SYSTEM INFORMATION ===");
                info.AppendLine($"OS: {RuntimeInformation.OSDescription}");
                info.AppendLine($"Architecture: {RuntimeInformation.OSArchitecture}");
                info.AppendLine($"Framework: {RuntimeInformation.FrameworkDescription}");
                info.AppendLine($"Process Architecture: {RuntimeInformation.ProcessArchitecture}");
                info.AppendLine($"Application Path: {_applicationPath}");
                info.AppendLine($"Available Memory: {GetAvailableMemory()} MB");
                info.AppendLine($"Processor Count: {Environment.ProcessorCount}");
                info.AppendLine($"User Name: {Environment.UserName}");
                info.AppendLine($"Machine Name: {Environment.MachineName}");
                info.AppendLine($"64-bit OS: {Environment.Is64BitOperatingSystem}");
                info.AppendLine($"64-bit Process: {Environment.Is64BitProcess}");
                info.AppendLine($"Current Directory: {Environment.CurrentDirectory}");
                info.AppendLine($"System Directory: {Environment.SystemDirectory}");
                info.AppendLine($"Current Process ID: {Process.GetCurrentProcess().Id}");
                info.AppendLine($"Current Process Name: {Process.GetCurrentProcess().ProcessName}");
                info.AppendLine($"Current Process Memory Usage: {Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024} MB");

                // Log the system information
                LogManager.Info(info.ToString());

                // Save to diagnostic file
                var filePath = Path.Combine(_diagnosticFilePath, $"SystemInfo_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
                File.WriteAllText(filePath, info.ToString());
            }
            catch (Exception ex)
            {
                LogManager.Error("Failed to collect system information", ex);
            }
        }

        /// <summary>
        /// Gets the available system memory in MB.
        /// </summary>
        private long GetAvailableMemory()
        {
            try
            {
                // Use GC information instead of PerformanceCounter which requires additional package
                var totalMemory = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / (1024 * 1024);
                return Convert.ToInt64(totalMemory);
            }
            catch (Exception ex)
            {
                LogManager.Error("Failed to get available memory", ex);
                return -1;
            }
        }

        /// <summary>
        /// Generates a diagnostic report with detailed information about the application state.
        /// </summary>
        /// <returns>The path to the generated diagnostic report.</returns>
        public async Task<string> GenerateDiagnosticReportAsync()
        {
            try
            {
                LogManager.Info("Generating diagnostic report...");

                var reportBuilder = new StringBuilder();
                reportBuilder.AppendLine("=== MAIKS MIXER DIAGNOSTIC REPORT ===");
                reportBuilder.AppendLine($"Generated: {DateTime.Now}");
                reportBuilder.AppendLine();

                // System Information
                reportBuilder.AppendLine("=== SYSTEM INFORMATION ===");
                reportBuilder.AppendLine($"OS: {RuntimeInformation.OSDescription}");
                reportBuilder.AppendLine($"Architecture: {RuntimeInformation.OSArchitecture}");
                reportBuilder.AppendLine($"Framework: {RuntimeInformation.FrameworkDescription}");
                reportBuilder.AppendLine($"Process Architecture: {RuntimeInformation.ProcessArchitecture}");
                reportBuilder.AppendLine($"Available Memory: {GetAvailableMemory()} MB");
                reportBuilder.AppendLine($"Processor Count: {Environment.ProcessorCount}");
                reportBuilder.AppendLine();

                // Application Information
                reportBuilder.AppendLine("=== APPLICATION INFORMATION ===");
                var assembly = Assembly.GetExecutingAssembly();
                reportBuilder.AppendLine($"Application Version: {assembly.GetName().Version}");
                reportBuilder.AppendLine($"Application Path: {_applicationPath}");
                reportBuilder.AppendLine($"Process Memory Usage: {Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024} MB");
                reportBuilder.AppendLine($"Process CPU Time: {Process.GetCurrentProcess().TotalProcessorTime}");
                reportBuilder.AppendLine($"Process Threads: {Process.GetCurrentProcess().Threads.Count}");
                reportBuilder.AppendLine($"Process Start Time: {Process.GetCurrentProcess().StartTime}");
                reportBuilder.AppendLine();

                // Running Processes
                reportBuilder.AppendLine("=== RUNNING PROCESSES ===");
                foreach (var process in Process.GetProcesses())
                {
                    try
                    {
                        reportBuilder.AppendLine($"{process.ProcessName} (ID: {process.Id}, Memory: {process.WorkingSet64 / 1024 / 1024} MB)");
                    }
                    catch
                    {
                        // Skip processes we can't access
                    }
                }
                reportBuilder.AppendLine();

                // Installed Audio Devices (placeholder - would need to be implemented with actual audio device enumeration)
                reportBuilder.AppendLine("=== AUDIO DEVICES ===");
                reportBuilder.AppendLine("Audio device enumeration not yet implemented");
                reportBuilder.AppendLine();

                // Recent Log Entries
                reportBuilder.AppendLine("=== RECENT LOG ENTRIES ===");
                var logDirectory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "MaiksMixer",
                    "Logs");
                
                var logFiles = Directory.GetFiles(logDirectory, "MaiksMixer_*.log");
                if (logFiles.Length > 0)
                {
                    // Get the most recent log file
                    var mostRecentLog = logFiles[0];
                    DateTime mostRecentDate = File.GetLastWriteTime(mostRecentLog);
                    
                    foreach (var logFile in logFiles)
                    {
                        var fileDate = File.GetLastWriteTime(logFile);
                        if (fileDate > mostRecentDate)
                        {
                            mostRecentLog = logFile;
                            mostRecentDate = fileDate;
                        }
                    }
                    
                    // Read the last 50 lines from the log file
                    var logLines = await File.ReadAllLinesAsync(mostRecentLog);
                    var recentLines = logLines.Length <= 50 ? logLines : logLines[(logLines.Length - 50)..];
                    
                    foreach (var line in recentLines)
                    {
                        reportBuilder.AppendLine(line);
                    }
                }
                else
                {
                    reportBuilder.AppendLine("No log files found");
                }

                // Save the report
                var reportFileName = $"DiagnosticReport_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                var reportPath = Path.Combine(_diagnosticFilePath, reportFileName);
                await File.WriteAllTextAsync(reportPath, reportBuilder.ToString());

                LogManager.Info($"Diagnostic report generated: {reportPath}");
                return reportPath;
            }
            catch (Exception ex)
            {
                LogManager.Error("Failed to generate diagnostic report", ex);
                return string.Empty;
            }
        }
    }
}
