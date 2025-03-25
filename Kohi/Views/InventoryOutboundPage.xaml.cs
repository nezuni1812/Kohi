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
using Kohi.Models;
using Kohi.ViewModels;
using System.Diagnostics;
using WinUI.TableView;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Kohi.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class InventoryOutboundPage : Page
    {
        public OutboundViewModel OutboundViewModel { get; set; } = new OutboundViewModel();
        public InventoryOutboundPage()
        {
            this.InitializeComponent();
            Loaded += OutboundsPage_Loaded;
            //GridContent.DataContext = IncomeViewModel;
        }
        public async void OutboundsPage_Loaded(object sender, RoutedEventArgs e)
        {
            await OutboundViewModel.LoadData(); // Tải trang đầu tiên
            UpdatePageList();
        }

        public void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is TableView tableView && tableView.SelectedItem is CustomerModel selectedCustomer)
            {
                int id = selectedCustomer.Id;
                Debug.WriteLine($"Selected Customer ID: {id}");
            }
        }

        public void addButton_click(object sender, RoutedEventArgs e)
        {
            Frame rootFrame = new Frame();
            this.Content = rootFrame;

            rootFrame.Navigate(typeof(AddNewInventoryOutboundPage), null);
        }

        public void UpdatePageList()
        {
            if (OutboundViewModel == null) return;
            pageList.ItemsSource = Enumerable.Range(1, OutboundViewModel.TotalPages);
            pageList.SelectedItem = OutboundViewModel.CurrentPage;
        }

        public async void OnPageSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OutboundViewModel == null || pageList.SelectedItem == null) return;

            var selectedPage = (int)pageList.SelectedItem;
            if (selectedPage != OutboundViewModel.CurrentPage)
            {
                await OutboundViewModel.LoadData(selectedPage);
                UpdatePageList();
            }
        }
    }
}
