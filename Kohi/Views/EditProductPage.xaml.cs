using Kohi.Models;
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

namespace Kohi.Views
{
    public sealed partial class EditProductPage : Page
    {
        public CategoryViewModel CategoryViewModel { get; set; } = new CategoryViewModel();
        public IngredientViewModel IngredientViewModel { get; set; } = new IngredientViewModel();
        public ProductViewModel ProductViewModel { get; set; } = new ProductViewModel();
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
                ProductNameTextBox.Text = _currentProduct.Name;
                CategoryProductComboBox.SelectedItem = _currentProduct.Category;
                IsActiveCheckBox.IsChecked = _currentProduct.IsActive;
                IsToppingCheckBox.IsChecked = _currentProduct.IsTopping;
                DescriptionTextBox.Text = _currentProduct.Description;

                // Hiển thị ảnh nếu có ImageUrl
                if (!string.IsNullOrEmpty(_currentProduct.ImageUrl))
                {
                    StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                    try
                    {
                        Debug.WriteLine($"Đang tìm tệp: {_currentProduct.ImageUrl} trong {localFolder.Path}");
                        StorageFile file = await localFolder.GetFileAsync(_currentProduct.ImageUrl);
                        using (var stream = await file.OpenAsync(FileAccessMode.Read))
                        {
                            var bitmapImage = new BitmapImage();
                            await bitmapImage.SetSourceAsync(stream);
                            mypic.Source = bitmapImage;
                        }
                        outtext.Text = "Ảnh hiện tại: " + _currentProduct.ImageUrl;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Lỗi tải ảnh: {ex.Message}");
                        outtext.Text = $"Không thể tải ảnh: {ex.Message}";
                    }
                }

                // Load Variants và RecipeDetails
                ViewModel.Variants.Clear();
                foreach (var variant in _currentProduct.ProductVariants)
                {
                    var variantVM = new AddNewProductViewModel.ProductVariantViewModel
                    {
                        Size = variant.Size,
                        Price = variant.Price,
                        Cost = variant.Cost
                    };
                    foreach (var recipe in variant.RecipeDetails)
                    {
                        variantVM.RecipeDetails.Add(new AddNewProductViewModel.RecipeDetailViewModel
                        {
                            Ingredient = recipe.Ingredient,
                            Quantity = recipe.Quantity,
                            Unit = recipe.Unit
                        });
                    }
                    ViewModel.Variants.Add(variantVM);
                }

                // Load Ingredients cho RecipeDetails
                //ViewModel.Ingredients = new ObservableCollection<IngredientModel>(await IngredientViewModel.GetAll());
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
                return _currentProduct.ImageUrl; // Giữ ảnh cũ nếu không chọn ảnh mới
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
                _currentProduct.Name = ProductNameTextBox.Text;
                _currentProduct.ImageUrl = imgName;
                _currentProduct.CategoryId = selectedCategory?.Id ?? 0;
                _currentProduct.IsActive = IsActiveCheckBox.IsChecked == true;
                _currentProduct.IsTopping = IsToppingCheckBox.IsChecked == true;
                _currentProduct.Description = DescriptionTextBox.Text;
                _currentProduct.Category = selectedCategory;

                // Cập nhật Variants và RecipeDetails
                _currentProduct.ProductVariants.Clear();
                foreach (var variantVM in ViewModel.Variants)
                {
                    var variantFields = new Dictionary<string, string>
                    {
                        { "Size", variantVM.Size ?? "" },
                        { "Price", variantVM.Price.ToString() },
                        { "Cost", variantVM.Cost.ToString() }
                    };

                    List<string> variantErrors = _errorHandler?.HandleError(variantFields) ?? new List<string>();
                    errors.AddRange(variantErrors);

                    if (HasErrors(errors))
                    {
                        return;
                    }

                    var variant = new ProductVariantModel
                    {
                        Size = variantVM.Size,
                        Price = variantVM.Price,
                        Cost = variantVM.Cost,
                        Product = _currentProduct,
                        RecipeDetails = new List<RecipeDetailModel>(),
                        InvoiceDetails = new List<InvoiceDetailModel>(),
                        Toppings = new List<OrderToppingModel>()
                    };

                    if (variantVM.RecipeDetails.Any())
                    {
                        foreach (var recipeVM in variantVM.RecipeDetails)
                        {
                            var recipeFields = new Dictionary<string, string>
                            {
                                { "Ingredient", recipeVM.Ingredient?.Name ?? "" },
                                { "Quantity", recipeVM.Quantity.ToString() }
                            };

                            List<string> recipeErrors = _errorHandler?.HandleError(recipeFields) ?? new List<string>();
                            errors.AddRange(recipeErrors);

                            if (HasErrors(errors))
                            {
                                return;
                            }

                            var recipe = new RecipeDetailModel
                            {
                                IngredientId = recipeVM.Ingredient?.Id ?? 0,
                                Quantity = recipeVM.Quantity,
                                Unit = recipeVM.Unit,
                                ProductVariant = variant,
                                Ingredient = recipeVM.Ingredient
                            };
                            variant.RecipeDetails.Add(recipe);
                        }
                    }

                    _currentProduct.ProductVariants.Add(variant);
                }

                await ProductViewModel.Update(_currentProduct.Id.ToString(), _currentProduct);

                outtext.Text = "✅ Đã cập nhật sản phẩm thành công!";
                Frame.Navigate(typeof(CategoriesPage)); // Hoặc trang danh sách sản phẩm
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
    }
}