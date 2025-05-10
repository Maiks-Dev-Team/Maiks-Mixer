using System.Windows.Controls;
using MaiksMixer.Core.Services;
using MaiksMixer.UI.ViewModels;

namespace MaiksMixer.UI.Views
{
    /// <summary>
    /// Interaction logic for MixerView.xaml
    /// </summary>
    public partial class MixerView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the MixerView class.
        /// </summary>
        public MixerView()
        {
            InitializeComponent();
        }
        
        /// <summary>
        /// Initializes a new instance of the MixerView class with a specific audio device service.
        /// </summary>
        /// <param name="audioDeviceService">The audio device service to use.</param>
        public MixerView(AudioDeviceService audioDeviceService)
        {
            InitializeComponent();
            
            // Create and set the view model
            DataContext = new MixerViewModel(audioDeviceService);
        }
    }
}
