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
                // This opens the image with Windows default viewer (Photos app, etc.)
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = imagePath,
                    UseShellExecute = true // This is key for opening with default application
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open image: {ex.Message}");
            }
        }

    }
}
