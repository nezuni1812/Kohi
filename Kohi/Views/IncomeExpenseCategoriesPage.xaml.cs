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
using Windows.Gaming.Preview.GamesEnumeration;
using WinUI.TableView;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Kohi.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class IncomeExpenseCategoriesPage : Page
    {
        public ExpenseCategoryViewModel ExpenseCategoryViewModel { get; set; } = new ExpenseCategoryViewModel();
        public ExpenseCategoryModel? SelectedExpenseCategory { get; set; }
        public IncomeExpenseCategoriesPage()
        {
            this.InitializeComponent();
            this.DataContext = this;
            SelectedExpenseCategory = null;
            Loaded += ExpenseCategoriesPage_Loaded;
        }

        public async void ExpenseCategoriesPage_Loaded(object sender, RoutedEventArgs e)
        {
            await ExpenseCategoryViewModel.LoadData();
            UpdatePageList();
        }

        public void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is TableView tableView && tableView.SelectedItem is ExpenseCategoryModel expenseCategory)
            {
                SelectedExpenseCategory = expenseCategory;
                Debug.WriteLine($"Selected Customer ID: {SelectedExpenseCategory.Id}");
            }
        }
        public void UpdatePageList()
        {
            if (ExpenseCategoryViewModel == null) return;
            pageList.ItemsSource = Enumerable.Range(1, ExpenseCategoryViewModel.TotalPages);
            pageList.SelectedItem = ExpenseCategoryViewModel.CurrentPage;
        }

        public async void OnPageSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ExpenseCategoryViewModel == null || pageList.SelectedItem == null) return;

            var selectedPage = (int)pageList.SelectedItem;
            if (selectedPage != ExpenseCategoryViewModel.CurrentPage)
            {
                await ExpenseCategoryViewModel.LoadData(selectedPage);
                UpdatePageList();
            }
        }

        private void ExpenseCategoryDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = true;

        }

        private void ExpenseCategoryDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }

        public async void showDeleteExpenseCategoryDialog_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedExpenseCategory == null)
            {
                var noSelectionDialog = new ContentDialog
                {
                    Title = "Lỗi",
                    Content = "Không có danh mục nào được chọn",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await noSelectionDialog.ShowAsync();
                return;
            }
            var deleteDialog = new ContentDialog
            {
                Title = "Xác nhận xóa",
                Content = "Bạn có chắc chắn muốn xóa danh mục này không? Lưu ý hành động này không thể hoàn tác.",
                PrimaryButtonText = "Xóa",
                CloseButtonText = "Hủy",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            var result = await deleteDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                await ExpenseCategoryViewModel.Delete(SelectedExpenseCategory.Id.ToString());
                Debug.WriteLine("Đã xóa danh mục");
            }
            else
            {
                Debug.WriteLine("Hủy xóa danh mục");
            }
        }

        public async void showEditExpenseCategoryDialog_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedExpenseCategory == null)
            {
                var noSelectionDialog = new ContentDialog
                {
                    Title = "Lỗi",
                    Content = "Không có danh mục nào được chọn",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };

                await noSelectionDialog.ShowAsync();
                return;
            }

            Debug.WriteLine("showEditExpenseCategoryDialog_Click triggered");
            EditExpenseCategoryName.Text = SelectedExpenseCategory.CategoryName;
            EditExpenseCategoryNote.Text = SelectedExpenseCategory.Description;
            var result = await EditExpenseCategoryDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                SelectedExpenseCategory.CategoryName = EditExpenseCategoryName.Text;
                SelectedExpenseCategory.Description = EditExpenseCategoryNote.Text;
                await ExpenseCategoryViewModel.Update(SelectedExpenseCategory.Id.ToString(), SelectedExpenseCategory);
            }
        }

        private void AddExpenseCategoryDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Simply close the dialog
        }
        private void EditExpenseCategoryDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Simply close the dialog without saving changes
        }
        public async void showAddExpenseCategoryDialog_Click(object sender, RoutedEventArgs e)
        {
            // Clear any existing values in the dialog fields
            expenseCategoryName.Text = string.Empty;
            expenseCategoryNote.Text = string.Empty;

            // Show the dialog
            var result = await AddExpenseCategoryDialog.ShowAsync();

            // Process the result if Primary button was clicked (Confirm)
            if (result == ContentDialogResult.Primary)
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(expenseCategoryName.Text))
                {
                    var validationDialog = new ContentDialog
                    {
                        Title = "Lỗi",
                        Content = "Tên danh mục không được để trống",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await validationDialog.ShowAsync();
                    return;
                }

                // Create new expense category
                var newCategory = new ExpenseCategoryModel
                {
                    CategoryName = expenseCategoryName.Text,
                    Description = expenseCategoryNote.Text
                };

                // Add to database
                await ExpenseCategoryViewModel.Add(newCategory);

                // Refresh data
                await ExpenseCategoryViewModel.LoadData();
                UpdatePageList();
            }
        }
    }
}
