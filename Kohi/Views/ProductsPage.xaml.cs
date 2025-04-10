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
using Microsoft.UI.Xaml.Media.Animation;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Kohi.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProductsPage : Page
    {
        //public ProductModel? selectedProduct { get; set; }

        public int selectedProductId = -1;
        public ProductViewModel ProductViewModel { get; set; } = new ProductViewModel();
        public ProductVariantViewModel ProductVariantViewModel { get; set; } = new ProductVariantViewModel();
        public ProductsPage()
        {
            this.InitializeComponent();
            Loaded += ProductPage_Loaded;
            //GridContent.DataContext = IncomeViewModel;
        }
        public async void ProductPage_Loaded(object sender, RoutedEventArgs e)
        {
            await ProductViewModel.LoadData(); // Tải trang đầu tiên
            UpdatePageList();
        }

        public void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is TableView tableView && tableView.SelectedItem is ProductModel selectedProduct)
            {
                selectedProductId = selectedProduct.Id;
                Debug.WriteLine($"Selected Product ID: {selectedProductId}");
            }
            else
            {
                selectedProduct = null;
                selectedProductId = -1;
                Debug.WriteLine("Không có sản phẩm nào được chọn!");
            }
        }

        public void showAddProduct_Click(object sender, RoutedEventArgs e)
        {
            Frame rootFrame = new Frame();
            this.Content = rootFrame;

            rootFrame.Navigate(typeof(AddNewProductPage), null);
            // Logic thêm khách hàng
        }

        public async void showDeleteProductDialog_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProductId == -1)
            {
                var noSelectionDialog = new ContentDialog
                {
                    Title = "Lỗi",
                    Content = "Không có sản phẩm nào được chọn",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };

                await noSelectionDialog.ShowAsync();
                return;
            }

            var deleteDialog = new ContentDialog
            {
                Title = "Xác nhận xóa",
                Content = $"Bạn có chắc chắn muốn xóa sản phẩm có ID là {selectedProductId} không? Tất cả các biến thể sản phẩm liên quan cũng sẽ bị xóa. Hành động này không thể hoàn tác.",
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
                    // 1. Lấy danh sách tất cả ProductVariants liên quan đến selectedProductId
                    var variants = await Task.Run(() => ProductVariantViewModel.Variants
                        .Where(v => v.ProductId == selectedProductId)
                        .ToList());

                    // 2. Xóa từng ProductVariant
                    foreach (var variant in variants)
                    {
                        await ProductVariantViewModel.Delete(variant.Id.ToString());
                        Debug.WriteLine($"Đã xóa ProductVariant ID: {variant.Id}");
                    }

                    // 3. Xóa Product sau khi đã xóa hết ProductVariants
                    await ProductViewModel.Delete(selectedProductId.ToString());
                    Debug.WriteLine($"Đã xóa sản phẩm ID: {selectedProductId}");

                    // 4. Cập nhật lại danh sách sản phẩm
                    await ProductViewModel.LoadData(ProductViewModel.CurrentPage);
                    UpdatePageList();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Lỗi khi xóa sản phẩm: {ex.Message}");
                    var errorDialog = new ContentDialog
                    {
                        Title = "Lỗi",
                        Content = $"Không thể xóa sản phẩm: {ex.Message}",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                }
            }
            else
            {
                Debug.WriteLine("Hủy xóa sản phẩm");
            }
        }

        public async void showEditProduct_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProductId == -1)
            {
                var noSelectionDialog = new ContentDialog
                {
                    Title = "Lỗi",
                    Content = "Không có sản phẩm nào được chọn",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };

                await noSelectionDialog.ShowAsync();
                return;
            }

            Frame rootFrame = new Frame();
            this.Content = rootFrame;

            rootFrame.Navigate(typeof(EditProductPage), selectedProductId);
        }

        public void UpdatePageList()
        {
            if (ProductViewModel == null) return;
            pageList.ItemsSource = Enumerable.Range(1, ProductViewModel.TotalPages);
            pageList.SelectedItem = ProductViewModel.CurrentPage;
        }

        public async void OnPageSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProductViewModel == null || pageList.SelectedItem == null) return;

            var selectedPage = (int)pageList.SelectedItem;
            if (selectedPage != ProductViewModel.CurrentPage)
            {
                await ProductViewModel.LoadData(selectedPage);
                UpdatePageList();
            }
        }
    }
}
