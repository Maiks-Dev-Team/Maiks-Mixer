using System.Windows.Controls;
using MaiksMixer.Core.Services;
using MaiksMixer.UI.ViewModels;

namespace MaiksMixer.UI.Views
{
    /// <summary>
    /// Interaction logic for PresetManagementView.xaml
    /// </summary>
    public partial class PresetManagementView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the PresetManagementView class.
        /// </summary>
        public PresetManagementView()
        {
            InitializeComponent();
            
            // Create settings service
            var settingsService = new SettingsService();
            
            // Create and set the view model
            DataContext = new PresetManagementViewModel(settingsService);
        }
        
        /// <summary>
        /// Initializes a new instance of the PresetManagementView class with a specific settings service.
        /// </summary>
        /// <param name="settingsService">The settings service to use.</param>
        public PresetManagementView(SettingsService settingsService)
        {
            InitializeComponent();
            
            // Create and set the view model
            DataContext = new PresetManagementViewModel(settingsService);
        }
    }
}
