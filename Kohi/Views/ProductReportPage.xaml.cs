﻿using System;
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
using Kohi.ViewModels;
using Kohi.Services;
using System.Diagnostics;
using System.Collections.ObjectModel;
using Kohi.Services;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Kohi.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProductReportPage : Page
    {
        ProductReportPageViewModel ViewModel { get; set; }

        //public class Model
        //{
        //    public double XValue { get; set; }
        //    public double YValue { get; set; }
        //}

        //public ObservableCollection<Model> Data { get; set; }
        public ProductReportPage()
        {
            this.InitializeComponent();
            Debug.WriteLine("Before viewmodel");
            ViewModel = new ProductReportPageViewModel();
            //Data = new ObservableCollection<Model>();
            //Data.Add(new Model() { XValue = 10, YValue = 100 });
            //Data.Add(new Model() { XValue = 20, YValue = 150 });
            //Data.Add(new Model() { XValue = 30, YValue = 110 });
            //Data.Add(new Model() { XValue = 40, YValue = 230 });
            Debug.WriteLine("After viewmodel");
        }

        private void SfAIAssistView_SuggestionSelected(object sender, Syncfusion.UI.Xaml.Chat.SuggestionClickedEventArgs e)
        {
            ViewModel.HandleSuggestionClicked(e.Item.ToString());
            //Debug.WriteLine(e.Item.ToString());
        }

        private void TimeRangeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TimeRangeComboBox.SelectedItem as string == "Tùy chỉnh")
            {
                StartDatePicker.Visibility = Visibility.Visible;
                EndDatePicker.Visibility = Visibility.Visible;
                ApplyButton.Visibility = Visibility.Visible;

                // Set default custom range to current month if not set
                if (ViewModel.StartDate == null)
                {
                    ViewModel.StartDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                }
                if (ViewModel.EndDate == null)
                {
                    ViewModel.EndDate = DateTime.Today;
                }
            }
            else
            {
                StartDatePicker.Visibility = Visibility.Collapsed;
                EndDatePicker.Visibility = Visibility.Collapsed;
                ApplyButton.Visibility = Visibility.Collapsed;
            }
        }

        private async void ApplyCustomDateRange_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.StartDate.HasValue && ViewModel.EndDate.HasValue)
            {
                if (ViewModel.StartDate.Value > ViewModel.EndDate.Value)
                {
                    await new ContentDialog
                    {
                        Title = "Lỗi",
                        Content = "Ngày bắt đầu không thể sau ngày kết thúc.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    }.ShowAsync();
                    return;
                }

                // Trigger data reload with custom range
                await ViewModel.LoadDataAsync();
            }
            else
            {
                await new ContentDialog
                {
                    Title = "Lỗi",
                    Content = "Vui lòng chọn cả ngày bắt đầu và ngày kết thúc.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                }.ShowAsync();
            }
        }
    }
}
