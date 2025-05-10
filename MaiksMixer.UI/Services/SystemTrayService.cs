using System;
using System.Windows;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Application = System.Windows.Application;
using ContextMenu = System.Windows.Forms.ContextMenu;
using MenuItem = System.Windows.Forms.MenuItem;

namespace MaiksMixer.UI.Services
{
    /// <summary>
    /// Service for managing system tray icon and functionality.
    /// </summary>
    public class SystemTrayService : IDisposable
    {
        private readonly NotifyIcon _notifyIcon;
        private readonly Window _mainWindow;
        private bool _isExiting;

        /// <summary>
        /// Initializes a new instance of the SystemTrayService class.
        /// </summary>
        /// <param name="mainWindow">The main window of the application.</param>
        public SystemTrayService(Window mainWindow)
        {
            _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
            
            // Create system tray icon
            _notifyIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application, // Replace with your own icon
                Text = "MaiksMixer",
                Visible = true
            };

            // Create context menu for system tray icon
            var contextMenu = new ContextMenu();
            
            // Show/Hide menu item
            var showHideMenuItem = new MenuItem("Show/Hide", OnShowHideClick);
            contextMenu.MenuItems.Add(showHideMenuItem);
            
            // Separator
            contextMenu.MenuItems.Add("-");
            
            // Exit menu item
            var exitMenuItem = new MenuItem("Exit", OnExitClick);
            contextMenu.MenuItems.Add(exitMenuItem);

            // Assign context menu to notify icon
            _notifyIcon.ContextMenu = contextMenu;
            
            // Double-click to show/hide
            _notifyIcon.DoubleClick += (sender, args) => OnShowHideClick(sender, args);
            
            // Handle main window closing
            _mainWindow.Closing += MainWindow_Closing;
        }

        /// <summary>
        /// Handles the main window closing event.
        /// </summary>
        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            if (!_isExiting)
            {
                e.Cancel = true;
                _mainWindow.Hide();
            }
        }

        /// <summary>
        /// Shows or hides the main window.
        /// </summary>
        private void OnShowHideClick(object sender, EventArgs e)
        {
            if (_mainWindow.IsVisible)
            {
                _mainWindow.Hide();
            }
            else
            {
                _mainWindow.Show();
                _mainWindow.WindowState = WindowState.Normal;
                _mainWindow.Activate();
            }
        }

        /// <summary>
        /// Exits the application.
        /// </summary>
        private void OnExitClick(object sender, EventArgs e)
        {
            _isExiting = true;
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Disposes the system tray icon.
        /// </summary>
        public void Dispose()
        {
            _notifyIcon.Dispose();
        }
    }
}
