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
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.EntityFrameworkCore.Metadata;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Kohi.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class InventoriesPage : Page
    {
        public IngredientViewModel IngredientViewModel { get; set; } = new IngredientViewModel();
        public SupplierViewModel SupplierViewModel { get; set; } = new SupplierViewModel();
        public InboundViewModel InboundViewModel { get; set; } = new InboundViewModel();
        public InventoryViewModel InventoryViewModel { get; set; } = new InventoryViewModel();
        public OutboundViewModel OutboundViewModel { get; set; } = new OutboundViewModel();
        public InventoryCheckViewModel CheckInventoryViewModel { get; set; } = new InventoryCheckViewModel();

        public InventoryModel? SelectedInventory { get; set; }
        
        public int SelectedInventoryId = -1;
        public InventoriesPage()
        {
            this.InitializeComponent();
            Loaded += InventoriesPage_Loaded;
            this.DataContext = this;
            //GridContent.DataContext = IncomeViewModel;
        }
        public async void InventoriesPage_Loaded(object sender, RoutedEventArgs e)
        {
            await InventoryViewModel.LoadData(); // Tải trang đầu tiên
            UpdatePageList();
        }

        public void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is TableView tableView && tableView.SelectedItem is InventoryModel selectedInventory)
            {
                SelectedInventoryId = selectedInventory.Id;
                CheckBatchCodeTextBox.IsEnabled = true;
                CheckBatchCodeTextBox.Text = SelectedInventoryId.ToString();
                CheckBatchCodeTextBox.IsEnabled = false;

                OutboundBatchCodeTextBox.IsEnabled = true;
                OutboundBatchCodeTextBox.Text = SelectedInventoryId.ToString();
                OutboundBatchCodeTextBox.IsEnabled = false;
                Debug.WriteLine($"Selected Inbound ID: {SelectedInventoryId}");
            }
            else
            {
                SelectedInventory = null;
                SelectedInventoryId = -1;
                Debug.WriteLine("Không có lô hàng nào được chọn!");
            }
        }

        public void addButton_click(object sender, RoutedEventArgs e)
        {
            //Frame rootFrame = new Frame();
            //this.Content = rootFrame;

            //rootFrame.Navigate(typeof(AddNewInventoryPage), null);
        }

        public void UpdatePageList()
        {
            if (InventoryViewModel == null) return;
            pageList.ItemsSource = Enumerable.Range(1, InventoryViewModel.TotalPages);
            pageList.SelectedItem = InventoryViewModel.CurrentPage;
        }

        public async void OnPageSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (InventoryViewModel == null || pageList.SelectedItem == null) return;

            var selectedPage = (int)pageList.SelectedItem;
            if (selectedPage != InventoryViewModel.CurrentPage)
            {
                await InventoryViewModel.LoadData(selectedPage);
                UpdatePageList();
            }
        }


        public async void showCheckInventoryDialog_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedInventoryId == -1)
            {
                var noSelectionDialog = new ContentDialog
                {
                    Title = "Lỗi",
                    Content = "Không có lô hàng nào được chọn",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };

                await noSelectionDialog.ShowAsync();
                return;
            }

            Debug.WriteLine("showCheckInventoryDialog_Click triggered");
            var result = await CheckDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                CheckInventoryModel checkInventory = new CheckInventoryModel
                {
                    InventoryId = Convert.ToInt32(CheckBatchCodeTextBox.Text),
                    ActualQuantity = Convert.ToInt32(InventoryQuantityBox.Text),
                    CheckDate = InventoryDatePicker.Date?.DateTime ?? DateTime.Now,
                    Notes = ReasonTextBox.Text, 
                };

                await CheckInventoryViewModel.Add(checkInventory);
            }
        }

        public async void showOutboundInventoryDialog_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedInventoryId == -1)
            {
                var noSelectionDialog = new ContentDialog
                {
                    Title = "Lỗi",
                    Content = "Không có lô hàng nào được chọn",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };

                await noSelectionDialog.ShowAsync();
                return;
            }

            Debug.WriteLine("showOutboundInventoryDialog_Click triggered");
            var result = await OutboundDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                // OutboundBatchCodeTextBox
                OutboundModel outbound = new OutboundModel
                {
                    InventoryId = Convert.ToInt32(OutboundBatchCodeTextBox.Text),
                    Quantity = Convert.ToInt32(OutboundQuantityBox.Text),
                    OutboundDate = OutboundDatePicker.Date?.DateTime ?? DateTime.Now,
                    Purpose = OutboundReasonTextBox.Text,
                    Notes = OutboundNotesTextBox.Text,
                };
                await OutboundViewModel.Add(outbound); // Sửa từ Add() thành Add(inbound)

            }
        }

        public async void showAddInventoryDialog_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("showAddInventoryDialog_Click triggered");
            var result = await InboundDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var selectedIngredient = IngredientComboBox.SelectedItem as IngredientModel;
                var selectedSupplier = InboudSupplierComboBox.SelectedItem as SupplierModel;

                if (selectedIngredient == null || selectedSupplier == null)
                {
                    // Thông báo lỗi nếu không chọn Ingredient hoặc Supplier
                    // Có thể hiển thị thông báo qua UI (TextBlock hoặc ContentDialog khác)
                    Debug.WriteLine("Vui lòng chọn nguyên vật liệu và nhà cung cấp.");
                    return;
                }

                InboundModel inbound = new InboundModel
                {
                    IngredientId = selectedIngredient.Id, // Lấy ID từ Ingredient đã chọn
                    SupplierId = selectedSupplier.Id,     // Lấy ID từ Supplier đã chọn
                    Quantity = Convert.ToInt32(InboundQuantityNumberBox.Value), // Sử dụng Value thay vì Text
                    TotalCost = Convert.ToInt32(InboundTotalValueNumberBox.Value), // Sử dụng Value thay vì Text
                    InboundDate = InboundDateCalendarDatePicker.Date?.DateTime ?? DateTime.Now, // Default nếu không chọn
                    ExpiryDate = InboundExpiryDateCalendarDatePicker.Date?.DateTime ?? DateTime.MaxValue // Default nếu không chọn
                };

                // Gọi phương thức Add từ InboundViewModel (sửa lại để truyền inbound)
                await InboundViewModel.Add(inbound); // Sửa từ Add() thành Add(inbound)

                InventoryModel inventory = new InventoryModel
                {
                    InboundId = inbound.Id,
                    Quantity = inbound.Quantity,
                    InboundDate = inbound.InboundDate,
                    ExpiryDate = inbound.ExpiryDate,
                };
                await InventoryViewModel.Add(inventory);
                Debug.WriteLine("Thêm thành công InventoryModel và Inbound");
            }
            else
            {
                Debug.WriteLine("Thêm ko thành công InventoryModel và Inbound");

            }
        }

        private void CheckDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = true;
            SelectedInventoryId = -1;
        }

        private void CheckDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }

        private void OutboundDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = true;
            SelectedInventoryId = -1;
        }

        private void OutboundDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }

        private void InboundDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = true;
            SelectedInventoryId = -1;
        }

        private void InboundDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }
        private void IngredientComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IngredientComboBox.SelectedItem is IngredientModel selectedIngredient)
            {
                UnitTextBlock.IsEnabled = true;
                UnitTextBlock.Text = selectedIngredient.Unit ?? "Chưa có đơn vị";
                UnitTextBlock.IsEnabled = false;
            }
        }

        private void saveButton_click(object sender, RoutedEventArgs e)
        {

        }

    }
}
