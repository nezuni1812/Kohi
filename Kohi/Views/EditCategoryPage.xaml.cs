using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Kohi.Models;
using Kohi.ViewModels;
using Windows.Storage.Pickers;
using Windows.Storage;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using System;
using System.IO;
using Kohi.Utils;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Kohi.Errors;
using System.Collections.Generic;
using System.Linq;

namespace Kohi.Views
{
    public sealed partial class EditCategoryPage : Page
    {
        public int CategoryId { get; set; }
        public CategoryViewModel CategoryViewModel { get; set; } = new CategoryViewModel();
        private CategoryModel _currentCategory;
        private StorageFile _selectedImageFile;
        private readonly IErrorHandler _errorHandler; // Thêm IErrorHandler
        public EditCategoryPage()
        {
            this.InitializeComponent();
            _errorHandler = new EmptyInputErrorHandler(); // Khởi tạo EmptyInputErrorHandler
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is int id)
            {
                CategoryId = id;
                LoadCategoryData();
            }
        }

        private async void LoadCategoryData()
        {
            _currentCategory = await CategoryViewModel.GetById(CategoryId.ToString());
            if (_currentCategory != null)
            {
                CategoryNameTextBox.Text = _currentCategory.Name;

                if (!string.IsNullOrEmpty(_currentCategory.ImageUrl))
                {
                    StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                    string imageFileName = Path.GetFileName(_currentCategory.ImageUrl);
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
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Lỗi tải ảnh: {ex.Message}");
                        outtext.Text = $"Không thể tải ảnh: {ex.Message}";
                    }
                }
                else
                {
                    outtext.Text = "Không có ảnh cho danh mục này.";
                }
            }
            else
            {
                outtext.Text = "Không tìm thấy danh mục.";
            }
        }

        private async void AddImageButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
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

            _selectedImageFile = await picker.PickSingleFileAsync();
            if (_selectedImageFile != null)
            {
                using (IRandomAccessStream stream = await _selectedImageFile.OpenAsync(FileAccessMode.Read))
                {
                    var bitmapImage = new BitmapImage();
                    await bitmapImage.SetSourceAsync(stream);
                    mypic.Source = bitmapImage;
                }
                outtext.Text = "Ảnh đã chọn: " + _selectedImageFile.Name;
            }
        }

        private async Task<string> SaveImage()
        {
            if (_selectedImageFile != null)
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
                    string fileExtension = Path.GetExtension(_selectedImageFile.Name);
                    string flag = DateTime.Now.ToString("yyyyMMddHHmmss");
                    string imageFileName = normalizedName + flag + fileExtension;

                    Debug.WriteLine($"Đang lưu tệp: {imageFileName} vào {localFolder.Path}");
                    await _selectedImageFile.CopyAsync(localFolder, imageFileName, NameCollisionOption.ReplaceExisting);
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
            else if (!string.IsNullOrEmpty(_currentCategory?.ImageUrl))
            {
                return Path.GetFileName(_currentCategory.ImageUrl);
            }
            else
            {
                outtext.Text = "Chưa chọn hình ảnh.";
                return "";
            }
        }

        private async void saveButton_click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (_currentCategory == null)
            {
                outtext.Text = "Không có danh mục để cập nhật.";
                return;
            }

            string imgName = await SaveImage();
            // Sử dụng IErrorHandler để kiểm tra các trường không trống
            var fields = new Dictionary<string, string>
            {
                { "Tên danh mục", CategoryNameTextBox.Text },
                { "Hình ảnh", imgName },
            };

            List<string> errors = _errorHandler?.HandleError(fields) ?? new List<string>();
            if (errors.Any())
            {
                outtext.Text = string.Join("\n", errors);
                return;
            }

            _currentCategory.Name = CategoryNameTextBox.Text;
            _currentCategory.ImageUrl = imgName;

            try
            {
                await CategoryViewModel.Update(CategoryId.ToString(), _currentCategory);
                outtext.Text = "✅ Đã cập nhật danh mục thành công!";
                Frame.Navigate(typeof(CategoriesPage));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi cập nhật danh mục: {ex.Message}");
                outtext.Text = $"Lỗi khi lưu danh mục: {ex.Message}";
            }
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(CategoriesPage));
        }
    }
}