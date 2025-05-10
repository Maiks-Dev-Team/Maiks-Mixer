using System;

namespace MaiksMixer.Core.Communication.Models
{
    /// <summary>
    /// Represents a connection between two audio ports.
    /// </summary>
    public class AudioConnection
    {
        /// <summary>
        /// Gets or sets the unique identifier for the connection.
        /// </summary>
        public string Id { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the source port ID.
        /// </summary>
        public string SourceId { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the destination port ID.
        /// </summary>
        public string DestinationId { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the connection status.
        /// </summary>
        public ConnectionStatus Status { get; set; } = ConnectionStatus.Connected;
        
        /// <summary>
        /// Gets or sets the volume level of the connection (0.0 to 1.0).
        /// </summary>
        public double Volume { get; set; } = 1.0;
        
        /// <summary>
        /// Gets or sets whether the connection is active.
        /// </summary>
        public bool IsActive => Status == ConnectionStatus.Connected;
        
        /// <summary>
        /// Gets or sets the timestamp when the connection was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Gets or sets the timestamp when the connection was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Creates a new instance of the AudioConnection class.
        /// </summary>
        public AudioConnection()
        {
            Id = Guid.NewGuid().ToString();
        }
        
        /// <summary>
        /// Creates a new instance of the AudioConnection class with the specified source and destination.
        /// </summary>
        /// <param name="sourceId">The source port ID.</param>
        /// <param name="destinationId">The destination port ID.</param>
        public AudioConnection(string sourceId, string destinationId)
        {
            Id = Guid.NewGuid().ToString();
            SourceId = sourceId;
            DestinationId = destinationId;
        }
    }
    
    /// <summary>
    /// Represents the status of an audio connection.
    /// </summary>
    public enum ConnectionStatus
    {
        /// <summary>
        /// Connection is active.
        /// </summary>
        Connected,
        
        /// <summary>
        /// Connection is muted.
        /// </summary>
        Muted,
        
        /// <summary>
        /// Connection is disconnected.
        /// </summary>
        Disconnected
    }
}
