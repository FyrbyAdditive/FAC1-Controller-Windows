using System;
using System.Windows;

namespace FAC1_Controller_Windows
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Set up global exception handling
            this.DispatcherUnhandledException += (sender, args) =>
            {
                MessageBox.Show($"An unexpected error occurred:\n{args.Exception.Message}", 
                    "FAC1 Controller Error", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                args.Handled = true;
            };
        }
    }
}