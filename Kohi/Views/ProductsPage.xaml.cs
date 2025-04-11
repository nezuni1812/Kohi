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
        public ProductModel? SelectedProduct { get; set; }
        public ProductViewModel ProductViewModel { get; set; } = new ProductViewModel();
        public ProductVariantViewModel ProductVariantViewModel { get; set; } = new ProductVariantViewModel();
        public RecipeDetailViewModel RecipeDetailViewModel { get; set; } = new RecipeDetailViewModel();
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
            if (sender is TableView tableView && tableView.SelectedItem is ProductModel selected)
            {
                SelectedProduct = selected;
            }
            else
            {
                SelectedProduct = null;
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
            if (SelectedProduct == null)
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
                Content = $"Bạn có chắc chắn muốn xóa sản phẩm '{SelectedProduct.Name}' (ID: {SelectedProduct.Id}) không? Tất cả các biến thể sản phẩm liên quan cũng sẽ bị xóa. Hành động này không thể hoàn tác.",
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
                    // 1. Lấy tất cả ProductVariants liên quan đến SelectedProduct.Id
                    var allVariants = await ProductVariantViewModel.GetByProductId(SelectedProduct.Id);

                    // 2. Xóa tất cả RecipeDetails liên quan đến từng ProductVariant
                    foreach (var variant in allVariants)
                    {
                        // Giả định có phương thức GetByProductVariantId để lấy RecipeDetails
                        var recipeDetails = await RecipeDetailViewModel.GetByProductVariantId(variant.Id);
                        foreach (var recipeDetail in recipeDetails)
                        {
                            await RecipeDetailViewModel.Delete(recipeDetail.Id.ToString());
                            Debug.WriteLine($"Đã xóa RecipeDetail ID: {recipeDetail.Id}");
                        }
                    }
                    Debug.WriteLine("Đã xóa hết RecipeDetails");

                    // 3. Xóa từng ProductVariant
                    foreach (var variant in allVariants)
                    {
                        await ProductVariantViewModel.Delete(variant.Id.ToString());
                        Debug.WriteLine($"Đã xóa ProductVariant ID: {variant.Id}");
                    }
                    Debug.WriteLine("Đã xóa hết ProductVariants");

                    // 4. Xóa Product sau khi đã xóa hết ProductVariants
                    await ProductViewModel.Delete(SelectedProduct.Id.ToString());
                    Debug.WriteLine($"Đã xóa sản phẩm ID: {SelectedProduct.Id}");

                    // 5. Cập nhật lại danh sách sản phẩm và đặt lại SelectedProduct
                    await ProductViewModel.LoadData(ProductViewModel.CurrentPage);
                    SelectedProduct = null;
                    UpdatePageList();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Lỗi khi xóa sản phẩm: {ex.Message}\nStackTrace: {ex.StackTrace}");
                }
            }
            else
            {
                Debug.WriteLine("Hủy xóa sản phẩm");
            }
        }

        public async void showEditProduct_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedProduct == null)
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
            rootFrame.Navigate(typeof(EditProductPage), SelectedProduct);
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
