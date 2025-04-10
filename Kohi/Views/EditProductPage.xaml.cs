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
    public sealed partial class EditProductPage : Page
    {
        public CategoryViewModel CategoryViewModel { get; set; } = new CategoryViewModel();
        public IngredientViewModel IngredientViewModel { get; set; } = new IngredientViewModel();
        public ProductViewModel ProductViewModel { get; set; } = new ProductViewModel();
        public ProductVariantViewModel ProductVariantViewModel { get; set; } = new ProductVariantViewModel();
        public RecipeDetailViewModel RecipeDetailViewModel { get; set; } = new RecipeDetailViewModel();
        public AddNewProductViewModel ViewModel { get; set; } = new AddNewProductViewModel();

        private StorageFile selectedImageFile;
        private readonly IErrorHandler _errorHandler;
        private ProductModel _currentProduct;

        public EditProductPage()
        {
            this.InitializeComponent();
            this.DataContext = ViewModel;

            var numericFields = new List<string> { "Price", "Cost", "Quantity" };
            var emptyInputHandler = new EmptyInputErrorHandler();
            var positiveNumberValidationHandler = new PositiveNumberValidationErrorHandler(numericFields);
            emptyInputHandler.SetNext(positiveNumberValidationHandler);
            _errorHandler = emptyInputHandler;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is int productId)
            {
                await LoadProductData(productId);
            }
        }

        private async Task LoadProductData(int productId)
        {
            _currentProduct = await ProductViewModel.GetById(productId.ToString());
            if (_currentProduct != null)
            {
                var category = await CategoryViewModel.GetById(_currentProduct.CategoryId.ToString());
                ProductNameTextBox.Text = _currentProduct.Name;
                CategoryProductComboBox.SelectedItem = category;
                IsActiveCheckBox.IsChecked = _currentProduct.IsActive;
                IsToppingCheckBox.IsChecked = _currentProduct.IsTopping;
                DescriptionTextBox.Text = _currentProduct.Description;

                // Hiển thị ảnh nếu có ImageUrl
                if (!string.IsNullOrEmpty(_currentProduct.ImageUrl))
                {
                    StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                    // Trích xuất tên tệp từ ImageUrl (loại bỏ đường dẫn đầy đủ nếu có)
                    string imageFileName = Path.GetFileName(_currentProduct.ImageUrl);
                    try
                    {
                        Debug.WriteLine($"Đang tìm tệp: {imageFileName} trong {localFolder.Path}");
                        StorageFile file = await localFolder.GetFileAsync(imageFileName);
                        using (var stream = await file.OpenAsync(FileAccessMode.Read))
                        {
                            var bitmapImage = new BitmapImage();
                            await bitmapImage.SetSourceAsync(stream);
                            mypic.Source = bitmapImage;
                        }
                        outtext.Text = "Ảnh hiện tại: " + imageFileName;
                    }
                    catch (FileNotFoundException ex)
                    {
                        Debug.WriteLine($"Tệp ảnh không tồn tại: {ex.Message}");
                        outtext.Text = "Ảnh không tồn tại trong thư mục lưu trữ.";
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Lỗi tải ảnh: {ex.Message}");
                        outtext.Text = $"Không thể tải ảnh: {ex.Message}";
                    }
                }

                ViewModel.Variants.Clear();
                ViewModel.Ingredients = new FullObservableCollection<IngredientModel>(await IngredientViewModel.GetAll());

                foreach (var variant in _currentProduct.ProductVariants)
                {
                    var variantVM = new AddNewProductViewModel.ProductVariantViewModel
                    {
                        Size = variant.Size,
                        Price = variant.Price,
                        Cost = variant.Cost
                    };
                    variant.RecipeDetails = await RecipeDetailViewModel.GetByProductVariantId(variant.Id);
                    foreach (var recipe in variant.RecipeDetails)
                    {
                        var matchingIngredient = ViewModel.Ingredients.FirstOrDefault(i => i.Id == recipe.IngredientId);
                        if (matchingIngredient != null)
                        {
                            variantVM.RecipeDetails.Add(new AddNewProductViewModel.RecipeDetailViewModel
                            {
                                Ingredient = matchingIngredient, 
                                Quantity = recipe.Quantity,
                                Unit = recipe.Unit
                            });
                            Debug.WriteLine($"Đã gán Ingredient: {matchingIngredient.Name}");
                        }
                        else
                        {
                            Debug.WriteLine($"Không tìm thấy Ingredient ID {recipe.IngredientId} trong ViewModel.Ingredients");
                            variantVM.RecipeDetails.Add(new AddNewProductViewModel.RecipeDetailViewModel
                            {
                                Ingredient = recipe.Ingredient,
                                Quantity = recipe.Quantity,
                                Unit = recipe.Unit
                            });
                        }
                    }
                    ViewModel.Variants.Add(variantVM);
                }

                VariantsListView.DataContext = ViewModel;
            }
            else
            {
                outtext.Text = "Không tìm thấy sản phẩm.";
            }
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
                using (IRandomAccessStream stream = await selectedImageFile.OpenAsync(FileAccessMode.Read))
                {
                    var bitmapImage = new BitmapImage();
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
                    return imageFileName;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Lỗi lưu ảnh: {ex.Message}");
                    outtext.Text = $"Lỗi khi lưu hình ảnh: {ex.Message}";
                    return "";
                }
            }
            else if (!string.IsNullOrEmpty(_currentProduct?.ImageUrl))
            {
                return Path.GetFileName(_currentProduct.ImageUrl); // Trả về tên tệp từ ImageUrl cũ
            }
            else
            {
                outtext.Text = "Chưa chọn hình ảnh.";
                return "";
            }
        }

        private async void saveButton_click(object sender, RoutedEventArgs e)
        {
            if (_currentProduct == null)
            {
                outtext.Text = "Không có sản phẩm để cập nhật.";
                return;
            }

            string imgName = await SaveImage();
            var selectedCategory = CategoryProductComboBox.SelectedItem as CategoryModel;
            List<string> errors = new List<string>();
            string categoryName = selectedCategory?.Name ?? "";

            var fields = new Dictionary<string, string>
    {
        { "Tên danh mục", categoryName },
        { "Hình sản phẩm", imgName },
        { "Tên sản phẩm", ProductNameTextBox.Text },
    };

            List<string> validationErrors = _errorHandler?.HandleError(fields) ?? new List<string>();
            errors.AddRange(validationErrors);

            if (HasErrors(errors))
            {
                return;
            }

            try
            {
                // Cập nhật thông tin Product
                _currentProduct.Name = ProductNameTextBox.Text;
                _currentProduct.ImageUrl = imgName;
                _currentProduct.CategoryId = selectedCategory?.Id ?? 0;
                _currentProduct.IsActive = IsActiveCheckBox.IsChecked == true;
                _currentProduct.IsTopping = IsToppingCheckBox.IsChecked == true;
                _currentProduct.Description = DescriptionTextBox.Text;
                _currentProduct.Category = selectedCategory;

                Debug.WriteLine("Trước khi cập nhật Product:");
                Debug.WriteLine($"  Id: {_currentProduct.Id}, Name: {_currentProduct.Name}, Variants: {_currentProduct.ProductVariants.Count}");

                var existingVariants = _currentProduct.ProductVariants.ToList();
                var updatedVariants = new List<ProductVariantModel>();
                var productVariantViewModel = new ProductVariantViewModel();
                var recipeDetailViewModel = new RecipeDetailViewModel();

                // Bước 1: Chuẩn bị dữ liệu Variants và Recipes
                for (int i = 0; i < ViewModel.Variants.Count; i++)
                {
                    var variantVM = ViewModel.Variants[i];
                    var variantFields = new Dictionary<string, string>
            {
                { "Size", variantVM.Size ?? "" },
                { "Price", variantVM.Price.ToString() },
                { "Cost", variantVM.Cost.ToString() }
            };

                    errors.AddRange(_errorHandler?.HandleError(variantFields) ?? new List<string>());
                    if (HasErrors(errors)) return;

                    ProductVariantModel variant;
                    if (i < existingVariants.Count)
                    {
                        // Dùng variant cũ tại vị trí i và cập nhật thông tin mới
                        variant = existingVariants[i];
                        variant.Size = variantVM.Size; // Cập nhật Size mới
                        variant.Price = variantVM.Price;
                        variant.Cost = variantVM.Cost;
                        variant.ProductId = _currentProduct.Id;
                    }
                    else
                    {
                        // Nếu không có variant cũ tương ứng, tạo mới
                        variant = new ProductVariantModel
                        {
                            Size = variantVM.Size,
                            Price = variantVM.Price,
                            Cost = variantVM.Cost,
                            ProductId = _currentProduct.Id,
                            RecipeDetails = new List<RecipeDetailModel>()
                        };
                    }

                    Debug.WriteLine($"Chuẩn bị ProductVariant (Size: {variant.Size}): Id = {variant.Id}, Price = {variant.Price}, Cost = {variant.Cost}");

                    // Xử lý RecipeDetails
                    if (variantVM.RecipeDetails.Any())
                    {
                        var existingRecipes = variant.RecipeDetails.ToList();
                        var updatedRecipes = new List<RecipeDetailModel>();

                        foreach (var recipeVM in variantVM.RecipeDetails)
                        {
                            var recipeFields = new Dictionary<string, string>
                    {
                        { "Ingredient", recipeVM.Ingredient?.Name ?? "" },
                        { "Quantity", recipeVM.Quantity.ToString() }
                    };

                            errors.AddRange(_errorHandler?.HandleError(recipeFields) ?? new List<string>());
                            if (HasErrors(errors)) return;

                            var existingRecipe = existingRecipes.FirstOrDefault(r => r.IngredientId == recipeVM.Ingredient?.Id);
                            RecipeDetailModel recipe;

                            if (existingRecipe != null)
                            {
                                // Dùng recipe cũ và cập nhật thông tin mới
                                recipe = existingRecipe;
                                recipe.Quantity = recipeVM.Quantity;
                                recipe.Unit = recipeVM.Unit;
                            }
                            else
                            {
                                // Nếu không có recipe cũ, tạo mới
                                recipe = new RecipeDetailModel
                                {
                                    IngredientId = recipeVM.Ingredient?.Id ?? 0,
                                    Quantity = recipeVM.Quantity,
                                    Unit = recipeVM.Unit
                                };
                            }

                            Debug.WriteLine($"  Chuẩn bị RecipeDetail: Id = {recipe.Id}, IngredientId = {recipe.IngredientId}, Quantity = {recipe.Quantity}");
                            updatedRecipes.Add(recipe);
                        }

                        variant.RecipeDetails.Clear();
                        variant.RecipeDetails.AddRange(updatedRecipes);
                    }
                    else
                    {
                        variant.RecipeDetails.Clear();
                    }

                    updatedVariants.Add(variant);
                }

                // Bước 2: Cập nhật Product
                Debug.WriteLine("Bắt đầu cập nhật Product...");
                await ProductViewModel.Update(_currentProduct.Id.ToString(), _currentProduct);
                Debug.WriteLine("Cập nhật Product hoàn tất.");

                // Bước 3: Cập nhật ProductVariants (không xóa, chỉ update)
                Debug.WriteLine("Bắt đầu cập nhật ProductVariants...");
                foreach (var variant in updatedVariants)
                {
                    string originalId = variant.Id.ToString();
                    Debug.WriteLine($"Cập nhật ProductVariant: Id = {variant.Id}, Size = {variant.Size}");
                    await productVariantViewModel.Update(originalId, variant);
                    Debug.WriteLine($"Sau khi cập nhật, ProductVariant Id = {variant.Id}");
                    if (variant.Id <= 0)
                    {
                        throw new Exception($"ProductVariant Size = {variant.Size} không được lưu đúng, Id = {variant.Id}");
                    }
                }
                Debug.WriteLine("Cập nhật ProductVariants hoàn tất.");

                // Bước 4: Cập nhật RecipeDetails (không xóa, chỉ update)
                Debug.WriteLine("Bắt đầu cập nhật RecipeDetails...");
                foreach (var variant in updatedVariants)
                {
                    foreach (var recipe in variant.RecipeDetails)
                    {
                        string originalId = recipe.Id.ToString();
                        recipe.ProductVariantId = variant.Id; // Gán ProductVariantId sau khi variant được lưu
                        Debug.WriteLine($"Cập nhật RecipeDetail: Id = {recipe.Id}, ProductVariantId = {recipe.ProductVariantId}");
                        await recipeDetailViewModel.Update(originalId, recipe);
                        Debug.WriteLine($"Sau khi cập nhật, RecipeDetail Id = {recipe.Id}");
                        if (recipe.Id <= 0)
                        {
                            throw new Exception($"RecipeDetail IngredientId = {recipe.IngredientId} không được lưu đúng, Id = {recipe.Id}");
                        }
                    }
                }
                Debug.WriteLine("Cập nhật RecipeDetails hoàn tất.");

                // Kiểm tra kết quả
                var updatedProduct = await ProductViewModel.GetById(_currentProduct.Id.ToString());
                Debug.WriteLine("Dữ liệu sau khi cập nhật:");
                Debug.WriteLine($"Product: Id = {updatedProduct.Id}, Name = {updatedProduct.Name}, Variants = {updatedProduct.ProductVariants.Count}");
                foreach (var v in updatedProduct.ProductVariants)
                {
                    Debug.WriteLine($"  Variant: Id = {v.Id}, Size = {v.Size}, Price = {v.Price}, Cost = {v.Cost}");
                    foreach (var r in v.RecipeDetails)
                    {
                        Debug.WriteLine($"    Recipe: Id = {r.Id}, IngredientId = {r.IngredientId}, Quantity = {r.Quantity}");
                    }
                }

                outtext.Text = "✅ Đã cập nhật sản phẩm thành công!";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi cập nhật sản phẩm: {ex.Message}");
                outtext.Text = $"Lỗi khi cập nhật sản phẩm: {ex.Message}";
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
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            VariantsListView.ItemsSource = null;
            VariantsListView.ItemsSource = ViewModel.Variants;
            Debug.WriteLine("Đã làm mới ItemsSource của VariantsListView");
        }

        private void AddRecipeDetailButton_Click(object sender, RoutedEventArgs e)
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