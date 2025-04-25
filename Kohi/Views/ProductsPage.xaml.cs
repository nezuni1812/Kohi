using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Kohi.Models;
using Kohi.ViewModels;
using System.Diagnostics;
using WinUI.TableView;

namespace Kohi.Views
{
    public sealed partial class ProductsPage : Page
    {
        public ProductModel? SelectedProduct { get; set; }
        public ProductViewModel ProductViewModel { get; set; } = new ProductViewModel();
        public ProductVariantViewModel ProductVariantViewModel { get; set; } = new ProductVariantViewModel();
        public RecipeDetailViewModel RecipeDetailViewModel { get; set; } = new RecipeDetailViewModel();
        public bool IsLoading { get; set; } = false;

        public ProductsPage()
        {
            this.InitializeComponent();
            Loaded += ProductPage_Loaded;
        }

        private async void ProductPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDataWithProgress();
        }

        private async Task LoadDataWithProgress(int page = 1)
        {
            try
            {
                IsLoading = true;
                ProgressRing.IsActive = true;
                await ProductViewModel.LoadData(page);
                UpdatePageList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading products: {ex.Message}");
                var errorDialog = new ContentDialog
                {
                    Title = "Lỗi",
                    Content = $"Không thể tải dữ liệu sản phẩm: {ex.Message}",
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

        public void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is TableView tableView && tableView.SelectedItem is ProductModel selected)
            {
                SelectedProduct = selected;
                editButton.IsEnabled = true;
                deleteButton.IsEnabled = true;
            }
            else
            {
                SelectedProduct = null;
                editButton.IsEnabled = false;
                deleteButton.IsEnabled = false;
            }
        }

        public void showAddProduct_Click(object sender, RoutedEventArgs e)
        {
            Frame rootFrame = new Frame();
            this.Content = rootFrame;
            rootFrame.Navigate(typeof(AddNewProductPage), null);
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
                    IsLoading = true;
                    ProgressRing.IsActive = true;

                    var allVariants = await ProductVariantViewModel.GetByProductId(SelectedProduct.Id);
                    foreach (var variant in allVariants)
                    {
                        var recipeDetails = await RecipeDetailViewModel.GetByProductVariantId(variant.Id);
                        foreach (var recipeDetail in recipeDetails)
                        {
                            await RecipeDetailViewModel.Delete(recipeDetail.Id.ToString());
                            Debug.WriteLine($"Đã xóa RecipeDetail ID: {recipeDetail.Id}");
                        }
                    }
                    Debug.WriteLine("Đã xóa hết RecipeDetails");

                    foreach (var variant in allVariants)
                    {
                        await ProductVariantViewModel.Delete(variant.Id.ToString());
                        Debug.WriteLine($"Đã xóa ProductVariant ID: {variant.Id}");
                    }
                    Debug.WriteLine("Đã xóa hết ProductVariants");

                    await ProductViewModel.Delete(SelectedProduct.Id.ToString());
                    Debug.WriteLine($"Đã xóa sản phẩm ID: {SelectedProduct.Id}");

                    await LoadDataWithProgress(ProductViewModel.CurrentPage);
                    SelectedProduct = null;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Lỗi khi xóa sản phẩm: {ex.Message}\nStackTrace: {ex.StackTrace}");
                    var errorDialog = new ContentDialog
                    {
                        Title = "Lỗi",
                        Content = $"Không thể xóa sản phẩm: {ex.Message}",
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
                Debug.WriteLine("Hủy xóa sản phẩm");
            }
        }

        public void showEditProduct_Click(object sender, RoutedEventArgs e)
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

                noSelectionDialog.ShowAsync();
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
                await LoadDataWithProgress(selectedPage);
            }
        }
    }
}