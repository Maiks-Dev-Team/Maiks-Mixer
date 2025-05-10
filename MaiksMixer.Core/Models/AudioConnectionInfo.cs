using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MaiksMixer.Core.Models
{
    /// <summary>
    /// Represents the status of an audio connection.
    /// </summary>
    public enum ConnectionStatus
    {
        /// <summary>
        /// Connection is active and working properly.
        /// </summary>
        Active,
        
        /// <summary>
        /// Connection is inactive but available.
        /// </summary>
        Inactive,
        
        /// <summary>
        /// Connection is pending creation or update.
        /// </summary>
        Pending,
        
        /// <summary>
        /// Connection has an error.
        /// </summary>
        Error,
        
        /// <summary>
        /// Connection status is unknown.
        /// </summary>
        Unknown
    }
    
    /// <summary>
    /// Represents a connection between two audio ports in the routing matrix.
    /// </summary>
    public class AudioConnectionInfo : INotifyPropertyChanged
    {
        private string _id;
        private string _sourceId;
        private string _destinationId;
        private string _sourceName;
        private string _destinationName;
        private bool _isConnected;
        private double _volume;
        private ConnectionStatus _status;
        private int _sourceRow;
        private int _destinationColumn;
        
        /// <summary>
        /// Gets or sets the unique identifier for the connection.
        /// </summary>
        public string Id
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Gets or sets the ID of the source port.
        /// </summary>
        public string SourceId
        {
            get => _sourceId;
            set
            {
                _sourceId = value;
                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Gets or sets the ID of the destination port.
        /// </summary>
        public string DestinationId
        {
            get => _destinationId;
            set
            {
                _destinationId = value;
                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Gets or sets the name of the source port.
        /// </summary>
        public string SourceName
        {
            get => _sourceName;
            set
            {
                _sourceName = value;
                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Gets or sets the name of the destination port.
        /// </summary>
        public string DestinationName
        {
            get => _destinationName;
            set
            {
                _destinationName = value;
                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether the connection is active.
        /// </summary>
        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                _isConnected = value;
                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Gets or sets the volume level of the connection (0.0 to 1.0).
        /// </summary>
        public double Volume
        {
            get => _volume;
            set
            {
                _volume = Math.Clamp(value, 0.0, 1.0);
                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Gets or sets the status of the connection.
        /// </summary>
        public ConnectionStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Gets or sets the row index of the source in the routing matrix.
        /// </summary>
        public int SourceRow
        {
            get => _sourceRow;
            set
            {
                _sourceRow = value;
                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Gets or sets the column index of the destination in the routing matrix.
        /// </summary>
        public int DestinationColumn
        {
            get => _destinationColumn;
            set
            {
                _destinationColumn = value;
                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Initializes a new instance of the AudioConnectionInfo class.
        /// </summary>
        public AudioConnectionInfo()
        {
            Id = Guid.NewGuid().ToString();
            SourceId = string.Empty;
            DestinationId = string.Empty;
            SourceName = string.Empty;
            DestinationName = string.Empty;
            IsConnected = false;
            Volume = 1.0;
            Status = ConnectionStatus.Inactive;
            SourceRow = 0;
            DestinationColumn = 0;
        }
        
        /// <summary>
        /// Creates a deep copy of this audio connection info.
        /// </summary>
        /// <returns>A new AudioConnectionInfo instance with the same values.</returns>
        public AudioConnectionInfo Clone()
        {
            return new AudioConnectionInfo
            {
                Id = this.Id,
                SourceId = this.SourceId,
                DestinationId = this.DestinationId,
                SourceName = this.SourceName,
                DestinationName = this.DestinationName,
                IsConnected = this.IsConnected,
                Volume = this.Volume,
                Status = this.Status,
                SourceRow = this.SourceRow,
                DestinationColumn = this.DestinationColumn
            };
        }
        
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
    }
}
