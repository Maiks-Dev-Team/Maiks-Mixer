using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using MaiksMixer.Core.Models;
using MaiksMixer.Core.Services;

namespace MaiksMixer.UI.ViewModels
{
    /// <summary>
    /// View model for the configuration UI.
    /// </summary>
    public class ConfigurationViewModel : INotifyPropertyChanged
    {
        private readonly SettingsService _settingsService;
        private string _statusMessage = string.Empty;
        private bool _isLoading;
        private string _selectedTab = "Audio";
        private bool _hasUnsavedChanges;
        private string _newPresetName = string.Empty;
        private string _newPresetDescription = string.Empty;
        private string _newPresetCategory = "Default";
        private PresetInfo? _selectedPreset;
        
        /// <summary>
        /// Event raised when a property changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;
        
        /// <summary>
        /// Gets or sets the application settings.
        /// </summary>
        public ApplicationSettings Settings => _settingsService.Settings;
        
        /// <summary>
        /// Gets or sets the status message.
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }
        
        /// <summary>
        /// Gets or sets whether the view model is loading data.
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }
        
        /// <summary>
        /// Gets or sets the selected tab.
        /// </summary>
        public string SelectedTab
        {
            get => _selectedTab;
            set => SetProperty(ref _selectedTab, value);
        }
        
        /// <summary>
        /// Gets or sets whether there are unsaved changes.
        /// </summary>
        public bool HasUnsavedChanges
        {
            get => _hasUnsavedChanges;
            set => SetProperty(ref _hasUnsavedChanges, value);
        }
        
        /// <summary>
        /// Gets or sets the name of the new preset.
        /// </summary>
        public string NewPresetName
        {
            get => _newPresetName;
            set => SetProperty(ref _newPresetName, value);
        }
        
        /// <summary>
        /// Gets or sets the description of the new preset.
        /// </summary>
        public string NewPresetDescription
        {
            get => _newPresetDescription;
            set => SetProperty(ref _newPresetDescription, value);
        }
        
        /// <summary>
        /// Gets or sets the category of the new preset.
        /// </summary>
        public string NewPresetCategory
        {
            get => _newPresetCategory;
            set => SetProperty(ref _newPresetCategory, value);
        }
        
        /// <summary>
        /// Gets or sets the selected preset.
        /// </summary>
        public PresetInfo? SelectedPreset
        {
            get => _selectedPreset;
            set => SetProperty(ref _selectedPreset, value);
        }
        
        /// <summary>
        /// Gets the presets.
        /// </summary>
        public ObservableCollection<PresetInfo> Presets { get; } = new ObservableCollection<PresetInfo>();
        
        /// <summary>
        /// Gets the available sample rates.
        /// </summary>
        public ObservableCollection<int> AvailableSampleRates { get; } = new ObservableCollection<int>();
        
        /// <summary>
        /// Gets the available buffer sizes.
        /// </summary>
        public ObservableCollection<int> AvailableBufferSizes { get; } = new ObservableCollection<int>();
        
        /// <summary>
        /// Gets the available bit depths.
        /// </summary>
        public ObservableCollection<int> AvailableBitDepths { get; } = new ObservableCollection<int>();
        
        /// <summary>
        /// Gets the available JACK drivers.
        /// </summary>
        public ObservableCollection<string> AvailableJackDrivers { get; } = new ObservableCollection<string>();
        
        /// <summary>
        /// Gets the available themes.
        /// </summary>
        public ObservableCollection<string> AvailableThemes { get; } = new ObservableCollection<string>();
        
        /// <summary>
        /// Gets the available log levels.
        /// </summary>
        public ObservableCollection<string> AvailableLogLevels { get; } = new ObservableCollection<string>();
        
        /// <summary>
        /// Gets the command to save settings.
        /// </summary>
        public ICommand SaveSettingsCommand { get; }
        
        /// <summary>
        /// Gets the command to reset settings.
        /// </summary>
        public ICommand ResetSettingsCommand { get; }
        
        /// <summary>
        /// Gets the command to save a preset.
        /// </summary>
        public ICommand SavePresetCommand { get; }
        
        /// <summary>
        /// Gets the command to load a preset.
        /// </summary>
        public ICommand LoadPresetCommand { get; }
        
        /// <summary>
        /// Gets the command to delete a preset.
        /// </summary>
        public ICommand DeletePresetCommand { get; }
        
        /// <summary>
        /// Initializes a new instance of the ConfigurationViewModel class.
        /// </summary>
        /// <param name="settingsService">The settings service to use.</param>
        public ConfigurationViewModel(SettingsService settingsService)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            
            // Initialize commands
            SaveSettingsCommand = new RelayCommand(_ => SaveSettings(), _ => HasUnsavedChanges);
            ResetSettingsCommand = new RelayCommand(_ => ResetSettings());
            SavePresetCommand = new RelayCommand(async _ => await SavePresetAsync(), _ => !string.IsNullOrWhiteSpace(NewPresetName));
            LoadPresetCommand = new RelayCommand(async _ => await LoadPresetAsync(), _ => SelectedPreset != null);
            DeletePresetCommand = new RelayCommand(_ => DeletePreset(), _ => SelectedPreset != null);
            
            // Register for settings changed event
            _settingsService.SettingsChanged += SettingsService_SettingsChanged;
            
            // Initialize collections
            InitializeCollections();
            
            // Load presets
            LoadPresets();
        }
        
        /// <summary>
        /// Initializes the collections.
        /// </summary>
        private void InitializeCollections()
        {
            // Clear collections
            AvailableSampleRates.Clear();
            AvailableBufferSizes.Clear();
            AvailableBitDepths.Clear();
            AvailableJackDrivers.Clear();
            AvailableThemes.Clear();
            AvailableLogLevels.Clear();
            
            // Add items to collections
            foreach (var sampleRate in _settingsService.GetAvailableSampleRates())
            {
                AvailableSampleRates.Add(sampleRate);
            }
            
            foreach (var bufferSize in _settingsService.GetAvailableBufferSizes())
            {
                AvailableBufferSizes.Add(bufferSize);
            }
            
            foreach (var bitDepth in _settingsService.GetAvailableBitDepths())
            {
                AvailableBitDepths.Add(bitDepth);
            }
            
            foreach (var driver in _settingsService.GetAvailableJackDrivers())
            {
                AvailableJackDrivers.Add(driver);
            }
            
            foreach (var theme in _settingsService.GetAvailableThemes())
            {
                AvailableThemes.Add(theme);
            }
            
            foreach (var logLevel in _settingsService.GetAvailableLogLevels())
            {
                AvailableLogLevels.Add(logLevel);
            }
        }
        
        /// <summary>
        /// Loads the presets.
        /// </summary>
        private void LoadPresets()
        {
            // Clear presets
            Presets.Clear();
            
            // Add presets
            foreach (var preset in Settings.Presets)
            {
                Presets.Add(preset);
            }
            
            // Select last opened preset
            if (!string.IsNullOrEmpty(Settings.LastOpenedPreset))
            {
                SelectedPreset = Presets.FirstOrDefault(p => p.Id == Settings.LastOpenedPreset);
            }
        }
        
        /// <summary>
        /// Handles the SettingsChanged event from the settings service.
        /// </summary>
        private void SettingsService_SettingsChanged(object? sender, EventArgs e)
        {
            // Refresh presets
            LoadPresets();
            
            // Notify property changed for settings
            OnPropertyChanged(nameof(Settings));
            
            // Reset unsaved changes flag
            HasUnsavedChanges = false;
        }
        
        /// <summary>
        /// Saves the settings.
        /// </summary>
        private void SaveSettings()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Saving settings...";
                
                bool result = _settingsService.SaveSettings();
                
                if (result)
                {
                    StatusMessage = "Settings saved successfully.";
                    HasUnsavedChanges = false;
                }
                else
                {
                    StatusMessage = "Failed to save settings.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error saving settings: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        /// <summary>
        /// Resets the settings.
        /// </summary>
        private void ResetSettings()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Resetting settings...";
                
                bool result = _settingsService.ResetSettings();
                
                if (result)
                {
                    StatusMessage = "Settings reset successfully.";
                    HasUnsavedChanges = false;
                }
                else
                {
                    StatusMessage = "Failed to reset settings.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error resetting settings: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        /// <summary>
        /// Saves a preset.
        /// </summary>
        private async Task SavePresetAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = $"Saving preset '{NewPresetName}'...";
                
                var preset = await _settingsService.SavePresetAsync(NewPresetName, NewPresetDescription, NewPresetCategory);
                
                if (preset != null)
                {
                    StatusMessage = $"Preset '{NewPresetName}' saved successfully.";
                    
                    // Reset form
                    NewPresetName = string.Empty;
                    NewPresetDescription = string.Empty;
                    NewPresetCategory = "Default";
                    
                    // Select the new preset
                    SelectedPreset = preset;
                }
                else
                {
                    StatusMessage = $"Failed to save preset '{NewPresetName}'.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error saving preset: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        /// <summary>
        /// Loads a preset.
        /// </summary>
        private async Task LoadPresetAsync()
        {
            if (SelectedPreset == null)
                return;
                
            try
            {
                IsLoading = true;
                StatusMessage = $"Loading preset '{SelectedPreset.Name}'...";
                
                bool result = await _settingsService.LoadPresetAsync(SelectedPreset.Id);
                
                if (result)
                {
                    StatusMessage = $"Preset '{SelectedPreset.Name}' loaded successfully.";
                    HasUnsavedChanges = false;
                }
                else
                {
                    StatusMessage = $"Failed to load preset '{SelectedPreset.Name}'.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading preset: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        /// <summary>
        /// Deletes a preset.
        /// </summary>
        private void DeletePreset()
        {
            if (SelectedPreset == null)
                return;
                
            try
            {
                IsLoading = true;
                StatusMessage = $"Deleting preset '{SelectedPreset.Name}'...";
                
                bool result = _settingsService.DeletePreset(SelectedPreset.Id);
                
                if (result)
                {
                    StatusMessage = $"Preset '{SelectedPreset.Name}' deleted successfully.";
                    SelectedPreset = null;
                }
                else
                {
                    StatusMessage = $"Failed to delete preset '{SelectedPreset.Name}'.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error deleting preset: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        /// <summary>
        /// Marks the settings as having unsaved changes.
        /// </summary>
        public void MarkAsChanged()
        {
            HasUnsavedChanges = true;
        }
        
        /// <summary>
        /// Sets a property value and raises the PropertyChanged event if the value has changed.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="storage">A reference to the property's backing field.</param>
        /// <param name="value">The new value of the property.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>True if the value was changed, false otherwise.</returns>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(storage, value))
                return false;
                
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        
        /// <summary>
        /// Raises the PropertyChanged event for the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    
    /// <summary>
    /// A command that relays its functionality to other objects.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;
        
        /// <summary>
        /// Event raised when the CanExecute state changes.
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
        
        /// <summary>
        /// Initializes a new instance of the RelayCommand class.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }
        
        /// <summary>
        /// Determines whether this command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command.</param>
        /// <returns>True if this command can be executed; otherwise, false.</returns>
        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }
        
        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="parameter">Data used by the command.</param>
        public void Execute(object? parameter)
        {
            _execute(parameter);
        }
    }
}
