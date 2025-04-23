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
using Syncfusion.UI.Xaml.Charts;
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
        public OverviewReportViewModel ViewModel { get; set; }

        public OverviewReportPage()
        {
            this.InitializeComponent();
            ViewModel = new OverviewReportViewModel();
            TimeRangeComboBox.SelectedIndex = 0;
        }

        private void TimeRangeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel.SelectedTimeRange is string selectedRange)
            {
                bool isCustom = selectedRange?.Trim() == "Tùy chỉnh";
                StartDatePicker.Visibility = isCustom ? Visibility.Visible : Visibility.Collapsed;
                EndDatePicker.Visibility = isCustom ? Visibility.Visible : Visibility.Collapsed;
                ApplyButton.Visibility = isCustom ? Visibility.Visible : Visibility.Collapsed;

                if (!isCustom)
                {
                    ViewModel.UpdateChartData(selectedRange);
                }
            }
        }

        private void ApplyCustomDateRange_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.UpdateChartData("Tùy chỉnh");
        }
    }
}
