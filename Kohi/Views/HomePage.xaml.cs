using Kohi.Models;
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
using System.Threading.Tasks;
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
        public HomePageViewModel ViewModel { get; set; } = new HomePageViewModel();
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
            //ViewModel.AddProduct();
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

        private async void ShowProductDialog_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            ProductModel product = button.DataContext as ProductModel;

            ContentDialog productDialog = new ContentDialog
            {
                Title = product.Name,
                PrimaryButtonText = "Xác nhận",
                PrimaryButtonStyle = (Style)Application.Current.Resources["AccentButtonStyle"],
                SecondaryButtonText = "Hủy",
                CloseButtonText = "Đóng",
                XamlRoot = this.XamlRoot
            };

            Grid dialogContent = new Grid { Width = 300, ColumnSpacing = 10 };
            dialogContent.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.5, GridUnitType.Star) });
            dialogContent.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.5, GridUnitType.Star) });
            dialogContent.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Product Variant
            dialogContent.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Đường và Đá
            dialogContent.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Topping
            dialogContent.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            StackPanel variantPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 10) };
            variantPanel.Children.Add(new TextBlock { Text = "Đơn vị:" });
            ItemsControl variants = new ItemsControl { ItemsSource = product.ProductVariants };
            variants.ItemTemplate = (DataTemplate)Resources["ProductVariantTemplate"];
            variantPanel.Children.Add(variants);
            Grid.SetRow(variantPanel, 0);
            Grid.SetColumnSpan(variantPanel, 2);
            dialogContent.Children.Add(variantPanel);

            RadioButtons sugarOptions = new RadioButtons { Header = "Đường:", SelectedIndex = 0 };
            sugarOptions.Items.Add(new RadioButton { Content = "100%" });
            sugarOptions.Items.Add(new RadioButton { Content = "75%" });
            sugarOptions.Items.Add(new RadioButton { Content = "50%" });
            sugarOptions.Items.Add(new RadioButton { Content = "25%" });
            sugarOptions.Items.Add(new RadioButton { Content = "0%" });
            Grid.SetRow(sugarOptions, 1);
            Grid.SetColumn(sugarOptions, 0);
            dialogContent.Children.Add(sugarOptions);

            RadioButtons iceOptions = new RadioButtons { Header = "Đá:", SelectedIndex = 0 };
            iceOptions.Items.Add(new RadioButton { Content = "100%" });
            iceOptions.Items.Add(new RadioButton { Content = "75%" });
            iceOptions.Items.Add(new RadioButton { Content = "50%" });
            iceOptions.Items.Add(new RadioButton { Content = "25%" });
            iceOptions.Items.Add(new RadioButton { Content = "0%" });
            Grid.SetRow(iceOptions, 1);
            Grid.SetColumn(iceOptions, 1);
            dialogContent.Children.Add(iceOptions);

            StackPanel toppingPanel = new StackPanel { Margin = new Thickness(0, 10, 0, 0) };
            toppingPanel.Children.Add(new TextBlock { Text = "Topping:", Margin = new Thickness(0, 0, 0, 5) });
            var toppingList = ViewModel.ToppingProducts;
            ItemsControl toppings = new ItemsControl { ItemsSource = toppingList };
            toppings.ItemTemplate = (DataTemplate)Resources["ToppingTemplate"];
            toppingPanel.Children.Add(toppings);
            Grid.SetRow(toppingPanel, 2);
            Grid.SetColumnSpan(toppingPanel, 2);
            dialogContent.Children.Add(toppingPanel);

            productDialog.Content = dialogContent;

            productDialog.PrimaryButtonClick += async (s, args) =>
            {
                string selectedSugar = (sugarOptions.SelectedItem as RadioButton)?.Content.ToString();
                string selectedIce = (iceOptions.SelectedItem as RadioButton)?.Content.ToString();

                ProductVariantModel selectedVariant = null;
                foreach (var item in product.ProductVariants)
                {
                    var container = variants.ContainerFromItem(item) as ContentPresenter;
                    if (container != null)
                    {
                        var radioButton = FindVisualChild<RadioButton>(container);
                        if (radioButton != null && radioButton.IsChecked == true)
                        {
                            selectedVariant = item;
                            break;
                        }
                    }
                }

                if (selectedVariant == null)
                {
                    args.Cancel = true;
                    productDialog.Hide();
                    await ShowErrorContentDialog(productDialog.XamlRoot, "Đơn vị không thể để trống");
                    await productDialog.ShowAsync();
                    return;
                }

                // Create new InvoiceDetailModel
                var newItem = new InvoiceDetailModel
                {
                    ProductId = product.Id,
                    ProductVariant = selectedVariant,
                    SugarLevel = int.Parse(selectedSugar.Replace("%", "")), // Convert "100%" to 100
                    IceLevel = int.Parse(selectedIce.Replace("%", "")),     // Convert "100%" to 100
                    Toppings = new List<OrderToppingModel>()
                };

                // Add selected toppings
                foreach (var topping in ViewModel.ToppingProducts)
                {
                    var container = toppings.ContainerFromItem(topping) as ContentPresenter;
                    if (container != null)
                    {
                        var checkBox = FindVisualChild<CheckBox>(container);
                        if (checkBox != null && checkBox.IsChecked == true)
                        {
                            newItem.Toppings.Add(new OrderToppingModel
                            {
                                ProductId = topping.Id,
                                ProductVariant = topping.ProductVariants[0] // Assuming first variant for topping
                            });
                        }
                    }
                }

                ViewModel.OrderItems.Add(newItem);

                string variantInfo = selectedVariant != null ? $"{selectedVariant.Size} - {selectedVariant.Price}" : "Không chọn";
                System.Diagnostics.Debug.WriteLine($"Product: {product.Name}, Variant: {variantInfo}, Sugar: {selectedSugar}, Ice: {selectedIce}, Toppings: {newItem.Toppings.Count}");
            };

            await productDialog.ShowAsync();
        }
        private void DeleteInvoiceDetail_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is InvoiceDetailModel item)
            {
                ViewModel.OrderItems.Remove(item); // Assuming OrderItems is your ObservableCollection
            }
        }

        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T foundChild)
                {
                    return foundChild;
                }
                var result = FindVisualChild<T>(child);
                if (result != null) return result;
            }
            return null;
        }

        private void ProductDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            
        }

        private async Task ShowErrorContentDialog(XamlRoot xamlRoot, string errorMessage)
        {
            ContentDialog errorDialog = new ContentDialog
            {
                Title = "Lỗi",
                Content = errorMessage,
                CloseButtonText = "Đóng",
                XamlRoot = xamlRoot
            };

            await errorDialog.ShowAsync();
        }

        private void ProductDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            
        }

        private void ProductDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Xử lý khi người dùng nhấn Hủy
        }
    }
}
