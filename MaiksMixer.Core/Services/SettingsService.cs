using System;
using System.IO;
using System.Threading.Tasks;
using MaiksMixer.Core.Models;

namespace MaiksMixer.Core.Services
{
    /// <summary>
    /// Service for managing application settings.
    /// </summary>
    public class SettingsService
    {
        private ApplicationSettings _settings;
        private readonly string _presetDirectory;
        
        /// <summary>
        /// Event raised when settings are changed.
        /// </summary>
        public event EventHandler<EventArgs>? SettingsChanged;
        
        /// <summary>
        /// Gets the current application settings.
        /// </summary>
        public ApplicationSettings Settings => _settings;
        
        /// <summary>
        /// Initializes a new instance of the SettingsService class.
        /// </summary>
        public SettingsService()
        {
            // Load settings
            _settings = ApplicationSettings.Load();
            
            // Set preset directory
            _presetDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "MaiksMixer",
                "Presets");
                
            // Ensure preset directory exists
            Directory.CreateDirectory(_presetDirectory);
        }
        
        /// <summary>
        /// Saves the current settings.
        /// </summary>
        /// <returns>True if the settings were saved successfully, false otherwise.</returns>
        public bool SaveSettings()
        {
            bool result = _settings.Save();
            
            if (result)
            {
                // Raise event
                SettingsChanged?.Invoke(this, EventArgs.Empty);
            }
            
            return result;
        }
        
        /// <summary>
        /// Resets the settings to default values.
        /// </summary>
        /// <returns>True if the settings were reset successfully, false otherwise.</returns>
        public bool ResetSettings()
        {
            _settings.Reset();
            bool result = _settings.Save();
            
            if (result)
            {
                // Raise event
                SettingsChanged?.Invoke(this, EventArgs.Empty);
            }
            
            return result;
        }
        
        /// <summary>
        /// Saves a preset.
        /// </summary>
        /// <param name="name">The preset name.</param>
        /// <param name="description">The preset description.</param>
        /// <param name="category">The preset category.</param>
        /// <returns>The saved preset info, or null if the operation failed.</returns>
        public async Task<PresetInfo?> SavePresetAsync(string name, string description, string category)
        {
            try
            {
                // Create preset info
                var presetInfo = new PresetInfo
                {
                    Name = name,
                    Description = description,
                    Category = category,
                    CreatedAt = DateTime.Now,
                    ModifiedAt = DateTime.Now
                };
                
                // Set file path
                string fileName = $"{presetInfo.Id}.json";
                string categoryDir = Path.Combine(_presetDirectory, category);
                Directory.CreateDirectory(categoryDir);
                presetInfo.FilePath = Path.Combine(categoryDir, fileName);
                
                // Create preset data
                var presetData = new
                {
                    Info = presetInfo,
                    Audio = _settings.Audio,
                    Jack = _settings.Jack
                };
                
                // Serialize preset data
                var options = new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                };
                
                string json = System.Text.Json.JsonSerializer.Serialize(presetData, options);
                
                // Write file
                await File.WriteAllTextAsync(presetInfo.FilePath, json);
                
                // Add to presets list if not already present
                var existingPreset = _settings.Presets.Find(p => p.Id == presetInfo.Id);
                if (existingPreset != null)
                {
                    // Update existing preset
                    existingPreset.Name = presetInfo.Name;
                    existingPreset.Description = presetInfo.Description;
                    existingPreset.Category = presetInfo.Category;
                    existingPreset.FilePath = presetInfo.FilePath;
                    existingPreset.ModifiedAt = presetInfo.ModifiedAt;
                }
                else
                {
                    // Add new preset
                    _settings.Presets.Add(presetInfo);
                }
                
                // Save settings
                _settings.LastOpenedPreset = presetInfo.Id;
                SaveSettings();
                
                return presetInfo;
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error saving preset: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Loads a preset.
        /// </summary>
        /// <param name="presetId">The preset ID.</param>
        /// <returns>True if the preset was loaded successfully, false otherwise.</returns>
        public async Task<bool> LoadPresetAsync(string presetId)
        {
            try
            {
                // Find preset
                var presetInfo = _settings.Presets.Find(p => p.Id == presetId);
                if (presetInfo == null || !File.Exists(presetInfo.FilePath))
                {
                    return false;
                }
                
                // Read file
                string json = await File.ReadAllTextAsync(presetInfo.FilePath);
                
                // Deserialize
                var options = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                };
                
                var presetData = System.Text.Json.JsonSerializer.Deserialize<dynamic>(json, options);
                
                if (presetData == null)
                {
                    return false;
                }
                
                // Update settings
                _settings.Audio = presetData.GetProperty("audio").Deserialize<AudioSettings>(options);
                _settings.Jack = presetData.GetProperty("jack").Deserialize<JackSettings>(options);
                _settings.LastOpenedPreset = presetId;
                
                // Save settings
                SaveSettings();
                
                return true;
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error loading preset: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Deletes a preset.
        /// </summary>
        /// <param name="presetId">The preset ID.</param>
        /// <returns>True if the preset was deleted successfully, false otherwise.</returns>
        public bool DeletePreset(string presetId)
        {
            try
            {
                // Find preset
                var presetInfo = _settings.Presets.Find(p => p.Id == presetId);
                if (presetInfo == null)
                {
                    return false;
                }
                
                // Delete file
                if (File.Exists(presetInfo.FilePath))
                {
                    File.Delete(presetInfo.FilePath);
                }
                
                // Remove from presets list
                _settings.Presets.Remove(presetInfo);
                
                // Clear last opened preset if it was this one
                if (_settings.LastOpenedPreset == presetId)
                {
                    _settings.LastOpenedPreset = string.Empty;
                }
                
                // Save settings
                SaveSettings();
                
                return true;
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error deleting preset: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Gets a list of available JACK drivers.
        /// </summary>
        /// <returns>An array of available JACK drivers.</returns>
        public string[] GetAvailableJackDrivers()
        {
            // In a real implementation, this would query the JACK server
            // For now, return a static list
            return new string[]
            {
                "portaudio",
                "alsa",
                "coreaudio",
                "dummy",
                "net",
                "netone"
            };
        }
        
        /// <summary>
        /// Gets a list of available sample rates.
        /// </summary>
        /// <returns>An array of available sample rates.</returns>
        public int[] GetAvailableSampleRates()
        {
            return new int[]
            {
                22050,
                32000,
                44100,
                48000,
                88200,
                96000,
                176400,
                192000
            };
        }
        
        /// <summary>
        /// Gets a list of available buffer sizes.
        /// </summary>
        /// <returns>An array of available buffer sizes.</returns>
        public int[] GetAvailableBufferSizes()
        {
            return new int[]
            {
                64,
                128,
                256,
                512,
                1024,
                2048,
                4096
            };
        }
        
        /// <summary>
        /// Gets a list of available bit depths.
        /// </summary>
        /// <returns>An array of available bit depths.</returns>
        public int[] GetAvailableBitDepths()
        {
            return new int[]
            {
                16,
                24,
                32
            };
        }
        
        /// <summary>
        /// Gets a list of available themes.
        /// </summary>
        /// <returns>An array of available themes.</returns>
        public string[] GetAvailableThemes()
        {
            return new string[]
            {
                "Dark",
                "Light",
                "Blue",
                "High Contrast"
            };
        }
        
        /// <summary>
        /// Gets a list of available log levels.
        /// </summary>
        /// <returns>An array of available log levels.</returns>
        public string[] GetAvailableLogLevels()
        {
            return new string[]
            {
                "Trace",
                "Debug",
                "Info",
                "Warning",
                "Error",
                "Critical",
                "None"
            };
        }
    }
}
