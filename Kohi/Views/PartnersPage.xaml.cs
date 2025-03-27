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
using System.Threading.Tasks;
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
                addButton.Click -= showAddSupplierDialog_Click;
                addButton.Click += showAddCustomerDialog_Click;

                deleteButtonTextBlock.Text = "Xóa khách hàng";
                deleteButton.Click -= ShowDeleteSupplierDialog;
                deleteButton.Click += ShowDeleteCustomerDialog;

                editButtonTextBlock.Text = "Chỉnh sửa khách hàng";
            }
            else if (sender.SelectedItem == SupplierSelectorBar)
            {
                MyTableView.ItemsSource = SupplierViewModel.Suppliers;
                addButtonTextBlock.Text = "Thêm nhà cung cấp";
                addButton.Click -= showAddCustomerDialog_Click;
                addButton.Click += showAddSupplierDialog_Click;

                deleteButtonTextBlock.Text = "Xóa nhà cung cấp";
                deleteButton.Click -= ShowDeleteCustomerDialog;
                deleteButton.Click += ShowDeleteSupplierDialog;

                editButtonTextBlock.Text = "Chỉnh sửa nhà cung cấp";

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

        private void addButton_click(object sender, RoutedEventArgs e)
        {

        }

        private void SupplierDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = true;
        }

        private void SupplierDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }
        private void CustomerDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = true;
        }

        private void CustomerDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }

        public async void showAddSupplierDialog_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("showAddSupplierDialog_Click triggered");
            var result = await SupplierDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {

            }
            else
            {

            }
        }

        public async void showAddCustomerDialog_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("showAddCustomerDialog_Click triggered");
            var result = await CustomerDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {

            }
            else
            {

            }
        }

        public async void ShowDeleteSupplierDialog(object sender, RoutedEventArgs e)
        {
            var deleteDialog = new ContentDialog
            {
                Title = "Xác nhận xóa",
                Content = "Bạn có chắc chắn muốn xóa nhà cung cấp này không? Lưu ý hành động này không thể hoàn tác.",
                PrimaryButtonText = "Xóa",
                CloseButtonText = "Hủy",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            var result = await deleteDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                Debug.WriteLine("Đã xóa nhà cung cấp");
            }
            else
            {
                Debug.WriteLine("Hủy xóa nhà cung cấp");
            }
        }

        public async void ShowDeleteCustomerDialog(object sender, RoutedEventArgs e)
        {
            var deleteDialog = new ContentDialog
            {
                Title = "Xác nhận xóa",
                Content = "Bạn có chắc chắn muốn xóa khách hàng này không? Lưu ý hành động này không thể hoàn tác.",
                PrimaryButtonText = "Xóa",
                CloseButtonText = "Hủy",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            var result = await deleteDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                Debug.WriteLine("Đã xóa khách hàng");
            }
            else
            {
                Debug.WriteLine("Hủy xóa khách hàng");
            }
        }
    }
}