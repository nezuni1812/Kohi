using Kohi.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;

//using Kohi.ViewModels;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Kohi.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>

    public sealed partial class HomePage : Page
    {
        public OrderPageViewModel ViewModel { get; set; } = new OrderPageViewModel();
        public HomePage()
        {
            this.InitializeComponent();
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = ViewModel;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
           // ViewModel.AddProduct();
        }

        private void onCategorySelectionChanged(object sender, ItemsViewSelectionChangedEventArgs e)
        {
            var selectedItem = categoriesList.SelectedItem;

            if (selectedItem != null)
            {
                var id = selectedItem.GetType().GetProperty("Id")?.GetValue(selectedItem, null)?.ToString();
                if (id != null)
                {
                    Debug.WriteLine(id);
                }
            }
        }
        private async void showProductDialog_Click(object sender, ItemClickEventArgs e)
        {
            Debug.WriteLine("showProductDialog_Click triggered"); 
            var clickedItem = e.ClickedItem;

            if (clickedItem != null)
            {
                var itemName = clickedItem.GetType().GetProperty("Id")?.GetValue(clickedItem, null)?.ToString();
                if (itemName != null)
                {
                    var result = await ProductDialog.ShowAsync();

                    if (result == ContentDialogResult.Primary)
                    {
                        string quantity = QuantityTextBox.Text;
                        string note = NoteTextBox.Text;
                    }
                    else
                    {
                        // Người dùng nhấn Hủy hoặc Đóng
                        // Xử lý tùy chọn
                    }
                }
            }
        }

        private void ProductDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Xử lý khi người dùng nhấn Xác nhận
            // Ví dụ: Kiểm tra dữ liệu nhập vào
            if (string.IsNullOrEmpty(QuantityTextBox.Text))
            {
                args.Cancel = true; // Ngăn không cho đóng dialog nếu dữ liệu không hợp lệ
            }
        }

        private void ProductDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Xử lý khi người dùng nhấn Hủy
        }

        private void ProductDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Xử lý khi người dùng nhấn Hủy
        }
    }
}
