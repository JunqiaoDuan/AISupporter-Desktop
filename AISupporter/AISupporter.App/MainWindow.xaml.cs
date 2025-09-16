using AISupporter.App.Helper;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AISupporter.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        #region Override

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);


        }

        #endregion

        #region Event

        private void CaptureButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var folderPath = "ScreenCapture";
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_ffff");
                var targetFilePath = System.IO.Path.Combine(folderPath, $"screen_capture_{timestamp}.png");

                using (var allScreensBitmap = ScreenCaptureHelper.CaptureAllScreens())
                {
                    ScreenCaptureHelper.CaptureAllScreensToFile(targetFilePath);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error capturing screen: {ex.Message}");
            }
        }
        
        #endregion

        #region Private

        #endregion

    }

}