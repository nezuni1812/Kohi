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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Kohi.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddNewProductPage : Page
    {
        public CategoryViewModel CategoryViewModel { get; set; } = new CategoryViewModel();
        public IngredientViewModel IngredientViewModel { get; set; } = new IngredientViewModel();
        public ProductViewModel ProductViewModel { get; set; } = new ProductViewModel();
        public AddNewProductViewModel ViewModel { get; set; } = new AddNewProductViewModel();

        private StorageFile selectedImageFile;
        public AddNewProductPage()
        {
            this.InitializeComponent();
            this.DataContext = ViewModel;
            if (!ViewModel.Variants.Any())
            {
                ViewModel.Variants.Add(new AddNewProductViewModel.ProductVariantViewModel());
            }
        }
        private async void AddImageButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            // Initialize the picker
            var picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            // WinUI 3 requires a window handle to initialize file pickers
            // Get the current window handle
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            // Initialize the picker with the window handle
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            // Pick a file
            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                // Load the image into an image control
                using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read))
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    await bitmapImage.SetSourceAsync(stream);
                    mypic.Source = bitmapImage;

                    // You can store the file path or file itself for later use if needed
                    // For example, if you want to save the file path to your product model:
                    // Product.ImagePath = file.Path;
                    selectedImageFile = file;
                    // You can also update the UI to show the selected file name
                    outtext.Text = "Ảnh đã chọn: " + file.Name;
                }
            }
        }

        public async Task<string> SaveImage()
        {
            // Giả sử selectedFile là đối tượng chứa thông tin hình ảnh
            if (selectedImageFile != null)
            {
                try
                {
                    // Kiểm tra tệp có tồn tại và truy cập được không
                    StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                    string categoryName = ProductNameTextBox.Text;
                    if (string.IsNullOrEmpty(categoryName))
                    {
                        outtext.Text = "Vui lòng nhập tên sản phẩm.";
                        return "";
                    }
                    string normalizedName = Utils.StringUtils.NormalizeString(categoryName);
                    string fileExtension = Path.GetExtension(selectedImageFile.Name); // Lấy phần mở rộng (ví dụ: .jpg)
                    string flag = DateTime.Now.ToString("yyyyMMddHHmmss");
                    normalizedName = normalizedName + flag + fileExtension;
                    await selectedImageFile.CopyAsync(localFolder, normalizedName, NameCollisionOption.ReplaceExisting);
                    outtext.Text = $"Đã lưu sản phẩm '{normalizedName}' và hình ảnh thành công.";
                    return normalizedName;
                }
                catch (Exception ex)
                {
                    outtext.Text = "Lỗi khi lưu hình ảnh: " + ex.Message;
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
            // Lấy tên ảnh từ hàm SaveImage
            string imgName = await SaveImage();

            // Lấy CategoryModel được chọn từ ComboBox
            var selectedCategory = CategoryProductComboBox.SelectedItem as CategoryModel;

            // Kiểm tra dữ liệu bắt buộc
            if (selectedCategory == null || string.IsNullOrWhiteSpace(ProductNameTextBox.Text))
            {
                Debug.WriteLine("Lỗi: Vui lòng nhập tên sản phẩm và chọn nhóm sản phẩm!");
                return;
            }

            // Tạo ProductModel từ dữ liệu nhập
            var product = new ProductModel
            {
                Name = ProductNameTextBox.Text,
                ImageUrl = imgName,
                CategoryId = selectedCategory.Id,
                IsActive = IsActiveCheckBox.IsChecked == true,
                IsTopping = (bool)IsToppingCheckBox.IsChecked,
                Description = DescriptionTextBox.Text,
                ProductVariants = new List<ProductVariantModel>() // Khởi tạo danh sách Variants
            };

            try
            {
                // Lưu Variants và RecipeDetails vào ProductModel trước khi lưu
                if (ViewModel.Variants.Any())
                {
                    foreach (var variantVM in ViewModel.Variants)
                    {
                        // Kiểm tra dữ liệu Variant
                        if (string.IsNullOrWhiteSpace(variantVM.Size) || variantVM.Price < 0 || variantVM.Cost < 0)
                        {
                            Debug.WriteLine("Lỗi: Vui lòng nhập đầy đủ thông tin kích cỡ (Tên, Giá bán, Giá nhập)!");
                            return;
                        }

                        // In thông tin Variant để debug
                        System.Diagnostics.Debug.WriteLine($"Variant - Size: {variantVM.Size}, Price: {variantVM.Price}, Cost: {variantVM.Cost}");

                        // Tạo ProductVariantModel
                        var variant = new ProductVariantModel
                        {
                            Size = variantVM.Size,
                            Price = variantVM.Price,
                            Cost = variantVM.Cost,
                            RecipeDetails = new List<RecipeDetailModel>()
                        };

                        // Lưu RecipeDetails nếu có
                        if (variantVM.RecipeDetails.Any())
                        {
                            foreach (var recipeVM in variantVM.RecipeDetails)
                            {
                                // Kiểm tra dữ liệu RecipeDetail
                                if (recipeVM.Ingredient == null || recipeVM.Quantity <= 0)
                                {
                                    Debug.WriteLine("Lỗi: Vui lòng chọn nguyên vật liệu và nhập số lượng hợp lệ!");
                                    return;
                                }

                                // In thông tin RecipeDetail để debug
                                System.Diagnostics.Debug.WriteLine($"RecipeDetail - Ingredient: {recipeVM.Ingredient.Name}, Quantity: {recipeVM.Quantity}, Unit: {recipeVM.Unit}");

                                // Tạo RecipeDetailModel
                                var recipe = new RecipeDetailModel
                                {
                                    IngredientId = recipeVM.Ingredient.Id,
                                    Quantity = recipeVM.Quantity,
                                    Unit = recipeVM.Unit
                                };
                                variant.RecipeDetails.Add(recipe); // Thêm vào danh sách RecipeDetails của variant
                            }
                        }

                        product.ProductVariants.Add(variant); // Thêm variant vào danh sách ProductVariants của product
                    }
                }

                // Lưu ProductModel và lấy Id
                await ProductViewModel.Add(product);

                int productId = Service.GetKeyedSingleton<IDao>().Products.GetCount(); // Với MockDao

                // Cập nhật ProductId và lưu Variants/RecipeDetails riêng lẻ nếu cần
                foreach (var variant in product.ProductVariants)
                {
                    variant.ProductId = productId; // Gán ProductId cho variant
                    int variantId = Service.GetKeyedSingleton<IDao>().ProductVariants.GetCount() + 1;
                    Service.GetKeyedSingleton<IDao>().ProductVariants.Insert(variant);

                    foreach (var recipe in variant.RecipeDetails)
                    {
                        recipe.ProductVariantId = variantId; // Gán ProductVariantId cho recipe
                        int recipeId = Service.GetKeyedSingleton<IDao>().RecipeDetails.GetCount() + 1;
                        Service.GetKeyedSingleton<IDao>().RecipeDetails.Insert(recipe);
                    }
                }

                // Hiển thị thông báo thành công
                Debug.WriteLine("Lưu thành công");

                // Xóa dữ liệu trên giao diện sau khi lưu
                ProductNameTextBox.Text = string.Empty;
                CategoryProductComboBox.SelectedItem = null;
                IsActiveCheckBox.IsChecked = false;
                IsToppingCheckBox.IsChecked = false;
                DescriptionTextBox.Text = string.Empty;
                ViewModel.Variants.Clear();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi: {ex.Message}");
            }
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
            var button = sender as Button; // Ép kiểu sender thành Button
            if (button != null)
            {
                var currentVariant = button.DataContext as AddNewProductViewModel.ProductVariantViewModel;
                if (currentVariant != null)
                {
                    var newRecipeDetail = new AddNewProductViewModel.RecipeDetailViewModel();
                    currentVariant.RecipeDetails.Add(newRecipeDetail);
                }
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox != null)
            {
                // Lấy RecipeDetailViewModel từ DataContext của ComboBox
                var recipeDetail = comboBox.DataContext as AddNewProductViewModel.RecipeDetailViewModel;
                if (recipeDetail != null && recipeDetail.Ingredient != null)
                {
                    // Tìm TextBox (IngredientUnit) trong cùng hàng (Grid)
                    var grid = FindParent<Grid>(comboBox);
                    if (grid != null)
                    {
                        var ingredientUnit = grid.FindName("IngredientUnit") as TextBox;
                        if (ingredientUnit != null)
                        {
                            // Cập nhật TextBox với Unit từ Ingredient
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
