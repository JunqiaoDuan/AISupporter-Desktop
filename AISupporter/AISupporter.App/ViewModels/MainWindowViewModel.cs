using AISupporter.App.Commands;
using AISupporter.App.Helper;
using AISupporter.ExternalService.AI.Interfaces;
using AISupporter.ExternalService.AI.Interfaces.Model;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
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

        private ObservableCollection<AIChatMessage> _messages = new();
        private string _currentMessage = string.Empty;
        private bool _isSending = false;

        public ObservableCollection<AIChatMessage> Messages
        {
            get => _messages;
            set { _messages = value; OnPropertyChanged(); }
        }

        public string CurrentMessage
        {
            get => _currentMessage;
            set { _currentMessage = value; OnPropertyChanged(); }
        }

        public bool IsSending
        {
            get => _isSending;
            set { _isSending = value; OnPropertyChanged(); }
        }

        public ICommand CaptureCommand { get; }
        public ICommand AICommand { get; }
        public ICommand SendMessageCommand { get; }

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
            SendMessageCommand = new AsyncRelayCommand(
                ExecuteSendMessage,
                () => !IsSending && !string.IsNullOrWhiteSpace(CurrentMessage));
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

                // Add the captured screenshot to chat
                Messages.Add(AIChatMessage.CreateUserMessage("I've captured a screenshot for analysis.", targetFilePath));
            }
            catch (Exception ex)
            {
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

        private async Task ExecuteSendMessage()
        {
            if (string.IsNullOrWhiteSpace(CurrentMessage) || IsSending)
                return;

            try
            {
                IsSending = true;
                var userMessage = CurrentMessage.Trim();

                // Take screenshot automatically when sending message
                string? screenshotPath = await TakeScreenshotAsync();

                // Add user message with screenshot
                Messages.Add(AIChatMessage.CreateUserMessage(userMessage, screenshotPath));
                CurrentMessage = string.Empty;

                // Get AI response
                var modelProvider = await _aiModelService.GetDefaultModel("");
                if (modelProvider != null)
                {
                    var aiService = _aiServiceFactory.GetService(modelProvider.Provider);
                    var messageList = Messages.ToList();
                    await aiService.ChatAsync(messageList, modelProvider.Code);

                    // Update the collection with new messages
                    Messages.Clear();
                    foreach (var msg in messageList)
                    {
                        Messages.Add(msg);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending message: {ex.Message}");
            }
            finally
            {
                IsSending = false;
            }
        }

        private async Task<string?> TakeScreenshotAsync()
        {
            return await Task.Run(() =>
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
                    return targetFilePath;
                }
                catch
                {
                    return null;
                }
            });
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
