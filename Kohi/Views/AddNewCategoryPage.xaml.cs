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

        private async void AddImageButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            Image img = sender as Image;

            picker.ViewMode = PickerViewMode.Thumbnail;

            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

            //picker.FileTypeFilter.Add(".jpg");

            StorageFile file = await picker.PickSingleFileAsync();
        }

        private void saveButton_click(object sender, RoutedEventArgs e)
        {

        }
    }
}
