using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using MaiksMixer.Core.Models;
using MaiksMixer.Core.Services;

namespace MaiksMixer.UI.ViewModels
{
    /// <summary>
    /// ViewModel for the preset management interface.
    /// </summary>
    public class PresetManagementViewModel : INotifyPropertyChanged
    {
        private readonly SettingsService _settingsService;
        private ObservableCollection<PresetInfo> _presets;
        private ObservableCollection<PresetInfo> _filteredPresets;
        private ObservableCollection<string> _categories;
        private string _selectedCategory;
        private string _searchText;
        private string _statusMessage;
        private bool _isLoading;
        private bool _isEditingPreset;
        private string _presetEditorTitle;
        private string _editPresetName;
        private string _editPresetDescription;
        private PresetInfo _presetBeingEdited;

        /// <summary>
        /// Initializes a new instance of the PresetManagementViewModel class.
        /// </summary>
        /// <param name="settingsService">The settings service to use.</param>
        public PresetManagementViewModel(SettingsService settingsService)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            
            // Initialize collections
            _presets = new ObservableCollection<PresetInfo>();
            _filteredPresets = new ObservableCollection<PresetInfo>();
            _categories = new ObservableCollection<string>();
            
            // Initialize commands
            CreatePresetCommand = new RelayCommand(CreatePreset);
            ImportPresetCommand = new RelayCommand(ImportPreset);
            ExportPresetCommand = new RelayCommand(ExportPreset, CanExportPreset);
            RefreshPresetsCommand = new RelayCommand(RefreshPresets);
            LoadPresetCommand = new RelayCommand<PresetInfo>(LoadPreset);
            EditPresetCommand = new RelayCommand<PresetInfo>(EditPreset);
            DeletePresetCommand = new RelayCommand<PresetInfo>(DeletePreset);
            AddCategoryCommand = new RelayCommand(AddCategory);
            RenameCategoryCommand = new RelayCommand(RenameCategory, CanModifyCategory);
            DeleteCategoryCommand = new RelayCommand(DeleteCategory, CanModifyCategory);
            SearchCommand = new RelayCommand(SearchPresets);
            CancelEditPresetCommand = new RelayCommand(CancelEditPreset);
            SaveEditPresetCommand = new RelayCommand(SaveEditPreset, CanSaveEditPreset);
            
            // Load presets
            RefreshPresets();
        }

        #region Properties

        /// <summary>
        /// Gets the collection of presets.
        /// </summary>
        public ObservableCollection<PresetInfo> Presets
        {
            get => _presets;
            private set
            {
                _presets = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the filtered collection of presets.
        /// </summary>
        public ObservableCollection<PresetInfo> FilteredPresets
        {
            get => _filteredPresets;
            private set
            {
                _filteredPresets = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasNoPresets));
            }
        }

        /// <summary>
        /// Gets a value indicating whether there are no presets to display.
        /// </summary>
        public bool HasNoPresets => FilteredPresets.Count == 0;

        /// <summary>
        /// Gets the collection of categories.
        /// </summary>
        public ObservableCollection<string> Categories
        {
            get => _categories;
            private set
            {
                _categories = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the selected category.
        /// </summary>
        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged();
                FilterPresets();
            }
        }

        /// <summary>
        /// Gets or sets the search text.
        /// </summary>
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the status message.
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the view is loading.
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether a preset is being edited.
        /// </summary>
        public bool IsEditingPreset
        {
            get => _isEditingPreset;
            set
            {
                _isEditingPreset = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the title of the preset editor dialog.
        /// </summary>
        public string PresetEditorTitle
        {
            get => _presetEditorTitle;
            set
            {
                _presetEditorTitle = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the name of the preset being edited.
        /// </summary>
        public string EditPresetName
        {
            get => _editPresetName;
            set
            {
                _editPresetName = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the description of the preset being edited.
        /// </summary>
        public string EditPresetDescription
        {
            get => _editPresetDescription;
            set
            {
                _editPresetDescription = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Gets the command to create a new preset.
        /// </summary>
        public ICommand CreatePresetCommand { get; }

        /// <summary>
        /// Gets the command to import a preset.
        /// </summary>
        public ICommand ImportPresetCommand { get; }

        /// <summary>
        /// Gets the command to export a preset.
        /// </summary>
        public ICommand ExportPresetCommand { get; }

        /// <summary>
        /// Gets the command to refresh the presets.
        /// </summary>
        public ICommand RefreshPresetsCommand { get; }

        /// <summary>
        /// Gets the command to load a preset.
        /// </summary>
        public ICommand LoadPresetCommand { get; }

        /// <summary>
        /// Gets the command to edit a preset.
        /// </summary>
        public ICommand EditPresetCommand { get; }

        /// <summary>
        /// Gets the command to delete a preset.
        /// </summary>
        public ICommand DeletePresetCommand { get; }

        /// <summary>
        /// Gets the command to add a category.
        /// </summary>
        public ICommand AddCategoryCommand { get; }

        /// <summary>
        /// Gets the command to rename a category.
        /// </summary>
        public ICommand RenameCategoryCommand { get; }

        /// <summary>
        /// Gets the command to delete a category.
        /// </summary>
        public ICommand DeleteCategoryCommand { get; }

        /// <summary>
        /// Gets the command to search presets.
        /// </summary>
        public ICommand SearchCommand { get; }

        /// <summary>
        /// Gets the command to cancel editing a preset.
        /// </summary>
        public ICommand CancelEditPresetCommand { get; }

        /// <summary>
        /// Gets the command to save a preset being edited.
        /// </summary>
        public ICommand SaveEditPresetCommand { get; }

        #endregion

        #region Command Methods

        private void CreatePreset()
        {
            PresetEditorTitle = "Create New Preset";
            EditPresetName = string.Empty;
            EditPresetDescription = string.Empty;
            _presetBeingEdited = null;
            IsEditingPreset = true;
        }

        private void ImportPreset()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "MaiksMixer Preset Files (*.mmpreset)|*.mmpreset|All Files (*.*)|*.*",
                Title = "Import Preset"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    IsLoading = true;
                    StatusMessage = "Importing preset...";

                    // In a real implementation, this would call the settings service to import the preset
                    // _settingsService.ImportPreset(dialog.FileName);
                    
                    // For now, we'll just simulate the import
                    System.Threading.Thread.Sleep(500);
                    
                    RefreshPresets();
                    StatusMessage = "Preset imported successfully.";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error importing preset: {ex.Message}";
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private void ExportPreset()
        {
            // In a real implementation, this would show a dialog to select presets to export
            var dialog = new SaveFileDialog
            {
                Filter = "MaiksMixer Preset Files (*.mmpreset)|*.mmpreset|All Files (*.*)|*.*",
                Title = "Export Preset"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    IsLoading = true;
                    StatusMessage = "Exporting preset...";

                    // In a real implementation, this would call the settings service to export the preset
                    // _settingsService.ExportPreset(selectedPreset, dialog.FileName);
                    
                    // For now, we'll just simulate the export
                    System.Threading.Thread.Sleep(500);
                    
                    StatusMessage = "Preset exported successfully.";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error exporting preset: {ex.Message}";
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private bool CanExportPreset()
        {
            // In a real implementation, this would check if a preset is selected
            return true;
        }

        private void RefreshPresets()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Loading presets...";

                // In a real implementation, this would load presets from the settings service
                // var presets = _settingsService.GetAllPresets();
                
                // For now, we'll create some sample presets
                var samplePresets = new List<PresetInfo>
                {
                    new PresetInfo
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Default Studio Setup",
                        Description = "Standard configuration for studio recording with ASIO driver and 48kHz sample rate.",
                        Category = "Default",
                        CreatedAt = DateTime.Now.AddDays(-30),
                        ModifiedAt = DateTime.Now.AddDays(-5),
                        IsFavorite = true
                    },
                    new PresetInfo
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Live Performance",
                        Description = "Low latency configuration for live performances with minimal buffer size.",
                        Category = "Live",
                        CreatedAt = DateTime.Now.AddDays(-20),
                        ModifiedAt = DateTime.Now.AddDays(-20),
                        IsFavorite = false
                    },
                    new PresetInfo
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "High Quality Recording",
                        Description = "High resolution configuration with 96kHz sample rate and 24-bit depth.",
                        Category = "Recording",
                        CreatedAt = DateTime.Now.AddDays(-15),
                        ModifiedAt = DateTime.Now.AddDays(-10),
                        IsFavorite = true
                    },
                    new PresetInfo
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Mastering Setup",
                        Description = "Configuration optimized for mastering with high quality settings.",
                        Category = "Mastering",
                        CreatedAt = DateTime.Now.AddDays(-10),
                        ModifiedAt = DateTime.Now.AddDays(-2),
                        IsFavorite = false
                    },
                    new PresetInfo
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Podcast Recording",
                        Description = "Setup for podcast recording with voice optimization.",
                        Category = "Recording",
                        CreatedAt = DateTime.Now.AddDays(-5),
                        ModifiedAt = DateTime.Now.AddDays(-1),
                        IsFavorite = false
                    }
                };

                Presets = new ObservableCollection<PresetInfo>(samplePresets);
                
                // Extract categories
                var allCategories = new HashSet<string>(Presets.Select(p => p.Category));
                Categories = new ObservableCollection<string>(allCategories);
                
                // Add "All" category
                Categories.Insert(0, "All");
                SelectedCategory = "All";
                
                FilterPresets();
                StatusMessage = $"Loaded {Presets.Count} presets.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading presets: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void LoadPreset(PresetInfo preset)
        {
            if (preset == null)
            {
                return;
            }

            try
            {
                IsLoading = true;
                StatusMessage = $"Loading preset '{preset.Name}'...";

                // In a real implementation, this would call the settings service to load the preset
                // _settingsService.LoadPreset(preset.Id);
                
                // For now, we'll just simulate the load
                System.Threading.Thread.Sleep(500);
                
                StatusMessage = $"Preset '{preset.Name}' loaded successfully.";
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

        private void EditPreset(PresetInfo preset)
        {
            if (preset == null)
            {
                return;
            }

            PresetEditorTitle = "Edit Preset";
            EditPresetName = preset.Name;
            EditPresetDescription = preset.Description;
            _presetBeingEdited = preset;
            IsEditingPreset = true;
        }

        private void DeletePreset(PresetInfo preset)
        {
            if (preset == null)
            {
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete the preset '{preset.Name}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    IsLoading = true;
                    StatusMessage = $"Deleting preset '{preset.Name}'...";

                    // In a real implementation, this would call the settings service to delete the preset
                    // _settingsService.DeletePreset(preset.Id);
                    
                    // For now, we'll just remove it from our collection
                    Presets.Remove(preset);
                    FilterPresets();
                    
                    StatusMessage = $"Preset '{preset.Name}' deleted successfully.";
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
        }

        private void AddCategory()
        {
            // In a real implementation, this would show a dialog to enter a new category name
            var categoryName = "New Category";
            
            if (!string.IsNullOrWhiteSpace(categoryName) && !Categories.Contains(categoryName))
            {
                Categories.Add(categoryName);
                StatusMessage = $"Category '{categoryName}' added.";
            }
        }

        private void RenameCategory()
        {
            if (string.IsNullOrEmpty(SelectedCategory) || SelectedCategory == "All")
            {
                return;
            }

            // In a real implementation, this would show a dialog to enter a new category name
            var newCategoryName = $"{SelectedCategory} (Renamed)";
            
            if (!string.IsNullOrWhiteSpace(newCategoryName) && !Categories.Contains(newCategoryName))
            {
                var oldCategoryName = SelectedCategory;
                
                // Update presets with the old category
                foreach (var preset in Presets.Where(p => p.Category == oldCategoryName))
                {
                    preset.Category = newCategoryName;
                }
                
                // Update categories collection
                var index = Categories.IndexOf(oldCategoryName);
                Categories.RemoveAt(index);
                Categories.Insert(index, newCategoryName);
                SelectedCategory = newCategoryName;
                
                FilterPresets();
                StatusMessage = $"Category '{oldCategoryName}' renamed to '{newCategoryName}'.";
            }
        }

        private void DeleteCategory()
        {
            if (string.IsNullOrEmpty(SelectedCategory) || SelectedCategory == "All")
            {
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete the category '{SelectedCategory}'? All presets in this category will be moved to 'Default'.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var categoryName = SelectedCategory;
                
                // Move presets to Default category
                foreach (var preset in Presets.Where(p => p.Category == categoryName))
                {
                    preset.Category = "Default";
                }
                
                // Ensure Default category exists
                if (!Categories.Contains("Default"))
                {
                    Categories.Add("Default");
                }
                
                // Remove the category
                Categories.Remove(categoryName);
                SelectedCategory = "All";
                
                FilterPresets();
                StatusMessage = $"Category '{categoryName}' deleted. Presets moved to 'Default'.";
            }
        }

        private bool CanModifyCategory()
        {
            return !string.IsNullOrEmpty(SelectedCategory) && SelectedCategory != "All";
        }

        private void SearchPresets()
        {
            FilterPresets();
        }

        private void CancelEditPreset()
        {
            IsEditingPreset = false;
            _presetBeingEdited = null;
        }

        private void SaveEditPreset()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Saving preset...";

                if (_presetBeingEdited == null)
                {
                    // Creating a new preset
                    var newPreset = new PresetInfo
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = EditPresetName,
                        Description = EditPresetDescription,
                        Category = "Default", // Default category for new presets
                        CreatedAt = DateTime.Now,
                        ModifiedAt = DateTime.Now,
                        IsFavorite = false
                    };

                    // In a real implementation, this would call the settings service to save the preset
                    // _settingsService.SavePreset(newPreset);
                    
                    // For now, we'll just add it to our collection
                    Presets.Add(newPreset);
                    
                    StatusMessage = $"Preset '{newPreset.Name}' created successfully.";
                }
                else
                {
                    // Updating an existing preset
                    _presetBeingEdited.Name = EditPresetName;
                    _presetBeingEdited.Description = EditPresetDescription;
                    _presetBeingEdited.ModifiedAt = DateTime.Now;

                    // In a real implementation, this would call the settings service to update the preset
                    // _settingsService.UpdatePreset(_presetBeingEdited);
                    
                    StatusMessage = $"Preset '{_presetBeingEdited.Name}' updated successfully.";
                }

                // Refresh the UI
                FilterPresets();
                IsEditingPreset = false;
                _presetBeingEdited = null;
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

        private bool CanSaveEditPreset()
        {
            return !string.IsNullOrWhiteSpace(EditPresetName);
        }

        #endregion

        #region Helper Methods

        private void FilterPresets()
        {
            IEnumerable<PresetInfo> filtered = Presets;
            
            // Filter by category
            if (!string.IsNullOrEmpty(SelectedCategory) && SelectedCategory != "All")
            {
                filtered = filtered.Where(p => p.Category == SelectedCategory);
            }
            
            // Filter by search text
            if (!string.IsNullOrEmpty(SearchText))
            {
                var search = SearchText.ToLowerInvariant();
                filtered = filtered.Where(p =>
                    p.Name.ToLowerInvariant().Contains(search) ||
                    p.Description.ToLowerInvariant().Contains(search) ||
                    p.Category.ToLowerInvariant().Contains(search));
            }
            
            FilteredPresets = new ObservableCollection<PresetInfo>(filtered);
        }

        #endregion

        #region INotifyPropertyChanged

        /// <summary>
        /// Event raised when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    /// <summary>
    /// Implementation of ICommand for relay commands.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        /// <summary>
        /// Initializes a new instance of the RelayCommand class.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Event raised when the execution status changes.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command.</param>
        /// <returns>true if this command can be executed; otherwise, false.</returns>
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute();
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="parameter">Data used by the command.</param>
        public void Execute(object parameter)
        {
            _execute();
        }
    }

    /// <summary>
    /// Implementation of ICommand for relay commands with a parameter.
    /// </summary>
    /// <typeparam name="T">The type of the command parameter.</typeparam>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T> _canExecute;

        /// <summary>
        /// Initializes a new instance of the RelayCommand class.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Event raised when the execution status changes.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command.</param>
        /// <returns>true if this command can be executed; otherwise, false.</returns>
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute((T)parameter);
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="parameter">Data used by the command.</param>
        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }
    }
}
