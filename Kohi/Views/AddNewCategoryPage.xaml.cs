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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Kohi.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddNewCategoryPage : Page
    {
        public AddNewCategoryPage()
        {
            this.InitializeComponent();
        }

        private async void AddImageButton_Click(object sender, RoutedEventArgs e)
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

                    // You can also update the UI to show the selected file name
                    outtext.Text = "Ảnh đã chọn: " + file.Name;
                }
            }
        }

        private void saveButton_click(object sender, RoutedEventArgs e)
        {

        }
    }
}
