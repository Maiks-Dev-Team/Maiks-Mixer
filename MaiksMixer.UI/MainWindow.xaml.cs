using System;
using System.Windows;
using MaiksMixer.UI.ViewModels;

namespace MaiksMixer.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        /// <summary>
        /// Gets the view model for this window.
        /// </summary>
        public MainViewModel ViewModel => (MainViewModel)DataContext;
    }
}