﻿using System;
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
    public sealed partial class IngredientsPage : Page
    {
        public IngredientViewModel IngredientViewModel { get; set; } = new IngredientViewModel();

        public IngredientModel? SelectedIngredient { get; set; }

        public int SelectedIngredientId = -1;
        public IngredientsPage()
        {
            this.InitializeComponent();
            Loaded += IngredientsPage_Loaded;
        }

        public async void IngredientsPage_Loaded(object sender, RoutedEventArgs e)
        {
            await IngredientViewModel.LoadData();
            UpdatePageList();
        }
        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is TableView tableView && tableView.SelectedItem is IngredientModel selectedIngredient)
            {
                SelectedIngredientId = selectedIngredient.Id;
                Debug.WriteLine($"Selected Ingredient ID: {SelectedIngredientId}");
            }
            else
            {
                SelectedIngredient = null;
                SelectedIngredientId = -1;
                Debug.WriteLine("Không có nguyên vật liệu nào được chọn!");
            }
        }

        public void UpdatePageList()
        {
            if (IngredientViewModel == null) return;
            pageList.ItemsSource = Enumerable.Range(1, IngredientViewModel.TotalPages);
            pageList.SelectedItem = IngredientViewModel.CurrentPage;
        }

        public async void OnPageSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IngredientViewModel == null || pageList.SelectedItem == null) return;

            var selectedPage = (int)pageList.SelectedItem;
            if (selectedPage != IngredientViewModel.CurrentPage)
            {
                await IngredientViewModel.LoadData(selectedPage);
                UpdatePageList();
            }
        }

        public async void showAddIngredientDialog_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("showAddIngredientDialog_Click triggered");
            var result = await IngredientDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {

            }
            else
            {

            }
        }

        private void AddIngredientDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = true;
            SelectedIngredientId = -1;
        }

        private void AddIngredientDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }

        public async void showEditIngredientDialog_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedIngredientId == -1)
            {
                var noSelectionDialog = new ContentDialog
                {
                    Title = "Lỗi",
                    Content = "Không có nguyên vật liệu nào được chọn",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };

                await noSelectionDialog.ShowAsync();
                return;
            }

            //if (SelectedCheckInventory != null)
            //{
            //    CheckBatchCodeTextBox.Text = SelectedCheckInventory.InventoryId.ToString();
            //    InventoryQuantityBox.Value = SelectedCheckInventory.ActualQuantity;
            //    InventoryDatePicker.Date = SelectedCheckInventory.CheckDate;
            //    ReasonTextBox.Text = SelectedCheckInventory.Notes ?? string.Empty;
            //}

            Debug.WriteLine("showEditInfoDialog_Click triggered");
            var result = await EditIngredientDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {

            }
        }
        private void EditIngredientDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = true;
            SelectedIngredientId = -1;
        }

        private void EditIngredientDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }


        public async void showDeleteIngredientDialog_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedIngredientId == -1)
            {
                var noSelectionDialog = new ContentDialog
                {
                    Title = "Lỗi",
                    Content = "Không có nguyên vật liệu được chọn",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };

                await noSelectionDialog.ShowAsync();
                return;
            }

            var deleteDialog = new ContentDialog
            {
                Title = "Xác nhận xóa",
                Content = $"Bạn có chắc chắn muốn xóa nguyên vật liệu có ID là {SelectedIngredientId} không? Lưu ý hành động này không thể hoàn tác.",
                PrimaryButtonText = "Xóa",
                CloseButtonText = "Hủy",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            var result = await deleteDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                Debug.WriteLine($"Đã xóa nguyên vật liệu ID: {SelectedIngredientId}");
            }
            else
            {
                Debug.WriteLine("Hủy xóa nguyên vật liệu");
            }
        }
    }
}
