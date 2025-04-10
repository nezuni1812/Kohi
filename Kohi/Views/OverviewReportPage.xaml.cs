using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
//using Syncfusion.UI.Xaml.Charts;
using Kohi.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Kohi.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class OverviewReportPage : Page
    {
        public ChartViewModel chartViewModel { get; set; } = new ChartViewModel();
        public OverviewReportPage()
        {
            this.InitializeComponent();

            //SfCircularChart chart = new SfCircularChart();

            //chart.DataContext = chartViewModel;
            this.DataContext = chartViewModel;
        }
    }
}
