using System.Windows.Controls;
using MaiksMixer.Core.Services;
using MaiksMixer.UI.ViewModels;

namespace MaiksMixer.UI.Views
{
    /// <summary>
    /// Interaction logic for ConfigurationView.xaml
    /// </summary>
    public partial class ConfigurationView : UserControl
    {
        private readonly ConfigurationViewModel _viewModel;
        
        /// <summary>
        /// Initializes a new instance of the ConfigurationView class.
        /// </summary>
        /// <param name="settingsService">The settings service to use.</param>
        public ConfigurationView(SettingsService settingsService)
        {
            InitializeComponent();
            
            // Create view model
            _viewModel = new ConfigurationViewModel(settingsService);
            DataContext = _viewModel;
        }
        
        /// <summary>
        /// Initializes a new instance of the ConfigurationView class.
        /// This constructor is used by the designer.
        /// </summary>
        public ConfigurationView()
        {
            InitializeComponent();
            
            // Design-time data
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                // Create a mock settings service
                var settingsService = new SettingsService();
                
                // Create view model
                _viewModel = new ConfigurationViewModel(settingsService);
                DataContext = _viewModel;
            }
        }
    }
}
