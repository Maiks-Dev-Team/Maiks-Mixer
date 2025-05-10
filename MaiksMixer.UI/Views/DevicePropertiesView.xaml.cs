using System.Windows.Controls;
using MaiksMixer.Core.Models;
using MaiksMixer.UI.ViewModels;

namespace MaiksMixer.UI.Views
{
    /// <summary>
    /// Interaction logic for DevicePropertiesView.xaml
    /// </summary>
    public partial class DevicePropertiesView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the DevicePropertiesView class.
        /// </summary>
        public DevicePropertiesView()
        {
            InitializeComponent();
        }
        
        /// <summary>
        /// Initializes a new instance of the DevicePropertiesView class with a specific device.
        /// </summary>
        /// <param name="device">The audio device to edit.</param>
        public DevicePropertiesView(AudioDeviceInfo device)
        {
            InitializeComponent();
            
            // Create and set the view model
            DataContext = new DevicePropertiesViewModel(device);
        }
    }
}
