using Kohi.Models;
using Kohi.ViewModels;
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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinUI.TableView;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Kohi.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PartnersPage : Page
    {
        public CustomerViewModel CustomerViewModel { get; set; } = new CustomerViewModel();
        public SupplierViewModel SupplierViewModel { get; set; } = new SupplierViewModel();
        public PartnersPage()
        {
            this.InitializeComponent();
            MyTableView.ItemsSource = CustomerViewModel.Customers;
            addButtonTextBlock.Text = "Thêm khách hàng";
        }
        private void SelectorBar_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
        {
            if (sender.SelectedItem == CustomerSelectorBar)
            {
                MyTableView.ItemsSource = CustomerViewModel.Customers;
                addButtonTextBlock.Text = "Thêm khách hàng";
                addButton.Click -= AddSupplier_Click;
                addButton.Click += AddCustomer_Click;
            }
            else if (sender.SelectedItem == SupplierSelectorBar)
            {
                MyTableView.ItemsSource = SupplierViewModel.Suppliers;
                addButtonTextBlock.Text = "Thêm nhà cung cấp";
                addButton.Click -= AddCustomer_Click;
                addButton.Click += AddSupplier_Click;
            }
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is TableView tableView && tableView.SelectedItem is SupplierModel selectedSupplier)
            {
                int id = selectedSupplier.Id;
                Debug.WriteLine($"Selected ID: {id}");
            }
        }

        private void AddCustomer_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AddNewCustomerPage));
        }

        private void AddSupplier_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AddNewSupplierPage));
        }
        private void addButton_click(object sender, RoutedEventArgs e)
        {
            //Frame rootFrame = new Frame();
            //this.Content = rootFrame;

            //rootFrame.Navigate(typeof(AddNewSupplierPage), null);
        }
    }
}