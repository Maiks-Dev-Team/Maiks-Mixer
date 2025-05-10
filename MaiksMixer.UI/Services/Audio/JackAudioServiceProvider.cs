using System;

namespace MaiksMixer.UI.Services.Audio
{
    /// <summary>
    /// Service provider for JACK audio services
    /// </summary>
    public static class JackAudioServiceProvider
    {
        /// <summary>
        /// Creates and returns a new instance of the JACK audio service
        /// </summary>
        /// <returns>An instance of IJackAudioService</returns>
        public static IJackAudioService CreateJackAudioService()
        {
            return new JackAudioServiceImpl();
        }
    }
}
