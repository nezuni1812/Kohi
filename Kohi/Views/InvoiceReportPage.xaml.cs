using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Kohi.ViewModels;
using Kohi.Models;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Kohi.Views
{
    public sealed partial class InvoiceReportPage : Page
    {
        public InvoiceReportViewModel ViewModel { get; set; }

        public InvoiceReportPage()
        {
            this.InitializeComponent();
            ViewModel = new InvoiceReportViewModel();
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

        private async void InvoiceListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListView listView && listView.SelectedItem is InvoiceModel selectedInvoice)
            {
                try
                {
                    var detailedInvoice = await ViewModel.GetInvoiceDetailsAsync(selectedInvoice.Id);
                    if (detailedInvoice != null)
                    {
                        Frame.Navigate(typeof(PrintInvoicePage), detailedInvoice);
                    }
                    else
                    {
                        await ShowErrorContentDialog(this.XamlRoot, "Hóa đơn chi tiết bị lỗi.");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error fetching invoice details: {ex.Message}");
                }
            }
        }
        private async Task ShowErrorContentDialog(XamlRoot xamlRoot, string errorMessage)
        {
            ContentDialog errorDialog = new ContentDialog
            {
                Title = "Lỗi",
                Content = errorMessage,
                CloseButtonText = "Đóng",
                XamlRoot = xamlRoot
            };

            await errorDialog.ShowAsync();
        }
    }
}