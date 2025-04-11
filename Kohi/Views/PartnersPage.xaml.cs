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
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinUI.TableView;
using static System.Net.Mime.MediaTypeNames;

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
        private SupplierModel selectedSupplier;
        private CustomerModel selectedCustomer;

        public PartnersPage()
        {
            this.InitializeComponent();
            //MyTableView.ItemsSource = CustomerViewModel.Customers;
            addButtonTextBlock.Text = "Thêm khách hàng";
            editButton.Click += EditButton_Click; // Thêm sự kiện cho nút chỉnh sửa
            Loaded += PartnersPage_Loaded; // Thêm sự kiện Loaded
        }
        private async void PartnersPage_Loaded(object sender, RoutedEventArgs e)
        {
            await CustomerViewModel.LoadData(); // Tải trang đầu tiên cho Customers
            MyTableView.ItemsSource = CustomerViewModel.Customers;
            addButtonTextBlock.Text = "Thêm khách hàng";
            UpdatePageList(); // Cập nhật phân trang ban đầu
        }
        private void UpdatePageList()
        {
            if (SelectorBar.SelectedItem == CustomerSelectorBar)
            {
                pageList.ItemsSource = Enumerable.Range(1, CustomerViewModel.TotalPages);
                pageList.SelectedItem = CustomerViewModel.CurrentPage;
            }
            else if (SelectorBar.SelectedItem == SupplierSelectorBar)
            {
                pageList.ItemsSource = Enumerable.Range(1, SupplierViewModel.TotalPages);
                pageList.SelectedItem = SupplierViewModel.CurrentPage;
            }
        }

        private async void OnPageSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pageList.SelectedItem == null) return;

            var selectedPage = (int)pageList.SelectedItem;
            if (SelectorBar.SelectedItem == CustomerSelectorBar && selectedPage != CustomerViewModel.CurrentPage)
            {
                await CustomerViewModel.LoadData(selectedPage);
                MyTableView.ItemsSource = CustomerViewModel.Customers;
                UpdatePageList();
            }
            else if (SelectorBar.SelectedItem == SupplierSelectorBar && selectedPage != SupplierViewModel.CurrentPage)
            {
                await SupplierViewModel.LoadData(selectedPage);
                MyTableView.ItemsSource = SupplierViewModel.Suppliers;
                UpdatePageList();
            }
        }
        private async void SelectorBar_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
        {
            if (sender.SelectedItem == CustomerSelectorBar)
            {
                await CustomerViewModel.LoadData(); // Tải trang đầu tiên khi chuyển tab
                MyTableView.ItemsSource = CustomerViewModel.Customers;
                addButtonTextBlock.Text = "Thêm khách hàng";
                addButton.Click -= showAddSupplierDialog_Click;
                addButton.Click += showAddCustomerDialog_Click;

                deleteButtonTextBlock.Text = "Xóa khách hàng";
                deleteButton.Click -= ShowDeleteSupplierDialog;
                deleteButton.Click += ShowDeleteCustomerDialog;

                editButtonTextBlock.Text = "Chỉnh sửa khách hàng";
                selectedSupplier = null;
                UpdatePageList();
            }
            else if (sender.SelectedItem == SupplierSelectorBar)
            {
                await SupplierViewModel.LoadData(); // Tải trang đầu tiên khi chuyển tab
                MyTableView.ItemsSource = SupplierViewModel.Suppliers;
                addButtonTextBlock.Text = "Thêm nhà cung cấp";
                addButton.Click -= showAddCustomerDialog_Click;
                addButton.Click += showAddSupplierDialog_Click;

                deleteButtonTextBlock.Text = "Xóa nhà cung cấp";
                deleteButton.Click -= ShowDeleteCustomerDialog;
                deleteButton.Click += ShowDeleteSupplierDialog;

                editButtonTextBlock.Text = "Chỉnh sửa nhà cung cấp";
                selectedCustomer = null;
                UpdatePageList();
            }
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectorBar.SelectedItem == CustomerSelectorBar && selectedCustomer != null)
            {
                EditCustomerNameTextBox.Text = selectedCustomer.Name;
                EditCustomerAddressTextBox.Text = selectedCustomer.Phone;
                EditCustomerPhoneNumberTextBox.Text = selectedCustomer.Email;
                EditCustomerEmailTextBox.Text = selectedCustomer.Address;
                var result = await CustomerEditDialog.ShowAsync();
                // Điền dữ liệu vào CustomerEditDialog

                if (result == ContentDialogResult.Primary)
                {
                    // Cập nhật dữ liệu sau khi chỉnh sửa
                    selectedCustomer.Name = EditCustomerNameTextBox.Text;
                    selectedCustomer.Phone = EditCustomerAddressTextBox.Text;
                    selectedCustomer.Email = EditCustomerPhoneNumberTextBox.Text;
                    selectedCustomer.Address = EditCustomerEmailTextBox.Text;

                    await CustomerViewModel.Update(selectedCustomer.Id.ToString(), selectedCustomer);
                }
            }
            else if (SelectorBar.SelectedItem == SupplierSelectorBar && selectedSupplier != null)
            {
                // Điền dữ liệu vào SupplierEditDialog
                EditSupplierNameTextBox.Text = selectedSupplier.Name;
                EditSupplierPhoneNumberTextBox.Text = selectedSupplier.Phone;
                EditSupplierEmailTextBox.Text = selectedSupplier.Email;
                EditSupplierAddressTextBox.Text = selectedSupplier.Address;

                var result = await SupplierEditDialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    // Cập nhật dữ liệu sau khi chỉnh sửa
                    selectedSupplier.Name = EditSupplierNameTextBox.Text;
                    selectedSupplier.Phone = EditSupplierPhoneNumberTextBox.Text;
                    selectedSupplier.Email = EditSupplierEmailTextBox.Text;
                    selectedSupplier.Address = EditSupplierAddressTextBox.Text;

                    await SupplierViewModel.Update(selectedSupplier.Id.ToString(), selectedSupplier);
                }
            }
            else
            {
                // Hiển thị thông báo nếu chưa chọn mục nào
                var warningDialog = new ContentDialog
                {
                    Title = "Cảnh báo",
                    Content = "Vui lòng chọn một mục để chỉnh sửa.",
                    CloseButtonText = "Đóng",
                    XamlRoot = this.XamlRoot
                };
                await warningDialog.ShowAsync();
            }
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is TableView tableView && tableView.SelectedItem != null)
            {
                if (SelectorBar.SelectedItem == CustomerSelectorBar && tableView.SelectedItem is CustomerModel customer)
                {
                    selectedCustomer = customer;
                    Debug.WriteLine($"Selected Customer ID: {selectedCustomer.Id}");
                }
                else if (SelectorBar.SelectedItem == SupplierSelectorBar && tableView.SelectedItem is SupplierModel supplier)
                {
                    selectedSupplier = supplier;
                    Debug.WriteLine($"Selected Supplier ID: {selectedSupplier.Id}");
                }
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
            SupplierNameTextBox.Text = "";
            SupplierAddressTextBox.Text = "";
            SupplierPhoneNumberTextBox.Text = "";
            SupplierEmailTextBox.Text = "";
            var result = await SupplierDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var newSupplier = new SupplierModel
                {
                    Name = SupplierNameTextBox.Text,
                    Address = SupplierAddressTextBox.Text,
                    Phone = SupplierPhoneNumberTextBox.Text,
                    Email = SupplierEmailTextBox.Text,
                };
                await SupplierViewModel.Add(newSupplier);
            }
            else
            {

            }
        }

        public async void showAddCustomerDialog_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("showAddCustomerDialog_Click triggered");
            CustomerNameTextBox.Text = "";
            CustomerAddressTextBox.Text = "";
            CustomerPhoneNumberTextBox.Text = "";
            CustomerEmailTextBox.Text = "";
            var result = await CustomerDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var newCustomer = new CustomerModel
                {
                    Name = CustomerNameTextBox.Text,
                    Address = CustomerAddressTextBox.Text,
                    Phone = CustomerPhoneNumberTextBox.Text,
                    Email = CustomerEmailTextBox.Text,
                };
                await CustomerViewModel.Add(newCustomer);

                Debug.WriteLine("In info: " + newCustomer.Id);
            }

            else
            {

            }
        }

        public async void ShowDeleteSupplierDialog(object sender, RoutedEventArgs e)
        {
            
            if (SelectorBar.SelectedItem == SupplierSelectorBar && selectedSupplier != null)
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
                    await SupplierViewModel.Delete(selectedSupplier.Id.ToString());
                    selectedSupplier = null;
                    Debug.WriteLine("Đã xóa nhà cung cấp");
                }
                else
                {
                    Debug.WriteLine("Hủy xóa nhà cung cấp");
                }
            }
            else
            {
                // Hiển thị thông báo nếu chưa chọn mục nào
                var warningDialog = new ContentDialog
                {
                    Title = "Cảnh báo",
                    Content = "Vui lòng chọn một mục để xóa.",
                    CloseButtonText = "Đóng",
                    XamlRoot = this.XamlRoot
                };
                await warningDialog.ShowAsync();
            }
        }

        public async void ShowDeleteCustomerDialog(object sender, RoutedEventArgs e)
        {
            if (SelectorBar.SelectedItem == CustomerSelectorBar && selectedCustomer != null)
            {
                var deleteDialog = new ContentDialog
                {
                    Title = "Xác nhận xóa",
                    Content = "Bạn có chắc chắn muốn xóa nhà khách hàng này không? Lưu ý hành động này không thể hoàn tác.",
                    PrimaryButtonText = "Xóa",
                    CloseButtonText = "Hủy",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = this.XamlRoot
                };

                var result = await deleteDialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    await CustomerViewModel.Delete(selectedCustomer.Id.ToString());
                    selectedCustomer = null;
                    Debug.WriteLine("Đã xóa khách hàng cung cấp");
                }
                else
                {
                    Debug.WriteLine("Hủy xóa khách hàng cung cấp");
                }
            }
            else
            {
                // Hiển thị thông báo nếu chưa chọn mục nào
                var warningDialog = new ContentDialog
                {
                    Title = "Cảnh báo",
                    Content = "Vui lòng chọn một mục để xóa.",
                    CloseButtonText = "Đóng",
                    XamlRoot = this.XamlRoot
                };
                await warningDialog.ShowAsync();
            }
        }
    }
}
