using Kohi.Errors;
using Kohi.Models;
using Kohi.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WinUI.TableView;

namespace Kohi.Views
{
    public sealed partial class PartnersPage : Page
    {
        public CustomerViewModel CustomerViewModel { get; set; } = new CustomerViewModel();
        public SupplierViewModel SupplierViewModel { get; set; } = new SupplierViewModel();
        private SupplierModel selectedSupplier;
        private CustomerModel selectedCustomer;
        private readonly IErrorHandler _errorHandler;
        public bool IsLoading { get; set; } = false;

        public PartnersPage()
        {
            this.InitializeComponent();
            addButtonTextBlock.Text = "Thêm khách hàng";
            editButton.Click += EditButton_Click;
            Loaded += PartnersPage_Loaded;
            var emptyInputHandler = new EmptyInputErrorHandler();
            _errorHandler = emptyInputHandler;
        }

        private async void PartnersPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDataWithProgress(true);
        }

        private async Task LoadDataWithProgress(bool isCustomer, int page = 1)
        {
            try
            {
                IsLoading = true;
                ProgressRing.IsActive = true;
                if (isCustomer)
                {
                    await CustomerViewModel.LoadData(page);
                    MyTableView.ItemsSource = CustomerViewModel.Customers;
                }
                else
                {
                    await SupplierViewModel.LoadData(page);
                    MyTableView.ItemsSource = SupplierViewModel.Suppliers;
                }
                UpdatePageList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading data: {ex.Message}");
                var errorDialog = new ContentDialog
                {
                    Title = "Lỗi",
                    Content = $"Không thể tải dữ liệu: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
            }
            finally
            {
                IsLoading = false;
                ProgressRing.IsActive = false;
            }
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
                await LoadDataWithProgress(true, selectedPage);
            }
            else if (SelectorBar.SelectedItem == SupplierSelectorBar && selectedPage != SupplierViewModel.CurrentPage)
            {
                await LoadDataWithProgress(false, selectedPage);
            }
        }

        private async void SelectorBar_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
        {
            if (sender.SelectedItem == CustomerSelectorBar)
            {
                await LoadDataWithProgress(true);
                addButtonTextBlock.Text = "Thêm khách hàng";
                addButton.Click -= showAddSupplierDialog_Click;
                addButton.Click += showAddCustomerDialog_Click;

                deleteButtonTextBlock.Text = "Xóa khách hàng";
                deleteButton.Click -= ShowDeleteSupplierDialog;
                deleteButton.Click += ShowDeleteCustomerDialog;

                editButtonTextBlock.Text = "Chỉnh sửa khách hàng";
                selectedSupplier = null;
            }
            else if (sender.SelectedItem == SupplierSelectorBar)
            {
                await LoadDataWithProgress(false);
                addButtonTextBlock.Text = "Thêm nhà cung cấp";
                addButton.Click -= showAddCustomerDialog_Click;
                addButton.Click += showAddSupplierDialog_Click;

                deleteButtonTextBlock.Text = "Xóa nhà cung cấp";
                deleteButton.Click -= ShowDeleteCustomerDialog;
                deleteButton.Click += ShowDeleteSupplierDialog;

                editButtonTextBlock.Text = "Chỉnh sửa nhà cung cấp";
                selectedCustomer = null;
            }
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectorBar.SelectedItem == CustomerSelectorBar && selectedCustomer != null)
            {
                EditCustomerNameTextBox.Text = selectedCustomer.Name;
                EditCustomerAddressTextBox.Text = selectedCustomer.Address;
                EditCustomerPhoneNumberTextBox.Text = selectedCustomer.Phone;
                EditCustomerEmailTextBox.Text = selectedCustomer.Email;
                var result = await CustomerEditDialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    try
                    {
                        IsLoading = true;
                        ProgressRing.IsActive = true;

                        var fields = new Dictionary<string, string>
                        {
                            { "Tên khách hàng", EditCustomerNameTextBox.Text },
                            { "Số điện thoại", EditCustomerPhoneNumberTextBox.Text }
                        };

                        List<string> errors = _errorHandler?.HandleError(fields) ?? new List<string>();
                        if (errors.Any())
                        {
                            ContentDialog errorDialog = new ContentDialog
                            {
                                Title = "Lỗi nhập liệu",
                                Content = string.Join("\n", errors),
                                CloseButtonText = "OK",
                                XamlRoot = this.XamlRoot
                            };
                            await errorDialog.ShowAsync();
                            return;
                        }

                        selectedCustomer.Name = EditCustomerNameTextBox.Text;
                        selectedCustomer.Phone = EditCustomerPhoneNumberTextBox.Text;
                        selectedCustomer.Email = EditCustomerEmailTextBox.Text;
                        selectedCustomer.Address = EditCustomerAddressTextBox.Text;

                        await CustomerViewModel.Update(selectedCustomer.Id.ToString(), selectedCustomer);
                        await LoadDataWithProgress(true, CustomerViewModel.CurrentPage);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error updating customer: {ex.Message}");
                        var errorDialog = new ContentDialog
                        {
                            Title = "Lỗi",
                            Content = $"Không thể cập nhật khách hàng: {ex.Message}",
                            CloseButtonText = "OK",
                            XamlRoot = this.XamlRoot
                        };
                        await errorDialog.ShowAsync();
                    }
                    finally
                    {
                        IsLoading = false;
                        ProgressRing.IsActive = false;
                    }
                }
            }
            else if (SelectorBar.SelectedItem == SupplierSelectorBar && selectedSupplier != null)
            {
                EditSupplierNameTextBox.Text = selectedSupplier.Name;
                EditSupplierPhoneNumberTextBox.Text = selectedSupplier.Phone;
                EditSupplierEmailTextBox.Text = selectedSupplier.Email;
                EditSupplierAddressTextBox.Text = selectedSupplier.Address;

                var result = await SupplierEditDialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    try
                    {
                        IsLoading = true;
                        ProgressRing.IsActive = true;

                        var fields = new Dictionary<string, string>
                        {
                            { "Tên nhà cung cấp", EditSupplierNameTextBox.Text },
                            { "Số điện thoại", EditSupplierPhoneNumberTextBox.Text }
                        };

                        List<string> errors = _errorHandler?.HandleError(fields) ?? new List<string>();
                        if (errors.Any())
                        {
                            ContentDialog errorDialog = new ContentDialog
                            {
                                Title = "Lỗi nhập liệu",
                                Content = string.Join("\n", errors),
                                CloseButtonText = "OK",
                                XamlRoot = this.XamlRoot
                            };
                            await errorDialog.ShowAsync();
                            return;
                        }

                        selectedSupplier.Name = EditSupplierNameTextBox.Text;
                        selectedSupplier.Phone = EditSupplierPhoneNumberTextBox.Text;
                        selectedSupplier.Email = EditSupplierEmailTextBox.Text;
                        selectedSupplier.Address = EditSupplierAddressTextBox.Text;

                        await SupplierViewModel.Update(selectedSupplier.Id.ToString(), selectedSupplier);
                        await LoadDataWithProgress(false, SupplierViewModel.CurrentPage);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error updating supplier: {ex.Message}");
                        var errorDialog = new ContentDialog
                        {
                            Title = "Lỗi",
                            Content = $"Không thể cập nhật nhà cung cấp: {ex.Message}",
                            CloseButtonText = "OK",
                            XamlRoot = this.XamlRoot
                        };
                        await errorDialog.ShowAsync();
                    }
                    finally
                    {
                        IsLoading = false;
                        ProgressRing.IsActive = false;
                    }
                }
            }
            else
            {
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
                    editButton.IsEnabled = true;
                    deleteButton.IsEnabled = true;
                }
                else if (SelectorBar.SelectedItem == SupplierSelectorBar && tableView.SelectedItem is SupplierModel supplier)
                {
                    selectedSupplier = supplier;
                    Debug.WriteLine($"Selected Supplier ID: {selectedSupplier.Id}");
                    editButton.IsEnabled = true;
                    deleteButton.IsEnabled = true;
                }
            }
            else
            {
                selectedCustomer = null;
                selectedSupplier = null;
                editButton.IsEnabled = false;
                deleteButton.IsEnabled = false;
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
                try
                {
                    IsLoading = true;
                    ProgressRing.IsActive = true;

                    var fields = new Dictionary<string, string>
                    {
                        { "Tên nhà cung cấp", SupplierNameTextBox.Text },
                        { "Số điện thoại", SupplierPhoneNumberTextBox.Text }
                    };

                    List<string> errors = _errorHandler?.HandleError(fields) ?? new List<string>();
                    if (errors.Any())
                    {
                        ContentDialog errorDialog = new ContentDialog
                        {
                            Title = "Lỗi nhập liệu",
                            Content = string.Join("\n", errors),
                            CloseButtonText = "OK",
                            XamlRoot = this.XamlRoot
                        };
                        await errorDialog.ShowAsync();
                        return;
                    }

                    var newSupplier = new SupplierModel
                    {
                        Name = SupplierNameTextBox.Text,
                        Address = SupplierAddressTextBox.Text,
                        Phone = SupplierPhoneNumberTextBox.Text,
                        Email = SupplierEmailTextBox.Text,
                    };
                    await SupplierViewModel.Add(newSupplier);
                    await LoadDataWithProgress(false, SupplierViewModel.CurrentPage);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error adding supplier: {ex.Message}");
                    var errorDialog = new ContentDialog
                    {
                        Title = "Lỗi",
                        Content = $"Không thể thêm nhà cung cấp: {ex.Message}",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                }
                finally
                {
                    IsLoading = false;
                    ProgressRing.IsActive = false;
                }
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
                try
                {
                    IsLoading = true;
                    ProgressRing.IsActive = true;

                    var fields = new Dictionary<string, string>
                    {
                        { "Tên khách hàng", CustomerNameTextBox.Text },
                        { "Số điện thoại", CustomerPhoneNumberTextBox.Text }
                    };

                    List<string> errors = _errorHandler?.HandleError(fields) ?? new List<string>();
                    if (errors.Any())
                    {
                        ContentDialog errorDialog = new ContentDialog
                        {
                            Title = "Lỗi nhập liệu",
                            Content = string.Join("\n", errors),
                            CloseButtonText = "OK",
                            XamlRoot = this.XamlRoot
                        };
                        await errorDialog.ShowAsync();
                        return;
                    }

                    var newCustomer = new CustomerModel
                    {
                        Name = CustomerNameTextBox.Text,
                        Address = CustomerAddressTextBox.Text,
                        Phone = CustomerPhoneNumberTextBox.Text,
                        Email = CustomerEmailTextBox.Text,
                    };
                    await CustomerViewModel.Add(newCustomer);
                    await LoadDataWithProgress(true, CustomerViewModel.CurrentPage);

                    Debug.WriteLine("In info: " + newCustomer.Id);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error adding customer: {ex.Message}");
                    var errorDialog = new ContentDialog
                    {
                        Title = "Lỗi",
                        Content = $"Không thể thêm khách hàng: {ex.Message}",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                }
                finally
                {
                    IsLoading = false;
                    ProgressRing.IsActive = false;
                }
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
                    try
                    {
                        IsLoading = true;
                        ProgressRing.IsActive = true;

                        int res = await SupplierViewModel.Delete(selectedSupplier.Id.ToString());

                        if (res == 0)
                        {
                            var errorDialog = new ContentDialog
                            {
                                Title = "Lỗi",
                                Content = "Không thể xóa nhà cung cấp này vì có nguyên liệu liên quan.",
                                CloseButtonText = "OK",
                                XamlRoot = this.XamlRoot
                            };
                            await errorDialog.ShowAsync();
                            return;
                        }
                        else
                        {
                            await LoadDataWithProgress(false, SupplierViewModel.CurrentPage);
                            Debug.WriteLine("Đã xóa nhà cung cấp");
                        }
                        selectedSupplier = null;

                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error deleting supplier: {ex.Message}");
                        var errorDialog = new ContentDialog
                        {
                            Title = "Lỗi",
                            Content = $"Không thể xóa nhà cung cấp: {ex.Message}",
                            CloseButtonText = "OK",
                            XamlRoot = this.XamlRoot
                        };
                        await errorDialog.ShowAsync();
                    }
                    finally
                    {
                        IsLoading = false;
                        ProgressRing.IsActive = false;
                    }
                }
                else
                {
                    Debug.WriteLine("Hủy xóa nhà cung cấp");
                }
            }
            else
            {
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
                    try
                    {
                        IsLoading = true;
                        ProgressRing.IsActive = true;

                        int res = await CustomerViewModel.Delete(selectedCustomer.Id.ToString());
                        if (res == 0)
                        {
                            var errorDialog = new ContentDialog
                            {
                                Title = "Lỗi",
                                Content = "Không thể xóa khách hàng này vì có hóa đơn liên quan.",
                                CloseButtonText = "OK",
                                XamlRoot = this.XamlRoot
                            };
                            await errorDialog.ShowAsync();
                            return;
                        }
                        else
                        {
                            await LoadDataWithProgress(true, CustomerViewModel.CurrentPage);
                            Debug.WriteLine("Đã xóa khách hàng");
                        }
                        selectedCustomer = null;
                    }
                    catch (Exception ex)
                    {

                    }
                    finally
                    {
                        IsLoading = false;
                        ProgressRing.IsActive = false;
                    }
                }
                else
                {
                    Debug.WriteLine("Hủy xóa khách hàng");
                }
            }
            else
            {
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