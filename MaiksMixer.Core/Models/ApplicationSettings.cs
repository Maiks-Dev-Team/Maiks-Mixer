using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MaiksMixer.Core.Models
{
    /// <summary>
    /// Represents the application settings.
    /// </summary>
    public class ApplicationSettings
    {
        private static readonly string SettingsFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "MaiksMixer",
            "settings.json");
            
        /// <summary>
        /// Gets or sets the audio settings.
        /// </summary>
        public AudioSettings Audio { get; set; } = new AudioSettings();
        
        /// <summary>
        /// Gets or sets the UI settings.
        /// </summary>
        public UISettings UI { get; set; } = new UISettings();
        
        /// <summary>
        /// Gets or sets the system settings.
        /// </summary>
        public SystemSettings System { get; set; } = new SystemSettings();
        
        /// <summary>
        /// Gets or sets the JACK settings.
        /// </summary>
        public JackSettings Jack { get; set; } = new JackSettings();
        
        /// <summary>
        /// Gets or sets the MIDI settings.
        /// </summary>
        public MidiSettings Midi { get; set; } = new MidiSettings();
        
        /// <summary>
        /// Gets or sets the presets.
        /// </summary>
        public List<PresetInfo> Presets { get; set; } = new List<PresetInfo>();
        
        /// <summary>
        /// Gets or sets the last opened preset.
        /// </summary>
        public string LastOpenedPreset { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the version of the settings file.
        /// </summary>
        public string Version { get; set; } = "1.0";
        
        /// <summary>
        /// Gets or sets the last modified date.
        /// </summary>
        public DateTime LastModified { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Loads the settings from the settings file.
        /// </summary>
        /// <returns>The loaded settings, or default settings if the file doesn't exist.</returns>
        public static ApplicationSettings Load()
        {
            try
            {
                // Ensure directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(SettingsFilePath));
                
                // Check if file exists
                if (File.Exists(SettingsFilePath))
                {
                    // Read file
                    string json = File.ReadAllText(SettingsFilePath);
                    
                    // Deserialize
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        ReadCommentHandling = JsonCommentHandling.Skip,
                        AllowTrailingCommas = true
                    };
                    
                    var settings = JsonSerializer.Deserialize<ApplicationSettings>(json, options);
                    
                    // Return settings or default if null
                    return settings ?? new ApplicationSettings();
                }
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error loading settings: {ex.Message}");
            }
            
            // Return default settings
            return new ApplicationSettings();
        }
        
        /// <summary>
        /// Saves the settings to the settings file.
        /// </summary>
        /// <returns>True if the settings were saved successfully, false otherwise.</returns>
        public bool Save()
        {
            try
            {
                // Update last modified date
                LastModified = DateTime.Now;
                
                // Ensure directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(SettingsFilePath));
                
                // Serialize
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                
                string json = JsonSerializer.Serialize(this, options);
                
                // Write file
                File.WriteAllText(SettingsFilePath, json);
                
                return true;
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error saving settings: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Resets the settings to default values.
        /// </summary>
        public void Reset()
        {
            Audio = new AudioSettings();
            UI = new UISettings();
            System = new SystemSettings();
            Jack = new JackSettings();
            Midi = new MidiSettings();
            Presets = new List<PresetInfo>();
            LastOpenedPreset = string.Empty;
            Version = "1.0";
            LastModified = DateTime.Now;
        }
    }
    
    /// <summary>
    /// Represents the audio settings.
    /// </summary>
    public class AudioSettings
    {
        /// <summary>
        /// Gets or sets the default sample rate.
        /// </summary>
        public int DefaultSampleRate { get; set; } = 48000;
        
        /// <summary>
        /// Gets or sets the default buffer size.
        /// </summary>
        public int DefaultBufferSize { get; set; } = 1024;
        
        /// <summary>
        /// Gets or sets the default bit depth.
        /// </summary>
        public int DefaultBitDepth { get; set; } = 24;
        
        /// <summary>
        /// Gets or sets whether to use ASIO if available.
        /// </summary>
        public bool UseAsioIfAvailable { get; set; } = true;
        
        /// <summary>
        /// Gets or sets whether to use WASAPI exclusive mode.
        /// </summary>
        public bool UseWasapiExclusiveMode { get; set; } = false;
        
        /// <summary>
        /// Gets or sets the default input device ID.
        /// </summary>
        public string DefaultInputDeviceId { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the default output device ID.
        /// </summary>
        public string DefaultOutputDeviceId { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets whether to auto-connect devices.
        /// </summary>
        public bool AutoConnectDevices { get; set; } = true;
        
        /// <summary>
        /// Gets or sets the meter update interval in milliseconds.
        /// </summary>
        public int MeterUpdateInterval { get; set; } = 50;
        
        /// <summary>
        /// Gets or sets the meter decay rate.
        /// </summary>
        public double MeterDecayRate { get; set; } = 0.9;
        
        /// <summary>
        /// Gets or sets the meter hold time in milliseconds.
        /// </summary>
        public int MeterHoldTime { get; set; } = 1000;
    }
    
    /// <summary>
    /// Represents the UI settings.
    /// </summary>
    public class UISettings
    {
        /// <summary>
        /// Gets or sets the theme.
        /// </summary>
        public string Theme { get; set; } = "Dark";
        
        /// <summary>
        /// Gets or sets the accent color.
        /// </summary>
        public string AccentColor { get; set; } = "#0078D7";
        
        /// <summary>
        /// Gets or sets the font size.
        /// </summary>
        public int FontSize { get; set; } = 12;
        
        /// <summary>
        /// Gets or sets whether to show tooltips.
        /// </summary>
        public bool ShowTooltips { get; set; } = true;
        
        /// <summary>
        /// Gets or sets whether to show the system tray icon.
        /// </summary>
        public bool ShowSystemTrayIcon { get; set; } = true;
        
        /// <summary>
        /// Gets or sets whether to minimize to system tray.
        /// </summary>
        public bool MinimizeToSystemTray { get; set; } = true;
        
        /// <summary>
        /// Gets or sets whether to start minimized.
        /// </summary>
        public bool StartMinimized { get; set; } = false;
        
        /// <summary>
        /// Gets or sets whether to show the main window on startup.
        /// </summary>
        public bool ShowMainWindowOnStartup { get; set; } = true;
        
        /// <summary>
        /// Gets or sets whether to show level meters.
        /// </summary>
        public bool ShowLevelMeters { get; set; } = true;
        
        /// <summary>
        /// Gets or sets whether to show peak indicators.
        /// </summary>
        public bool ShowPeakIndicators { get; set; } = true;
        
        /// <summary>
        /// Gets or sets whether to show the routing matrix.
        /// </summary>
        public bool ShowRoutingMatrix { get; set; } = true;
        
        /// <summary>
        /// Gets or sets the main window width.
        /// </summary>
        public int MainWindowWidth { get; set; } = 1024;
        
        /// <summary>
        /// Gets or sets the main window height.
        /// </summary>
        public int MainWindowHeight { get; set; } = 768;
    }
    
    /// <summary>
    /// Represents the system settings.
    /// </summary>
    public class SystemSettings
    {
        /// <summary>
        /// Gets or sets whether to start with Windows.
        /// </summary>
        public bool StartWithWindows { get; set; } = false;
        
        /// <summary>
        /// Gets or sets whether to check for updates on startup.
        /// </summary>
        public bool CheckForUpdatesOnStartup { get; set; } = true;
        
        /// <summary>
        /// Gets or sets whether to automatically install updates.
        /// </summary>
        public bool AutoInstallUpdates { get; set; } = false;
        
        /// <summary>
        /// Gets or sets whether to send anonymous usage statistics.
        /// </summary>
        public bool SendAnonymousUsageStatistics { get; set; } = true;
        
        /// <summary>
        /// Gets or sets the log level.
        /// </summary>
        public string LogLevel { get; set; } = "Info";
        
        /// <summary>
        /// Gets or sets whether to save log files.
        /// </summary>
        public bool SaveLogFiles { get; set; } = true;
        
        /// <summary>
        /// Gets or sets the maximum number of log files to keep.
        /// </summary>
        public int MaxLogFiles { get; set; } = 10;
        
        /// <summary>
        /// Gets or sets the maximum log file size in MB.
        /// </summary>
        public int MaxLogFileSizeMB { get; set; } = 10;
        
        /// <summary>
        /// Gets or sets the number of days to keep log files.
        /// </summary>
        public int LogRetentionDays { get; set; } = 30;
    }
    
    /// <summary>
    /// Represents the JACK settings.
    /// </summary>
    public class JackSettings
    {
        /// <summary>
        /// Gets or sets the JACK server path.
        /// </summary>
        public string ServerPath { get; set; } = @"C:\Program Files\JACK2\jackd.exe";
        
        /// <summary>
        /// Gets or sets the JACK driver.
        /// </summary>
        public string Driver { get; set; } = "portaudio";
        
        /// <summary>
        /// Gets or sets the JACK sample rate.
        /// </summary>
        public int SampleRate { get; set; } = 48000;
        
        /// <summary>
        /// Gets or sets the JACK buffer size.
        /// </summary>
        public int BufferSize { get; set; } = 1024;
        
        /// <summary>
        /// Gets or sets the JACK periods.
        /// </summary>
        public int Periods { get; set; } = 2;
        
        /// <summary>
        /// Gets or sets whether to start the JACK server on startup.
        /// </summary>
        public bool StartServerOnStartup { get; set; } = true;
        
        /// <summary>
        /// Gets or sets whether to stop the JACK server on exit.
        /// </summary>
        public bool StopServerOnExit { get; set; } = true;
        
        /// <summary>
        /// Gets or sets whether to auto-connect to system ports.
        /// </summary>
        public bool AutoConnectSystemPorts { get; set; } = true;
        
        /// <summary>
        /// Gets or sets the JACK server options.
        /// </summary>
        public string ServerOptions { get; set; } = "-T -R";
        
        /// <summary>
        /// Gets or sets the JACK client name.
        /// </summary>
        public string ClientName { get; set; } = "MaiksMixer";
    }
    
    /// <summary>
    /// Represents the MIDI settings.
    /// </summary>
    public class MidiSettings
    {
        /// <summary>
        /// Gets or sets whether to enable MIDI control.
        /// </summary>
        public bool EnableMidiControl { get; set; } = false;
        
        /// <summary>
        /// Gets or sets the MIDI input device ID.
        /// </summary>
        public string MidiInputDeviceId { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the MIDI output device ID.
        /// </summary>
        public string MidiOutputDeviceId { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the MIDI channel.
        /// </summary>
        public int MidiChannel { get; set; } = 1;
        
        /// <summary>
        /// Gets or sets whether to send MIDI feedback.
        /// </summary>
        public bool SendMidiFeedback { get; set; } = true;
        
        /// <summary>
        /// Gets or sets the MIDI control mappings.
        /// </summary>
        public List<MidiControlMapping> ControlMappings { get; set; } = new List<MidiControlMapping>();
    }
    
    /// <summary>
    /// Represents a MIDI control mapping.
    /// </summary>
    public class MidiControlMapping
    {
        /// <summary>
        /// Gets or sets the MIDI message type.
        /// </summary>
        public string MessageType { get; set; } = "CC";
        
        /// <summary>
        /// Gets or sets the MIDI channel.
        /// </summary>
        public int Channel { get; set; } = 1;
        
        /// <summary>
        /// Gets or sets the MIDI controller number.
        /// </summary>
        public int ControllerNumber { get; set; } = 0;
        
        /// <summary>
        /// Gets or sets the target parameter.
        /// </summary>
        public string TargetParameter { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the target device ID.
        /// </summary>
        public string TargetDeviceId { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the target channel.
        /// </summary>
        public int TargetChannel { get; set; } = 0;
        
        /// <summary>
        /// Gets or sets the minimum value.
        /// </summary>
        public double MinValue { get; set; } = 0.0;
        
        /// <summary>
        /// Gets or sets the maximum value.
        /// </summary>
        public double MaxValue { get; set; } = 1.0;
        
        /// <summary>
        /// Gets or sets whether the control is inverted.
        /// </summary>
        public bool IsInverted { get; set; } = false;
    }
    
    /// <summary>
    /// Represents a preset information.
    /// </summary>
    public class PresetInfo
    {
        /// <summary>
        /// Gets or sets the preset ID.
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// Gets or sets the preset name.
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the preset description.
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the preset file path.
        /// </summary>
        public string FilePath { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the creation date.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Gets or sets the last modified date.
        /// </summary>
        public DateTime ModifiedAt { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Gets or sets the preset category.
        /// </summary>
        public string Category { get; set; } = "Default";
        
        /// <summary>
        /// Gets or sets whether the preset is a favorite.
        /// </summary>
        public bool IsFavorite { get; set; } = false;
    }
}
