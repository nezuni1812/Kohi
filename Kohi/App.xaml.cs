using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Kohi.Views;
using Kohi.Services;
using Syncfusion.Licensing;
using System.Net.Http;
using System.Threading.Tasks;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Kohi
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public static Window MainWindow { get; set; }

        public App()
        {
            this.InitializeComponent();
            Service.AddKeyedSingleton<IDao, APIDao>();
            MainWindow = new MainWindow();

        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            SyncfusionLicenseProvider.RegisterLicense("");
            m_window = MainWindow;
            m_window.Activate();

            await CheckDatabaseConnectionAsync();
        }

        private async Task CheckDatabaseConnectionAsync()
        {
            try
            {
                using var client = new HttpClient();
                var response = await client.GetAsync("http://localhost:3000/categories");

                if (!response.IsSuccessStatusCode)
                {
                    await ShowErrorDialogAndExit("Không thể kết nối cơ sở dữ liệu.");
                }
            }
            catch
            {
                await ShowErrorDialogAndExit("Không thể kết nối cơ sở dữ liệu.");
            }
        }

        private async Task ShowErrorDialogAndExit(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Lỗi",
                Content = message,
                CloseButtonText = "Xác nhận",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = MainWindow.Content.XamlRoot
            };

            await dialog.ShowAsync();
            Exit();
        }

        private Window? m_window;
    }
}
