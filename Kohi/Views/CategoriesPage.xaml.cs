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
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Kohi.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CategoriesPage : Page
    {
        public CategoryModel? SelectedCategory { get; set; }

        public int selectedCategoryId = -1;
        public CategoryViewModel CategoryViewModel { get; set; } = new CategoryViewModel();
        public CategoriesPage()
        {
            this.InitializeComponent();
            Loaded += CategoriesPage_Loaded;
        }
        public async void CategoriesPage_Loaded(object sender, RoutedEventArgs e)
        {
            await CategoryViewModel.LoadData(); // Tải trang đầu tiên
            UpdatePageList();
        }

        public void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is TableView tableView && tableView.SelectedItem is CategoryModel selectedId)
            {
                selectedCategoryId = selectedId.Id;
                Debug.WriteLine($"Selected Category ID: {selectedCategoryId}");
            }
        }

        public void showAddCategory_Click(object sender, RoutedEventArgs e)
        {
            Frame rootFrame = new Frame();
            this.Content = rootFrame;

            rootFrame.Navigate(typeof(AddNewCategoryPage), null);
            // Logic thêm khách hàng
        }
        private async Task ShowErrorDialog(string title, string message)
        {
            var errorDialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };

            await errorDialog.ShowAsync();
        }

        public async void showDeleteCategoryDialog_Click(object sender, RoutedEventArgs e)
        {
            if (selectedCategoryId == -1)
            {
                await ShowErrorDialog("Lỗi", "Không có danh mục nào được chọn");
                return;
            }

            var category = CategoryViewModel.Categories.FirstOrDefault(c => c.Id == selectedCategoryId);

            if (category == null)
            {
                await ShowErrorDialog("Lỗi", "Không tìm thấy danh mục");
                return;
            }

            if (category.Products != null && category.Products.Any())
            {
                await ShowErrorDialog("Lỗi", "Không thể xóa danh mục vì có sản phẩm trong danh mục này");
                return;
            }

            var deleteDialog = new ContentDialog
            {
                Title = "Xác nhận xóa",
                Content = $"Bạn có chắc chắn muốn xóa danh mục có ID là {selectedCategoryId} không? Lưu ý hành động này không thể hoàn tác.",
                PrimaryButtonText = "Xóa",
                CloseButtonText = "Hủy",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            var result = await deleteDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                await CategoryViewModel.Delete(selectedCategoryId.ToString());
                Debug.WriteLine($"Đã xóa danh mục ID: {selectedCategoryId}");
            }
            else
            {
                Debug.WriteLine("Hủy xóa danh mục");
            }
        }

        public async void showEditCategory_Click(object sender, RoutedEventArgs e)
        {
            if (selectedCategoryId == -1)
            {
                await ShowErrorDialog("Lỗi", "Không có danh mục nào được chọn");
                return;
            }

            Frame rootFrame = new Frame();
            this.Content = rootFrame;

            rootFrame.Navigate(typeof(EditCategoryPage), selectedCategoryId);
        }

        public void UpdatePageList()
        {
            if (CategoryViewModel == null) return;
            pageList.ItemsSource = Enumerable.Range(1, CategoryViewModel.TotalPages);
            pageList.SelectedItem = CategoryViewModel.CurrentPage;
        }

        public async void OnPageSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoryViewModel == null || pageList.SelectedItem == null) return;

            var selectedPage = (int)pageList.SelectedItem;
            if (selectedPage != CategoryViewModel.CurrentPage)
            {
                await CategoryViewModel.LoadData(selectedPage);
                UpdatePageList();
            }
        }
    }
}
