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
using Kohi.Utils;
using Microsoft.UI.Dispatching;

namespace Kohi.Views
{
    public sealed partial class EditProductPage : Page
    {
        public CategoryViewModel CategoryViewModel { get; set; } = new CategoryViewModel();
        public IngredientViewModel IngredientViewModel { get; set; } = new IngredientViewModel();
        public ProductViewModel ProductViewModel { get; set; } = new ProductViewModel();
        public ProductVariantViewModel ProductVariantViewModel { get; set; } = new ProductVariantViewModel();
        public RecipeDetailViewModel RecipeDetailViewModel { get; set; } = new RecipeDetailViewModel();
        public EditProductViewModel ViewModel { get; set; } = new EditProductViewModel();

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

            if (e.Parameter is ProductModel product)
            {
                await LoadProductData(product);
            }
            else
            {
                DispatcherQueue.GetForCurrentThread().TryEnqueue(() =>
                {
                    outtext.Text = "Không nhận được dữ liệu sản phẩm.";
                });
                Debug.WriteLine("Parameter is not a ProductModel.");
            }
        }

        private async Task LoadProductData(ProductModel product)
        {
            _currentProduct = product;
            if (_currentProduct != null)
            {
                var selectedCategory = _currentProduct.Category;
                ProductNameTextBox.Text = _currentProduct.Name;
                if (selectedCategory != null)
                {
                    CategoryProductComboBox.SelectedIndex = selectedCategory.Id - 1;
                }

                Debug.WriteLine($"Categories count: {ViewModel.Categories.Count}");
                for (int i = 0; i < ViewModel.Categories.Count; i++)
                {
                    Debug.WriteLine($"Index {i}: Id={ViewModel.Categories[i].Id}, Name={ViewModel.Categories[i].Name}");
                }

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
                    var variantVM = new EditProductViewModel.ProductVariantViewModel
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
                            variantVM.RecipeDetails.Add(new EditProductViewModel.RecipeDetailViewModel
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
                            variantVM.RecipeDetails.Add(new EditProductViewModel.RecipeDetailViewModel
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
                Debug.WriteLine("Bắt đầu xử lý ProductVariants...");
                foreach (var variant in updatedVariants)
                {
                    if (variant.Id <= 0)
                    {
                        Debug.WriteLine($"Thêm mới ProductVariant: Size = {variant.Size}");
                        await productVariantViewModel.Add(variant);
                        Debug.WriteLine($"Đã thêm mới ProductVariant, Id mới = {variant.Id}");
                    }
                    else
                    {
                        string originalId = variant.Id.ToString();
                        Debug.WriteLine($"Cập nhật ProductVariant: Id = {variant.Id}, Size = {variant.Size}");
                        await productVariantViewModel.Update(originalId, variant);
                        Debug.WriteLine($"Sau khi cập nhật, ProductVariant Id = {variant.Id}");
                    }

                    if (variant.Id <= 0)
                    {
                        throw new Exception($"ProductVariant Size = {variant.Size} không được lưu đúng, Id = {variant.Id}");
                    }
                }
                Debug.WriteLine("Xử lý ProductVariants hoàn tất.");

                // Bước 4: Cập nhật RecipeDetails
                Debug.WriteLine("Kiểm tra và xóa RecipeDetails đã bị xóa trong giao diện...");
                foreach (var variant in updatedVariants)
                {
                    var originalRecipes = (await recipeDetailViewModel.GetByProductVariantId(variant.Id))
                        ?? new List<RecipeDetailModel>();
                    var currentRecipeIds = new HashSet<int>(variant.RecipeDetails.Select(r => r.Id).Where(id => id > 0));
                    var originalRecipeIds = new HashSet<int>(originalRecipes.Select(r => r.Id));
                    var recipesToDelete = originalRecipeIds.Except(currentRecipeIds);

                    Debug.WriteLine($"Variant Id = {variant.Id}, Size = {variant.Size}:");
                    Debug.WriteLine($"  Original Recipes: {string.Join(", ", originalRecipes.Select(r => r.Id))}");
                    Debug.WriteLine($"  Current Recipes: {string.Join(", ", currentRecipeIds)}");
                    Debug.WriteLine($"  Recipes to Delete: {string.Join(", ", recipesToDelete)}");

                    foreach (var recipeId in recipesToDelete)
                    {
                        Debug.WriteLine($"Xóa RecipeDetail: Id = {recipeId}");
                        await recipeDetailViewModel.Delete(recipeId.ToString());
                    }
                }

                Debug.WriteLine("Bắt đầu cập nhật RecipeDetails...");
                foreach (var variant in updatedVariants)
                {
                    foreach (var recipe in variant.RecipeDetails)
                    {
                        recipe.ProductVariantId = variant.Id; 
                        Debug.WriteLine($"Xử lý RecipeDetail: Id = {recipe.Id}, ProductVariantId = {recipe.ProductVariantId}");

                        if (recipe.Id <= 0) 
                        {
                            Debug.WriteLine($"Thêm mới RecipeDetail: IngredientId = {recipe.IngredientId}, Quantity = {recipe.Quantity}");
                            await recipeDetailViewModel.Add(recipe);
                            Debug.WriteLine($"Đã thêm mới RecipeDetail");
                        }
                        else 
                        {
                            string originalId = recipe.Id.ToString();
                            Debug.WriteLine($"Cập nhật RecipeDetail: Id = {recipe.Id}");
                            await recipeDetailViewModel.Update(originalId, recipe);
                            Debug.WriteLine($"Sau khi cập nhật, RecipeDetail Id = {recipe.Id}");
                        }
                    }
                }
                Debug.WriteLine("Cập nhật RecipeDetails hoàn tất.");

                // Kiểm tra kết quả
                var updatedProduct = await ProductViewModel.GetById(_currentProduct.Id.ToString());
                Debug.WriteLine("Dữ liệu sau khi cập nhật:");
                Debug.WriteLine($"Product: Id = {updatedProduct.Id}, Name = {updatedProduct.Name}, Variants = {updatedProduct.ProductVariants.Count}");
                foreach (var variant in updatedProduct.ProductVariants)
                {
                    Debug.WriteLine($"  Variant: Id = {variant.Id}, Size = {variant.Size}, Price = {variant.Price}, Cost = {variant.Cost}");
                    foreach (var recipeDetail in variant.RecipeDetails)
                    {
                        Debug.WriteLine($"Recipe: Id = {recipeDetail.Id}, IngredientId = {recipeDetail.IngredientId}, Quantity = {recipeDetail.Quantity}");
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
                ViewModel.Variants.Add(new EditProductViewModel.ProductVariantViewModel());
            }
        }

        private void RemoveVariantButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var variant = (EditProductViewModel.ProductVariantViewModel)button.Tag;

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
                var currentVariant = button.DataContext as EditProductViewModel.ProductVariantViewModel;
                if (currentVariant != null)
                {
                    currentVariant.RecipeDetails.Add(new EditProductViewModel.RecipeDetailViewModel());
                }
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox != null)
            {
                var recipeDetail = comboBox.DataContext as EditProductViewModel.RecipeDetailViewModel;
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
            var recipeDetailVM = button.DataContext as EditProductViewModel.RecipeDetailViewModel;

            var listView = FindParent<ListView>(button);
            if (listView != null)
            {
                var currentVariantVM = listView.DataContext as EditProductViewModel.ProductVariantViewModel;
                if (currentVariantVM != null && recipeDetailVM != null)
                {
                    currentVariantVM.RecipeDetails.Remove(recipeDetailVM);
                    Debug.WriteLine($"Đã xóa RecipeDetail Id = {recipeDetailVM.Id} khỏi Variant Size = {currentVariantVM.Size}");
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