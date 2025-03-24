using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.Linq;
using WinUI.TableView;
using Kohi.ViewModels;
using Kohi.Models;
using Windows.Gaming.Preview.GamesEnumeration;

namespace Kohi.Views
{
    public sealed partial class CustomersPage : Page
    {
        public CustomerViewModel CustomerViewModel { get; set; } = new CustomerViewModel();

        public CustomersPage()
        {
            this.InitializeComponent();
            Loaded += CustomersPage_Loaded;
        }

        public async void CustomersPage_Loaded(object sender, RoutedEventArgs e)
        {
            await CustomerViewModel.LoadData(); // Tải trang đầu tiên
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
            // Logic thêm khách hàng
        }

        public void UpdatePageList()
        {
            if (CustomerViewModel == null) return;
            pageList.ItemsSource = Enumerable.Range(1, CustomerViewModel.TotalPages);
            pageList.SelectedItem = CustomerViewModel.CurrentPage;
        }

        public async void OnPageSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CustomerViewModel == null || pageList.SelectedItem == null) return;

            var selectedPage = (int)pageList.SelectedItem;
            if (selectedPage != CustomerViewModel.CurrentPage)
            {
                await CustomerViewModel.LoadData(selectedPage);
                UpdatePageList();
            }
        }
    }
}