﻿using Kohi.Models;
using Kohi.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using System.Diagnostics;
using Kohi.Services;
using Kohi.Errors;
using Kohi.Utils;

namespace Kohi.Views
{
    public sealed partial class AddNewProductPage : Page
    {
        public CategoryViewModel CategoryViewModel { get; set; } = new CategoryViewModel();
        public IngredientViewModel IngredientViewModel { get; set; } = new IngredientViewModel();
        public ProductViewModel ProductViewModel { get; set; } = new ProductViewModel();
        public RecipeDetailViewModel RecipeDetailViewModel { get; set; } = new RecipeDetailViewModel();
        public ProductVariantViewModel ProductVariantViewModel { get; set; } = new ProductVariantViewModel();
        public AddNewProductViewModel ViewModel { get; set; } = new AddNewProductViewModel();

        private StorageFile selectedImageFile;
        private readonly IErrorHandler _errorHandler;

        public AddNewProductPage()
        {
            this.InitializeComponent();
            this.DataContext = ViewModel;
            if (!ViewModel.Variants.Any())
            {
                ViewModel.Variants.Add(new AddNewProductViewModel.ProductVariantViewModel());
            }

            var numericFields = new List<string> { "Price", "Cost", "Quantity" };
            var emptyInputHandler = new EmptyInputErrorHandler();
            var positiveNumberValidationHandler = new PositiveNumberValidationErrorHandler(numericFields);
            emptyInputHandler.SetNext(positiveNumberValidationHandler);
            _errorHandler = emptyInputHandler;
        }

        private async void AddImageButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            selectedImageFile = await picker.PickSingleFileAsync();
            if (selectedImageFile != null)
            {
                // Hiển thị ảnh đã chọn lên UI
                using (IRandomAccessStream stream = await selectedImageFile.OpenAsync(FileAccessMode.Read))
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    await bitmapImage.SetSourceAsync(stream);
                    mypic.Source = bitmapImage;
                }
                outtext.Text = "Ảnh đã chọn: " + selectedImageFile.Name;
            }
        }

        private async Task<string> SaveImage()
        {
            if (selectedImageFile != null)
            {
                try
                {
                    StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                    string productName = ProductNameTextBox.Text;
                    if (string.IsNullOrEmpty(productName))
                    {
                        outtext.Text = "Vui lòng nhập tên sản phẩm.";
                        return "";
                    }
                    string normalizedName = Utils.StringUtils.NormalizeString(productName);
                    string fileExtension = Path.GetExtension(selectedImageFile.Name);
                    string flag = DateTime.Now.ToString("yyyyMMddHHmmss");
                    string imageFileName = normalizedName + flag + fileExtension;

                    Debug.WriteLine($"Đang lưu tệp: {imageFileName} vào {localFolder.Path}");
                    await selectedImageFile.CopyAsync(localFolder, imageFileName, NameCollisionOption.ReplaceExisting);
                    outtext.Text = $"Đã lưu hình ảnh '{imageFileName}' thành công.";
                    return imageFileName; // Trả về tên tệp gồm normalizedName + flag + fileExtension
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Lỗi lưu ảnh: {ex.Message}");
                    outtext.Text = $"Lỗi khi lưu hình ảnh: {ex.Message}";
                    return "";
                }
            }
            else
            {
                outtext.Text = "Chưa chọn hình ảnh.";
                return "";
            }
        }

        private async void saveButton_click(object sender, RoutedEventArgs e)
        {
            List<string> errors = new List<string>();

            // Bước 1: Thu thập và kiểm tra dữ liệu Product
            string imgName = await SaveImage();
            var selectedCategory = CategoryProductComboBox.SelectedItem as CategoryModel;
            string categoryName = selectedCategory?.Name ?? "";

            var productFields = new Dictionary<string, string>
    {
        { "Tên danh mục", categoryName },
        { "Hình sản phẩm", imgName },
        { "Tên sản phẩm", ProductNameTextBox.Text }
    };

            errors.AddRange(_errorHandler?.HandleError(productFields) ?? new List<string>());

            // Bước 2: Thu thập và kiểm tra dữ liệu ProductVariants và RecipeDetails
            var productVariants = new List<ProductVariantModel>();
            foreach (var variantVM in ViewModel.Variants)
            {
                var variantFields = new Dictionary<string, string>
        {
            { "Size", variantVM.Size ?? "" },
            { "Price", variantVM.Price.ToString(System.Globalization.CultureInfo.InvariantCulture) },
            { "Cost", variantVM.Cost.ToString(System.Globalization.CultureInfo.InvariantCulture) }
        };

                errors.AddRange(_errorHandler?.HandleError(variantFields)?.Select(err => $"Biến thể: {err}") ?? new List<string>());

                var variant = new ProductVariantModel
                {
                    Size = variantVM.Size,
                    Price = variantVM.Price,
                    Cost = variantVM.Cost,
                    RecipeDetails = new List<RecipeDetailModel>()
                };

                // Kiểm tra RecipeDetails
                foreach (var recipeVM in variantVM.RecipeDetails)
                {
                    var recipeFields = new Dictionary<string, string>
            {
                { "Nguyên liệu", recipeVM.Ingredient?.Name ?? "" },
                { "Số lượng", recipeVM.Quantity.ToString(System.Globalization.CultureInfo.InvariantCulture) }
            };

                    errors.AddRange(_errorHandler?.HandleError(recipeFields)?.Select(err => $"Biến thể {variant.Size}, Công thức: {err}") ?? new List<string>());

                    var recipe = new RecipeDetailModel
                    {
                        IngredientId = recipeVM.Ingredient?.Id ?? 0,
                        Quantity = recipeVM.Quantity,
                        Unit = recipeVM.Unit ?? recipeVM.Ingredient?.Unit ?? ""
                    };

                    variant.RecipeDetails.Add(recipe);
                }

                productVariants.Add(variant);
            }

            // Bước 3: Hiển thị lỗi nếu có
            if (errors.Any())
            {
                outtext.Text = $"Lỗi nhập liệu:\n{string.Join("\n", errors)}";
                return;
            }

            // Bước 4: Lưu dữ liệu nếu không có lỗi
            try
            {
                // Tạo Product
                var product = new ProductModel
                {
                    Name = ProductNameTextBox.Text,
                    ImageUrl = imgName,
                    CategoryId = selectedCategory?.Id ?? 0,
                    IsActive = IsActiveCheckBox.IsChecked == true,
                    IsTopping = IsToppingCheckBox.IsChecked == true,
                    Description = DescriptionTextBox.Text,
                    Category = selectedCategory,
                    ProductVariants = new List<ProductVariantModel>()
                };

                // Lưu Product
                Debug.WriteLine("Bắt đầu thêm Product...");
                await ProductViewModel.Add(product);
                Debug.WriteLine($"Đã thêm Product, Id = {product.Id}");

                // Lưu ProductVariants và RecipeDetails
                foreach (var variant in productVariants)
                {
                    variant.ProductId = product.Id;
                    Debug.WriteLine($"Thêm mới ProductVariant: Size = {variant.Size}");
                    await ProductVariantViewModel.Add(variant);
                    Debug.WriteLine($"Đã thêm ProductVariant, Id = {variant.Id}");
                    product.ProductVariants.Add(variant);

                    foreach (var recipe in variant.RecipeDetails)
                    {
                        recipe.ProductVariantId = variant.Id;
                        Debug.WriteLine($"Trước khi thêm RecipeDetail: ProductVariantId = {recipe.ProductVariantId}, IngredientId = {recipe.IngredientId}, Quantity = {recipe.Quantity}, Unit = {recipe.Unit}");
                        int recipeId = await RecipeDetailViewModel.Add(recipe);
                        recipe.Id = recipeId;
                        Debug.WriteLine($"Đã thêm RecipeDetail, Id = {recipe.Id}, ProductVariantId = {recipe.ProductVariantId}");
                    }
                }

                // Log kết quả
                Debug.WriteLine("Dữ liệu sau khi thêm:");
                Debug.WriteLine($"Product: Id = {product.Id}, Name = {product.Name}, Variants = {product.ProductVariants.Count}");
                foreach (var variant in product.ProductVariants)
                {
                    Debug.WriteLine($"  Variant: Id = {variant.Id}, Size = {variant.Size}");
                    foreach (var recipe in variant.RecipeDetails)
                    {
                        Debug.WriteLine($"    Recipe: Id = {recipe.Id}, ProductVariantId = {recipe.ProductVariantId}, IngredientId = {recipe.IngredientId}, Quantity = {recipe.Quantity}, Unit = {recipe.Unit}");
                    }
                }

                // Cập nhật UI
                outtext.Text = "✅ Đã thêm sản phẩm thành công!";
                ProductNameTextBox.Text = string.Empty;
                CategoryProductComboBox.SelectedItem = null;
                IsActiveCheckBox.IsChecked = false;
                IsToppingCheckBox.IsChecked = false;
                DescriptionTextBox.Text = string.Empty;
                ViewModel.Variants.Clear();
                ViewModel.Variants.Add(new AddNewProductViewModel.ProductVariantViewModel());
                mypic.Source = null;
                selectedImageFile = null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi thêm sản phẩm: {ex.Message}");
                outtext.Text = $"Lỗi khi thêm sản phẩm: {ex.Message}";
            }
        }

        public bool PrintErrors(List<string> errors)
        {
            if (errors.Any())
            {
                outtext.Text = string.Join("\n", errors);
                return true;
            }
            else
            {
                outtext.Text = "✅ Dữ liệu hợp lệ!";
                return false;
            }
        }

        private bool HasErrors(List<string> errors)
        {
            return PrintErrors(errors);
        }

        private void AddVariantButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsToppingCheckBox.IsChecked != true)
            {
                ViewModel.Variants.Add(new AddNewProductViewModel.ProductVariantViewModel());
            }
        }

        private void RemoveVariantButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var variant = (AddNewProductViewModel.ProductVariantViewModel)button.Tag;

            int index = ViewModel.Variants.IndexOf(variant);
            if (ViewModel.Variants.Count > 1 && index >= 0)
            {
                ViewModel.Variants.RemoveAt(index);
            }
        }

        private async void AddRecipeDetailButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                var currentVariant = button.DataContext as AddNewProductViewModel.ProductVariantViewModel;
                if (currentVariant != null)
                {
                    currentVariant.RecipeDetails.Add(new AddNewProductViewModel.RecipeDetailViewModel());
                }
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox != null)
            {
                var recipeDetail = comboBox.DataContext as AddNewProductViewModel.RecipeDetailViewModel;
                if (recipeDetail != null && recipeDetail.Ingredient != null)
                {
                    var grid = FindParent<Grid>(comboBox);
                    if (grid != null)
                    {
                        var ingredientUnit = grid.FindName("IngredientUnit") as TextBox;
                        if (ingredientUnit != null)
                        {
                            ingredientUnit.Text = recipeDetail.Ingredient.Unit ?? "N/A";
                        }
                    }
                }
            }
        }

        private void RemoveRecipeDetailButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var recipeDetail = button.DataContext as AddNewProductViewModel.RecipeDetailViewModel;

            var listView = FindParent<ListView>(button);
            if (listView != null)
            {
                var currentVariant = listView.DataContext as AddNewProductViewModel.ProductVariantViewModel;
                if (currentVariant != null && recipeDetail != null)
                {
                    currentVariant.RecipeDetails.Remove(recipeDetail);
                }
            }
        }

        private T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            while (parentObject != null)
            {
                T parent = parentObject as T;
                if (parent != null)
                    return parent;
                parentObject = VisualTreeHelper.GetParent(parentObject);
            }
            return null;
        }

        private void IsToppingCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            AddVariantButton.IsEnabled = false;
            if (ViewModel.Variants.Count > 1)
            {
                var firstVariant = ViewModel.Variants.First();
                ViewModel.Variants.Clear();
                ViewModel.Variants.Add(firstVariant);
            }
        }

        private void IsToppingCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            AddVariantButton.IsEnabled = true;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ProductsPage));
        }
    }
}