using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using Windows.Storage;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using Kohi.Utils;
using Kohi.ViewModels;
using Kohi.Models;
using System.Threading.Tasks;
using Kohi.Errors;
using System.Diagnostics;

namespace Kohi.Views
{
    public sealed partial class AddNewCategoryPage : Page
    {
        private StorageFile selectedImageFile;
        private readonly IErrorHandler _errorHandler = new EmptyInputErrorHandler();
        public CategoryViewModel ViewModel { get; set; } = new CategoryViewModel();

        public AddNewCategoryPage()
        {
            this.InitializeComponent();
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
                    string categoryName = CategoryNameTextBox.Text;
                    if (string.IsNullOrEmpty(categoryName))
                    {
                        outtext.Text = "Vui lòng nhập tên danh mục.";
                        return "";
                    }
                    string normalizedName = Utils.StringUtils.NormalizeString(categoryName);
                    string fileExtension = Path.GetExtension(selectedImageFile.Name);
                    string flag = DateTime.Now.ToString("yyyyMMddHHmmss");
                    string imageFileName = normalizedName + flag + fileExtension;

                    Debug.WriteLine($"Đang lưu tệp: {imageFileName} vào {localFolder.Path}");
                    await selectedImageFile.CopyAsync(localFolder, imageFileName, NameCollisionOption.ReplaceExisting);
                    outtext.Text = $"Đã lưu hình ảnh '{imageFileName}' thành công.";
                    return imageFileName; // Chỉ trả về tên tệp
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
            string imgName = await SaveImage();
            var fields = new Dictionary<string, string>
            {
                { "Tên danh mục", CategoryNameTextBox.Text },
                { "Hình ảnh", imgName },
            };

            // Kiểm tra lỗi bằng IErrorHandler
            List<string> errors = _errorHandler?.HandleError(fields) ?? new List<string>();

            if (errors.Any())
            {
                outtext.Text = string.Join("\n", errors);
                return;
            }

            try
            {
                await ViewModel.Add(new CategoryModel
                {
                    Name = CategoryNameTextBox.Text,
                    ImageUrl = imgName // Lưu tên tệp vào ImageUrl
                });
                outtext.Text = "✅ Đã thêm danh mục thành công!";
                Frame.Navigate(typeof(CategoriesPage));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi thêm danh mục: {ex.Message}");
                outtext.Text = $"Lỗi khi thêm danh mục: {ex.Message}";
            }
        }
    }
}