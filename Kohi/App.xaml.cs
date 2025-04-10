﻿using System;
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
//using Syncfusion.Licensing;


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
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            //SyncfusionLicenseProvider.RegisterLicense("Mzc3MDU4OEAzMjM4MmUzMDJlMzBsZ211SjlybUsyRzl4UlZ2a2cxMEFVWVZ4MkkvcG9kU1hWYXJaUDhsbDhjPQ==");
            m_window = new MainWindow();
            m_window.Activate();
        }

        private Window? m_window;
    }
}
