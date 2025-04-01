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
    public sealed partial class InventoryCheckPage : Page
    {
        public InventoryCheckViewModel InventoryCheckViewModel { get; set; } = new InventoryCheckViewModel();
        public CheckInventoryModel? SelectedCheckInventory { get; set; }
        public int SelectedCheckInventoryId = -1;
        public InventoryCheckPage()
        {
            this.InitializeComponent();
            Loaded += InventoryChecksPage_Loaded;

            //GridContent.DataContext = IncomeViewModel;
        }
        public async void InventoryChecksPage_Loaded(object sender, RoutedEventArgs e)
        {
            await InventoryCheckViewModel.LoadData(); // Tải trang đầu tiên
            UpdatePageList();
        }

        public void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(sender is WinUI.TableView.TableView tableView && tableView.SelectedItem is CheckInventoryModel selectedCheckInventory)
            {
                //SelectedCheckInventory = selectedCheckInventory;
                SelectedCheckInventoryId = selectedCheckInventory.Id;
                CheckBatchCodeTextBox.IsEnabled = true;
                CheckBatchCodeTextBox.Text = SelectedCheckInventoryId.ToString();
                CheckBatchCodeTextBox.IsEnabled = false;
                Debug.WriteLine($"Selected Check Inventory ID: {SelectedCheckInventoryId}");
            }
            else
            {
                SelectedCheckInventory = null;
                SelectedCheckInventoryId = -1;
                CheckBatchCodeTextBox.Text = string.Empty;
                Debug.WriteLine("Không có kiểm kê nào được chọn!");
            }
        }

        public void UpdatePageList()
        {
            if (InventoryCheckViewModel == null) return;
            pageList.ItemsSource = Enumerable.Range(1, InventoryCheckViewModel.TotalPages);
            pageList.SelectedItem = InventoryCheckViewModel.CurrentPage;
        }

        public async void OnPageSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (InventoryCheckViewModel == null || pageList.SelectedItem == null) return;

            var selectedPage = (int)pageList.SelectedItem;
            if (selectedPage != InventoryCheckViewModel.CurrentPage)
            {
                await InventoryCheckViewModel.LoadData(selectedPage);
                UpdatePageList();
            }
        }
        public async void showEditInfoDialog_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedCheckInventoryId == -1)
            {
                var noSelectionDialog = new ContentDialog
                {
                    Title = "Lỗi",
                    Content = "Không có kiểm kê nào được chọn",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };

                await noSelectionDialog.ShowAsync();
                return;
            }

            if (SelectedCheckInventory != null)
            {
                CheckBatchCodeTextBox.Text = SelectedCheckInventory.InventoryId.ToString();
                InventoryQuantityBox.Value = SelectedCheckInventory.ActualQuantity;
                InventoryDatePicker.Date = SelectedCheckInventory.CheckDate;
                ReasonTextBox.Text = SelectedCheckInventory.Notes ?? string.Empty;
            }

            Debug.WriteLine("showEditInfoDialog_Click triggered");
            var result = await CheckDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                
            }
        }

        private void CheckDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = true;
            SelectedCheckInventoryId = -1;
        }

        private void CheckDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }

        public async void showDeleteInfoDialog_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedCheckInventoryId == -1)
            {
                var noSelectionDialog = new ContentDialog
                {
                    Title = "Lỗi",
                    Content = "Không có kiểm kê nào được chọn",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };

                await noSelectionDialog.ShowAsync();
                return;
            }

            var deleteDialog = new ContentDialog
            {
                Title = "Xác nhận xóa",
                Content = $"Bạn có chắc chắn muốn xóa kiểm kê có ID là {SelectedCheckInventoryId} không? Lưu ý hành động này không thể hoàn tác.",
                PrimaryButtonText = "Xóa",
                CloseButtonText = "Hủy",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            var result = await deleteDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                Debug.WriteLine($"Đã xóa kiểm kê ID: {SelectedCheckInventoryId}");
            }
            else
            {
                Debug.WriteLine("Hủy xóa kiểm kê");
            }
        }
    }
}
