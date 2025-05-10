using System;
using System.Windows.Input;
using MaiksMixer.Core;

namespace MaiksMixer.UI.ViewModels
{
    /// <summary>
    /// ViewModel for an audio channel in the mixer.
    /// </summary>
    public class ChannelViewModel : ViewModelBase
    {
        private string _name = string.Empty;
        private double _volume;
        private double _peak;
        private double _rms;
        private bool _isMuted;
        private bool _isSoloed;
        private double _pan;
        private double _gain;

        /// <summary>
        /// Initializes a new instance of the ChannelViewModel class.
        /// </summary>
        public ChannelViewModel()
        {
            // Initialize commands
            MuteCommand = new RelayCommand(ExecuteMute);
            SoloCommand = new RelayCommand(ExecuteSolo);
            
            // Initialize default values
            Volume = 1.0;
            Pan = 0.0;
            Gain = 0.0;
            Peak = -60.0;
            RMS = -60.0;
        }

        #region Properties

        /// <summary>
        /// Gets or sets the name of the channel.
        /// </summary>
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        /// <summary>
        /// Gets or sets the volume level (0.0 to 1.0).
        /// </summary>
        public double Volume
        {
            get => _volume;
            set
            {
                if (SetProperty(ref _volume, Math.Clamp(value, 0.0, 1.0)))
                {
                    OnPropertyChanged(nameof(VolumeInDb));
                }
            }
        }

        /// <summary>
        /// Gets the volume level in decibels.
        /// </summary>
        public double VolumeInDb
        {
            get
            {
                if (Volume <= 0.0)
                    return -double.PositiveInfinity;
                
                // Convert from linear (0.0 to 1.0) to dB (-inf to 0)
                return 20.0 * Math.Log10(Volume);
            }
        }

        /// <summary>
        /// Gets or sets the peak level in decibels.
        /// </summary>
        public double Peak
        {
            get => _peak;
            set => SetProperty(ref _peak, value);
        }

        /// <summary>
        /// Gets or sets the RMS level in decibels.
        /// </summary>
        public double RMS
        {
            get => _rms;
            set => SetProperty(ref _rms, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the channel is muted.
        /// </summary>
        public bool IsMuted
        {
            get => _isMuted;
            set => SetProperty(ref _isMuted, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the channel is soloed.
        /// </summary>
        public bool IsSoloed
        {
            get => _isSoloed;
            set => SetProperty(ref _isSoloed, value);
        }

        /// <summary>
        /// Gets or sets the pan position (-1.0 = left, 0.0 = center, 1.0 = right).
        /// </summary>
        public double Pan
        {
            get => _pan;
            set => SetProperty(ref _pan, Math.Clamp(value, -1.0, 1.0));
        }

        /// <summary>
        /// Gets or sets the gain in decibels.
        /// </summary>
        public double Gain
        {
            get => _gain;
            set => SetProperty(ref _gain, value);
        }

        #endregion

        #region Commands

        /// <summary>
        /// Gets the command to toggle the mute state.
        /// </summary>
        public ICommand MuteCommand { get; }

        /// <summary>
        /// Gets the command to toggle the solo state.
        /// </summary>
        public ICommand SoloCommand { get; }

        #endregion

        #region Command Handlers

        private void ExecuteMute(object parameter)
        {
            IsMuted = !IsMuted;
        }

        private void ExecuteSolo(object parameter)
        {
            IsSoloed = !IsSoloed;
        }

        #endregion
    }
}
