using System.Windows.Controls;
using MaiksMixer.Core.Services;
using MaiksMixer.UI.ViewModels;

namespace MaiksMixer.UI.Views
{
    /// <summary>
    /// Interaction logic for RoutingMatrixView.xaml
    /// </summary>
    public partial class RoutingMatrixView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the RoutingMatrixView class.
        /// </summary>
        public RoutingMatrixView()
        {
            InitializeComponent();
        }
        
        /// <summary>
        /// Initializes a new instance of the RoutingMatrixView class with a specific audio device service.
        /// </summary>
        /// <param name="audioDeviceService">The audio device service to use.</param>
        public RoutingMatrixView(AudioDeviceService audioDeviceService)
        {
            InitializeComponent();
            
            // Create and set the view model
            DataContext = new RoutingMatrixViewModel(audioDeviceService);
        }
    }
}
