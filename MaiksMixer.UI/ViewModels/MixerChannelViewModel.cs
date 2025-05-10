using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MaiksMixer.Core.Models;
using MaiksMixer.Core.Services;

namespace MaiksMixer.UI.ViewModels
{
    /// <summary>
    /// ViewModel for the mixer channel view.
    /// </summary>
    public class MixerChannelViewModel : INotifyPropertyChanged
    {
        private readonly AudioDeviceService _audioDeviceService;
        private string _id;
        private string _name;
        private double _volume;
        private double _pan;
        private bool _isMuted;
        private bool _isSolo;
        private double _leftLevel;
        private double _rightLevel;
        private double _peakLeftLevel;
        private double _peakRightLevel;
        private AudioDeviceInfo _device;
        private AudioPortInfo _port;

        /// <summary>
        /// Initializes a new instance of the MixerChannelViewModel class.
        /// </summary>
        /// <param name="audioDeviceService">The audio device service to use.</param>
        /// <param name="device">The audio device.</param>
        /// <param name="port">The audio port.</param>
        public MixerChannelViewModel(AudioDeviceService audioDeviceService, AudioDeviceInfo device, AudioPortInfo port)
        {
            _audioDeviceService = audioDeviceService ?? throw new ArgumentNullException(nameof(audioDeviceService));
            _device = device ?? throw new ArgumentNullException(nameof(device));
            _port = port ?? throw new ArgumentNullException(nameof(port));
            
            // Initialize properties
            _id = port.Id;
            _name = $"{device.Name} - {port.Name}";
            _volume = 0.75;
            _pan = 0.0;
            _isMuted = false;
            _isSolo = false;
            _leftLevel = 0.0;
            _rightLevel = 0.0;
            _peakLeftLevel = 0.0;
            _peakRightLevel = 0.0;
            
            // Initialize commands
            ResetCommand = new RelayCommand(ResetChannel);
            
            // Register for level meter updates
            _audioDeviceService.LevelUpdated += AudioDeviceService_LevelUpdated;
        }

        #region Properties

        /// <summary>
        /// Gets the ID of the channel.
        /// </summary>
        public string Id => _id;

        /// <summary>
        /// Gets or sets the name of the channel.
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the volume level of the channel (0.0 to 1.0).
        /// </summary>
        public double Volume
        {
            get => _volume;
            set
            {
                // Clamp the value between 0 and 1
                _volume = Math.Clamp(value, 0.0, 1.0);
                OnPropertyChanged();
                
                // Update the audio engine
                UpdateAudioEngine();
            }
        }

        /// <summary>
        /// Gets or sets the pan position of the channel (-1.0 to 1.0).
        /// </summary>
        public double Pan
        {
            get => _pan;
            set
            {
                // Clamp the value between -1 and 1
                _pan = Math.Clamp(value, -1.0, 1.0);
                OnPropertyChanged();
                
                // Update the audio engine
                UpdateAudioEngine();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the channel is muted.
        /// </summary>
        public bool IsMuted
        {
            get => _isMuted;
            set
            {
                _isMuted = value;
                OnPropertyChanged();
                
                // Update the audio engine
                UpdateAudioEngine();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the channel is soloed.
        /// </summary>
        public bool IsSolo
        {
            get => _isSolo;
            set
            {
                _isSolo = value;
                OnPropertyChanged();
                
                // Update the audio engine
                UpdateAudioEngine();
            }
        }

        /// <summary>
        /// Gets or sets the left channel level (0.0 to 1.0).
        /// </summary>
        public double LeftLevel
        {
            get => _leftLevel;
            set
            {
                _leftLevel = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the right channel level (0.0 to 1.0).
        /// </summary>
        public double RightLevel
        {
            get => _rightLevel;
            set
            {
                _rightLevel = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the peak left channel level (0.0 to 1.0).
        /// </summary>
        public double PeakLeftLevel
        {
            get => _peakLeftLevel;
            set
            {
                _peakLeftLevel = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the peak right channel level (0.0 to 1.0).
        /// </summary>
        public double PeakRightLevel
        {
            get => _peakRightLevel;
            set
            {
                _peakRightLevel = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the audio device.
        /// </summary>
        public AudioDeviceInfo Device => _device;

        /// <summary>
        /// Gets the audio port.
        /// </summary>
        public AudioPortInfo Port => _port;

        #endregion

        #region Commands

        /// <summary>
        /// Gets the command to reset the channel.
        /// </summary>
        public ICommand ResetCommand { get; }

        #endregion

        #region Command Methods

        private void ResetChannel()
        {
            // Reset channel properties to default values
            Volume = 0.75;
            Pan = 0.0;
            IsMuted = false;
            IsSolo = false;
            
            // Update the audio engine
            UpdateAudioEngine();
        }

        #endregion

        #region Private Methods

        private void UpdateAudioEngine()
        {
            try
            {
                // Update the channel properties in the audio engine
                _audioDeviceService.SetChannelPropertiesAsync(
                    _device.Id,
                    _port.Id,
                    _volume,
                    _pan,
                    _isMuted,
                    _isSolo);
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error updating audio engine: {ex.Message}");
            }
        }

        #endregion

        #region Event Handlers

        private void AudioDeviceService_LevelUpdated(object sender, AudioLevelEventArgs e)
        {
            // Check if this is for our port
            if (e.PortId == _port.Id)
            {
                // Update the level meters
                LeftLevel = e.LeftLevel;
                RightLevel = e.RightLevel;
                
                // Update peak levels if the current level is higher
                if (e.LeftLevel > PeakLeftLevel)
                {
                    PeakLeftLevel = e.LeftLevel;
                }
                
                if (e.RightLevel > PeakRightLevel)
                {
                    PeakRightLevel = e.RightLevel;
                }
            }
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
}
