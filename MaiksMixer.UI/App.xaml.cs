using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MaiksMixer.Core.Communication;
using MaiksMixer.Core.Diagnostics;
using MaiksMixer.Core.Logging;
using MaiksMixer.UI.Services;
using MaiksMixer.UI.Services.Audio;
using MaiksMixer.UI.ViewModels;
using MaiksMixer.UI.Views;

namespace MaiksMixer.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private SystemTrayService? _systemTrayService;
        private DiagnosticService? _diagnosticService;
        private CommunicationService? _communicationService;
        private CommandDispatcher? _commandDispatcher;
        private SharedMemoryManager? _sharedMemoryManager;
        private DateTime _startTime = DateTime.Now;
        
        /// <summary>
        /// Handles the application startup event.
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize logging system
            LogManager.Initialize();
            LogManager.Info("Application starting");
            
            try
            {
                // Initialize diagnostic service
                _diagnosticService = new DiagnosticService();
                _diagnosticService.CollectSystemInfo();
                LogManager.Info("Diagnostic service initialized");
                
                // Create main window
                var mainWindow = new MainWindow();
                LogManager.Info("Main window created");
                
                // Create the JackAudioViewModel
                var jackAudioService = JackAudioServiceProvider.CreateJackAudioService();
                var jackAudioViewModel = new JackAudioViewModel(jackAudioService);
                LogManager.Info("JackAudioViewModel created");
                
                // Find the JackAudioTab and set its DataContext
                if (mainWindow.FindName("JackAudioTab") is TabItem jackAudioTab)
                {
                    var jackAudioView = jackAudioTab.Content as JackAudioView;
                    if (jackAudioView != null)
                    {
                        jackAudioView.DataContext = jackAudioViewModel;
                        LogManager.Info("JackAudioView DataContext set");
                    }
                }
                
                mainWindow.Show();
                LogManager.Info("Main window shown");

                // Initialize system tray service
                _systemTrayService = new SystemTrayService(mainWindow);
                LogManager.Info("System tray service initialized");
                
                // Initialize command dispatcher
                _commandDispatcher = new CommandDispatcher();
                RegisterCommandHandlers();
                LogManager.Info("Command dispatcher initialized");
                
                // Initialize communication service
                _communicationService = new CommunicationService();
                _communicationService.CommandReceived += OnCommandReceived;
                _communicationService.ClientConnected += OnClientConnected;
                _communicationService.ClientDisconnected += OnClientDisconnected;

                _communicationService.Start();
                LogManager.Info("Communication service started");
                
                // Initialize shared memory manager
                _sharedMemoryManager = new SharedMemoryManager();
                _sharedMemoryManager.Initialize();
                LogManager.Info("Shared memory manager initialized");
                
                // Write initial data to shared memory
                var initialData = "MaiksMixer started at " + DateTime.Now.ToString();
                _sharedMemoryManager.WriteData(initialData);
                LogManager.Info("Initial data written to shared memory");

                // Handle application exit
                Exit += App_Exit;
                
                // Log successful startup
                LogManager.Info("Application startup completed successfully");
            }
            catch (Exception ex)
            {
                LogManager.Fatal("Application startup failed", ex);
                System.Windows.MessageBox.Show($"Application failed to start: {ex.Message}", "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(-1);
            }
        }

        /// <summary>
        /// Handles the application exit event.
        /// </summary>
        private void App_Exit(object sender, ExitEventArgs e)
        {
            try
            {
                LogManager.Info("Application shutting down");
                
                // Clean up system tray icon
                _systemTrayService?.Dispose();
                LogManager.Info("System tray service disposed");
                
                // Clean up communication service
                if (_communicationService != null)
                {
                    _communicationService.CommandReceived -= OnCommandReceived;
                    _communicationService.ClientConnected -= OnClientConnected;
                    _communicationService.ClientDisconnected -= OnClientDisconnected;
                    _communicationService.Dispose();
                    LogManager.Info("Communication service disposed");
                }
                
                // Clean up shared memory manager
                if (_sharedMemoryManager != null)
                {
                    _sharedMemoryManager.Dispose();
                    LogManager.Info("Shared memory manager disposed");
                }
                
                // Generate final diagnostic report if needed
                if (_diagnosticService != null)
                {
                    var reportTask = _diagnosticService.GenerateDiagnosticReportAsync();
                    reportTask.Wait(); // Wait for the report to be generated before exiting
                    LogManager.Info($"Final diagnostic report generated: {reportTask.Result}");
                }
                
                // Shutdown logging system
                LogManager.Info("Application shutdown complete");
                LogManager.Shutdown();
            }
            catch (Exception ex)
            {
                // Use Console.WriteLine as a fallback since logging might have failed
                Console.WriteLine($"Error during application shutdown: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Handles commands received from clients.
        /// </summary>
        private void OnCommandReceived(object? sender, CommandMessage e)
        {
            try
            {
                LogManager.Info($"Processing command: {e.Command}");
                
                if (_commandDispatcher != null)
                {
                    // Use Task.Run to handle the async operation in a non-async event handler
                    Task.Run(async () => 
                    {
                        bool handled = await _commandDispatcher.DispatchCommandAsync(e);
                        
                        if (!handled)
                        {
                            LogManager.Warn($"Command not handled: {e.Command}");
                        }
                    });
                }
                else
                {
                    LogManager.Error("Command dispatcher not initialized");
                }
            }
            catch (Exception ex)
            {
                LogManager.Error($"Error processing command: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Handles client connection events.
        /// </summary>
        private void OnClientConnected(object? sender, string clientId)
        {
            LogManager.Info($"Client connected: {clientId}");
        }
        
        /// <summary>
        /// Handles client disconnection events.
        /// </summary>
        private void OnClientDisconnected(object? sender, string clientId)
        {
            LogManager.Info($"Client disconnected: {clientId}");
        }
        
        /// <summary>
        /// Registers command handlers with the command dispatcher.
        /// </summary>
        private void RegisterCommandHandlers()
        {
            if (_commandDispatcher == null)
                return;
                
            // Register command handlers
            _commandDispatcher.RegisterHandler("GetStatus", HandleGetStatusCommand);
            _commandDispatcher.RegisterHandler("ShowWindow", HandleShowWindowCommand);
            _commandDispatcher.RegisterHandler("HideWindow", HandleHideWindowCommand);
            _commandDispatcher.RegisterHandler("Exit", HandleExitCommand);
            _commandDispatcher.RegisterHandler("GetDiagnostics", HandleGetDiagnosticsCommand);
        }
        
        /// <summary>
        /// Handles the GetStatus command.
        /// </summary>
        private async Task HandleGetStatusCommand(CommandMessage command)
        {
            LogManager.Info("Handling GetStatus command");
            
            if (_communicationService != null)
            {
                var status = new StatusMessage();
                status.Status = "Running";
                status.Data = new
                {
                    StartTime = _startTime,
                    Uptime = DateTime.Now - _startTime,
                    Version = "1.0.0",
                    IsVisible = MainWindow?.IsVisible ?? false
                };
                
                await _communicationService.SendStatusUpdateAsync(status);
            }
        }
        
        /// <summary>
        /// Handles the ShowWindow command.
        /// </summary>
        private Task HandleShowWindowCommand(CommandMessage command)
        {
            LogManager.Info("Handling ShowWindow command");
            
            Dispatcher.Invoke(() =>
            {
                if (MainWindow != null)
                {
                    MainWindow.Show();
                    MainWindow.WindowState = WindowState.Normal;
                    MainWindow.Activate();
                }
            });
            
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Handles the HideWindow command.
        /// </summary>
        private Task HandleHideWindowCommand(CommandMessage command)
        {
            LogManager.Info("Handling HideWindow command");
            
            Dispatcher.Invoke(() =>
            {
                if (MainWindow != null)
                {
                    MainWindow.Hide();
                }
            });
            
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Handles the Exit command.
        /// </summary>
        private Task HandleExitCommand(CommandMessage command)
        {
            LogManager.Info("Handling Exit command");
            
            Dispatcher.Invoke(() =>
            {
                Shutdown();
            });
            
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Handles the GetDiagnostics command.
        /// </summary>
        private async Task HandleGetDiagnosticsCommand(CommandMessage command)
        {
            LogManager.Info("Handling GetDiagnostics command");
            
            if (_diagnosticService != null && _communicationService != null)
            {
                // Use the async version of the diagnostic report generation
                string reportPath = await _diagnosticService.GenerateDiagnosticReportAsync();
                
                // Create a status message with the report path
                var status = new StatusMessage();
                status.Status = "Diagnostics";
                status.Data = new { ReportPath = reportPath };
                
                await _communicationService.SendStatusUpdateAsync(status);
            }
        }
    }
}
