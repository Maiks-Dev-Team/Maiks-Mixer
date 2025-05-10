using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using MaiksMixer.Core.Communication.Models;
using MaiksMixer.Core.Services;
using MaiksMixer.UI.ViewModels;

namespace MaiksMixer.UI.Views
{
    /// <summary>
    /// Interaction logic for DeviceManagementView.xaml
    /// </summary>
    public partial class DeviceManagementView : UserControl
    {
        private readonly DeviceManagementViewModel _viewModel;
        
        /// <summary>
        /// Initializes a new instance of the DeviceManagementView class.
        /// </summary>
        /// <param name="deviceService">The device service to use.</param>
        public DeviceManagementView(AudioDeviceService deviceService)
        {
            InitializeComponent();
            
            // Create view model
            _viewModel = new DeviceManagementViewModel(deviceService);
            DataContext = _viewModel;
            
            // Set up connection status indicator
            ConnectionStatus.SetStatus(ConnectionStatus.Connected);
            
            // Handle connection status events
            deviceService.DevicesUpdated += (s, e) => ConnectionStatus.SetStatus(ConnectionStatus.Connected);
        }
        
        /// <summary>
        /// Initializes a new instance of the DeviceManagementView class.
        /// This constructor is used by the designer.
        /// </summary>
        public DeviceManagementView()
        {
            InitializeComponent();
            
            // Design-time data
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                // Create a mock device service
                var deviceService = new AudioDeviceService(new MaiksMixer.Core.Communication.ConnectionManager("test"));
                
                // Create view model
                _viewModel = new DeviceManagementViewModel(deviceService);
                DataContext = _viewModel;
            }
        }
    }
    
    /// <summary>
    /// Converts a device status to a brush.
    /// </summary>
    public class DeviceStatusToBrushConverter : IValueConverter
    {
        /// <summary>
        /// Converts a device status to a brush.
        /// </summary>
        /// <param name="value">The device status.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="parameter">The converter parameter.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>A brush representing the device status.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AudioDeviceStatus status)
            {
                return status switch
                {
                    AudioDeviceStatus.Online => new SolidColorBrush(Colors.Green),
                    AudioDeviceStatus.Offline => new SolidColorBrush(Colors.Red),
                    AudioDeviceStatus.Error => new SolidColorBrush(Colors.Orange),
                    AudioDeviceStatus.Busy => new SolidColorBrush(Colors.Yellow),
                    AudioDeviceStatus.Initializing => new SolidColorBrush(Colors.Blue),
                    _ => new SolidColorBrush(Colors.Gray)
                };
            }
            
            return new SolidColorBrush(Colors.Gray);
        }
        
        /// <summary>
        /// Converts a brush back to a device status.
        /// </summary>
        /// <param name="value">The brush.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="parameter">The converter parameter.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>A device status.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    /// <summary>
    /// Converts a device type to a string.
    /// </summary>
    public class DeviceTypeToStringConverter : IValueConverter
    {
        /// <summary>
        /// Converts a device type to a string.
        /// </summary>
        /// <param name="value">The device type.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="parameter">The converter parameter.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>A string representing the device type.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AudioDeviceType type)
            {
                return type switch
                {
                    AudioDeviceType.PhysicalInterface => "Physical Audio Interface",
                    AudioDeviceType.VirtualDevice => "Virtual Audio Device",
                    AudioDeviceType.JackClient => "JACK Audio Client",
                    AudioDeviceType.AsioDevice => "ASIO Device",
                    AudioDeviceType.WasapiDevice => "WASAPI Device",
                    AudioDeviceType.DirectSoundDevice => "DirectSound Device",
                    _ => "Unknown Device Type"
                };
            }
            
            return "Unknown Device Type";
        }
        
        /// <summary>
        /// Converts a string back to a device type.
        /// </summary>
        /// <param name="value">The string.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="parameter">The converter parameter.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>A device type.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
