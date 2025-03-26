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
    public sealed partial class InventoriesPage : Page
    {
        public IngredientViewModel IngredientViewModel { get; set; } = new IngredientViewModel();
        public SupplierViewModel SupplierViewModel { get; set; } = new SupplierViewModel();
        public InboundViewModel InboundViewModel { get; set; } = new InboundViewModel();
        public InventoryViewModel InventoryViewModel { get; set; } = new InventoryViewModel();
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

            }
        }

        public async void showAddInventoryDialog_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("showAddInventoryDialog_Click triggered");
            var result = await InboundDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {

            }
            else
            {

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
