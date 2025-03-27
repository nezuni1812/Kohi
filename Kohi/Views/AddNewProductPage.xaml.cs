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
            string imgName = await SaveImage();
            //await ProductViewModel.Add(new CategoryModel()
            //{
            //    Name = ProductNameTextBox.Text,
            //    ImageUrl = imgName,
            //});
        }

        private void AddVariantButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Variants.Add(new AddNewProductViewModel.ProductVariantViewModel());
        }

        private void RemoveVariantButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var variant = (AddNewProductViewModel.ProductVariantViewModel)button.Tag;
            ViewModel.Variants.Remove(variant);
        }

        private async void AddRecipeDetailButton_Click(object sender, RoutedEventArgs e)
        {
            var currentVariant = ViewModel.Variants.LastOrDefault();

            if (currentVariant != null)
            {
                var newRecipeDetail = new AddNewProductViewModel.RecipeDetailViewModel();
                currentVariant.RecipeDetails.Add(newRecipeDetail);
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
    }
}
