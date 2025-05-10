using System.Windows.Controls;
using MaiksMixer.Core.Services;

namespace MaiksMixer.UI.Views
{
    /// <summary>
    /// Interaction logic for ConfigurationTestView.xaml
    /// </summary>
    public partial class ConfigurationTestView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the ConfigurationTestView class.
        /// </summary>
        public ConfigurationTestView()
        {
            InitializeComponent();
            
            // Create settings service
            var settingsService = new SettingsService();
            
            // Create configuration view
            var configurationView = new ConfigurationView(settingsService);
            ConfigurationContainer.Content = configurationView;
        }
    }
}
