using System;
using System.Windows;

namespace MaiksMixer.UI.Services
{
    /// <summary>
    /// Service factory for creating and managing application services.
    /// </summary>
    public static class ServiceFactory
    {
        /// <summary>
        /// Creates a system tray service for the specified window.
        /// </summary>
        /// <param name="mainWindow">The main application window.</param>
        /// <returns>A new SystemTrayService instance.</returns>
        public static SystemTrayService CreateSystemTrayService(Window mainWindow)
        {
            return new SystemTrayService(mainWindow);
        }
    }
}
