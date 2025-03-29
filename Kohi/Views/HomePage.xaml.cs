using Kohi.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;

//using Kohi.ViewModels;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Kohi.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>

    public sealed partial class HomePage : Page
    {
        private ProductViewModel ViewModel { get; set; }
        public HomePage()
        {
            this.InitializeComponent();
            ViewModel = new ProductViewModel();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Add product");
            ViewModel.AddProduct();
        }
    }
}
