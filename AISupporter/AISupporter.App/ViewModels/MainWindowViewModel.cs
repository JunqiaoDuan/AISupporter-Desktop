using AISupporter.App.Commands;
using AISupporter.App.Helper;
using AISupporter.ExternalService.AI.Interfaces;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AISupporter.App.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        #region Event&Fields&Properties

        public event PropertyChangedEventHandler? PropertyChanged;

        private readonly IAIModelService _aiModelService;
        private readonly IAIServiceFactory _aiServiceFactory;

        public ICommand CaptureCommand { get; }
        public ICommand AICommand { get; }

        #endregion

        #region Constructor

        public MainWindowViewModel(
            IAIModelService aiModelService,
            IAIServiceFactory aiServiceFactory)
        {
            // Dependency Injection
            _aiModelService = aiModelService;
            _aiServiceFactory = aiServiceFactory;

            // Initialize Commands
            CaptureCommand = new RelayCommand(ExecuteCapture);
            AICommand = new AsyncRelayCommand(ExecuteAI);
        }

        #endregion

        #region Commands

        private void ExecuteCapture()
        {
            try
            {
                var folderPath = "ScreenCapture";
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_ffff");
                var targetFilePath = Path.Combine(folderPath, $"screen_capture_{timestamp}.png");

                using (var allScreensBitmap = ScreenCaptureHelper.CaptureAllScreens())
                {
                    ScreenCaptureHelper.CaptureAllScreensToFile(targetFilePath);
                }
            }
            catch (Exception ex)
            {
                // In a proper MVVM implementation, you might want to use a message service
                // For now, keeping the MessageBox for simplicity
                MessageBox.Show($"Error capturing screen: {ex.Message}");
            }
        }

        private async Task ExecuteAI()
        {
            try
            {
                var modelProvider = await _aiModelService.GetDefaultModel("");
                if (modelProvider == null)
                {
                    return;
                }
                var _aiService = _aiServiceFactory.GetService(modelProvider.Provider);
                _aiService.Test();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error running AI command: {ex.Message}");
            }
        }

        #endregion

        #region Protected Methods

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
