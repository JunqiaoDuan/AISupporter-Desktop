using AISupporter.App.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace AISupporter.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        // Add this method to set focus when window is loaded
        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            // Focus the input textbox when window loads
            MessageTextBox.Focus();
        }

        private void ScreenshotIcon_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is AISupporter.ExternalService.AI.Interfaces.Model.AIChatMessage msg)
            {
                if (!string.IsNullOrEmpty(msg.ImagePath))
                {
                    OpenWithDefaultViewer(msg.ImagePath);
                }
            }
        }

        private void OpenWithDefaultViewer(string imagePath)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = imagePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open image: {ex.Message}");
            }
        }
    }
}
