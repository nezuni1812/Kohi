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
    public sealed partial class InventoryInboundPage : Page
    {
        public SupplierViewModel SupplierViewModel { get; set; } = new SupplierViewModel();
        public IngredientViewModel IngredientViewModel { get; set; } = new IngredientViewModel();
        public InboundViewModel InboundViewModel { get; set; } = new InboundViewModel();
        public InboundModel? SelectedInbound { get; set; }
        public int SelectedInboundId = -1;
        public InventoryInboundPage()
        {
            this.InitializeComponent();
            Loaded += InboundsPage_Loaded;
            this.DataContext = this;
        }
        public async void InboundsPage_Loaded(object sender, RoutedEventArgs e)
        {
            await InboundViewModel.LoadData();
            UpdatePageList();
        }

        public void UpdatePageList()
        {
            if (InboundViewModel == null) return;
            pageList.ItemsSource = Enumerable.Range(1, InboundViewModel.TotalPages);
            pageList.SelectedItem = InboundViewModel.CurrentPage;
        }

        public async void OnPageSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (InboundViewModel == null || pageList.SelectedItem == null) return;

            var selectedPage = (int)pageList.SelectedItem;
            if (selectedPage != InboundViewModel.CurrentPage)
            {
                await InboundViewModel.LoadData(selectedPage);
                UpdatePageList();
            }
        }

        public void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is TableView tableView && tableView.SelectedItem is InboundModel selectedInbound)
            {
                SelectedInboundId = selectedInbound.Id;
                InboundBatchCodeTextBox.IsEnabled = true;
                InboundBatchCodeTextBox.Text = SelectedInboundId.ToString();
                InboundBatchCodeTextBox.IsEnabled = false;
                Debug.WriteLine($"Selected Inbound ID: {SelectedInboundId}");
            }
            else
            {
                SelectedInbound = null;
                SelectedInboundId = -1;
                Debug.WriteLine("Không có lô hàng nào được chọn!");
            }
        }

        public async void showEditInfoDialog_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedInboundId == -1)
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

            Debug.WriteLine("showEditInfoDialog_Click triggered");
            var result = await InfoDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {

            }
        }

        private void InfoDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = true;
            SelectedInboundId = -1;
        }

        private void InfoDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            
        }

        public async void showDeleteInfoDialog_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedInboundId == -1)
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

            var deleteDialog = new ContentDialog
            {
                Title = "Xác nhận xóa",
                Content = $"Bạn có chắc chắn muốn xóa lô hàng {SelectedInboundId} không? Lưu ý hành động này không thể hoàn tác.",
                PrimaryButtonText = "Xóa",
                CloseButtonText = "Hủy",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            var result = await deleteDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                Debug.WriteLine($"Đã xóa lô hàng với ID: {SelectedInboundId}");
            }
            else
            {
                Debug.WriteLine("Hủy xóa lô hàng");
            }
        }
        private void saveButton_click(object sender, RoutedEventArgs e)
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
    }
}
