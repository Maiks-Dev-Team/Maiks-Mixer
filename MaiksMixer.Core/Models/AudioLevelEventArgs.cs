using System;

namespace MaiksMixer.Core.Models
{
    /// <summary>
    /// Event arguments for audio level updates.
    /// </summary>
    public class AudioLevelEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the ID of the port.
        /// </summary>
        public string PortId { get; }

        /// <summary>
        /// Gets the left channel level (0.0 to 1.0).
        /// </summary>
        public double LeftLevel { get; }

        /// <summary>
        /// Gets the right channel level (0.0 to 1.0).
        /// </summary>
        public double RightLevel { get; }

        /// <summary>
        /// Gets the timestamp of the level update.
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// Initializes a new instance of the AudioLevelEventArgs class.
        /// </summary>
        /// <param name="portId">The ID of the port.</param>
        /// <param name="leftLevel">The left channel level (0.0 to 1.0).</param>
        /// <param name="rightLevel">The right channel level (0.0 to 1.0).</param>
        public AudioLevelEventArgs(string portId, double leftLevel, double rightLevel)
        {
            PortId = portId;
            LeftLevel = Math.Clamp(leftLevel, 0.0, 1.0);
            RightLevel = Math.Clamp(rightLevel, 0.0, 1.0);
            Timestamp = DateTime.Now;
        }
    }
}
