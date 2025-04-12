using Kohi.Models;
using Kohi.ViewModels;
using Kohi.Views.Converter;
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

using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
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
            this.DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //ViewModel.AddProduct();
        }

        private void onCategorySelectionChanged(object sender, ItemsViewSelectionChangedEventArgs e)
        {
            var selectedItem = categoriesList.SelectedItem as CategoryModel;

            if (selectedItem != null)
            {
                int? categoryId = selectedItem.Id;
                Debug.WriteLine($"Selected category: {selectedItem.Name}, ID: {categoryId}");
                ViewModel.FilterProductsByCategory(categoryId);
            }
            else
            {
                ViewModel.FilterProductsByCategory(null);
            }
        }

        private void ShowAllProducts_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.FilterProductsByCategory(null);
            categoriesList.DeselectAll();
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

            ScrollViewer scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Padding = new Thickness(0, 0, 15, 0)
            };

            Grid dialogContent = new Grid { Width = 300, ColumnSpacing = 10 };
            dialogContent.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.5, GridUnitType.Star) });
            dialogContent.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.5, GridUnitType.Star) });
            dialogContent.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Product Variant
            dialogContent.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Số lượng sản phẩm
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

            StackPanel quantityPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 10) };
            quantityPanel.Children.Add(new TextBlock { Text = "Số lượng:" });
            NumberBox productQuantityBox = new NumberBox
            {
                Value = 1,
                Minimum = 1,
                Maximum = 100,
                SmallChange = 1,
                LargeChange = 10,
                Width = 100,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            quantityPanel.Children.Add(productQuantityBox);
            Grid.SetRow(quantityPanel, 1);
            Grid.SetColumnSpan(quantityPanel, 2);
            dialogContent.Children.Add(quantityPanel);

            RadioButtons sugarOptions = new RadioButtons { Header = "Đường:", SelectedIndex = 0 };
            sugarOptions.ItemTemplate = (DataTemplate)Resources["RadioButtonTemplate"];
            sugarOptions.ItemsSource = new List<string> { "100%", "75%", "50%", "25%", "0%" };
            Grid.SetRow(sugarOptions, 2);
            Grid.SetColumn(sugarOptions, 0);
            dialogContent.Children.Add(sugarOptions);

            RadioButtons iceOptions = new RadioButtons { Header = "Đá:", SelectedIndex = 0 };
            iceOptions.ItemTemplate = (DataTemplate)Resources["RadioButtonTemplate"];
            iceOptions.ItemsSource = new List<string> { "100%", "75%", "50%", "25%", "0%" };
            Grid.SetRow(iceOptions, 2);
            Grid.SetColumn(iceOptions, 1);
            dialogContent.Children.Add(iceOptions);

            // Topping
            StackPanel toppingPanel = new StackPanel { Margin = new Thickness(0, 10, 0, 0) };
            toppingPanel.Children.Add(new TextBlock { Text = "Topping:", Margin = new Thickness(0, 0, 0, 5) });
            var toppingList = ViewModel.ToppingProducts;
            ItemsControl toppings = new ItemsControl { ItemsSource = toppingList };
            toppings.ItemTemplate = (DataTemplate)Resources["ToppingTemplate"];
            toppingPanel.Children.Add(toppings);
            Grid.SetRow(toppingPanel, 3);
            Grid.SetColumnSpan(toppingPanel, 2);
            dialogContent.Children.Add(toppingPanel);

            // Dictionary để lưu NumberBox cho từng topping
            Dictionary<ProductModel, NumberBox> toppingQuantities = new Dictionary<ProductModel, NumberBox>();

            // Xử lý sự kiện khi ItemsControl render các item
            toppings.Loaded += (s, args) =>
            {
                foreach (var topping in toppingList)
                {
                    var container = toppings.ContainerFromItem(topping) as ContentPresenter;
                    if (container != null)
                    {
                        var checkBox = FindVisualChild<CheckBox>(container);
                        if (checkBox != null)
                        {
                            checkBox.Checked += (s2, e2) =>
                            {
                                if (!toppingQuantities.ContainsKey(topping))
                                {
                                    NumberBox quantityBox = new NumberBox
                                    {
                                        Value = 1,
                                        Minimum = 1,
                                        Maximum = 100,
                                        SmallChange = 1,
                                        LargeChange = 10,
                                        Width = 100,
                                        Margin = new Thickness(20, 0, 0, 0)
                                    };
                                    toppingQuantities[topping] = quantityBox;
                                    var toppingGrid = FindVisualChild<Grid>(checkBox);
                                    if (toppingGrid != null)
                                    {
                                        Grid.SetColumn(quantityBox, 2);
                                        toppingGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                                        toppingGrid.Children.Add(quantityBox);
                                    }
                                }
                            };
                            checkBox.Unchecked += (s2, e2) =>
                            {
                                if (toppingQuantities.ContainsKey(topping))
                                {
                                    var quantityBox = toppingQuantities[topping];
                                    var toppingGrid = FindVisualChild<Grid>(checkBox);
                                    if (toppingGrid != null)
                                    {
                                        toppingGrid.Children.Remove(quantityBox);
                                        toppingGrid.ColumnDefinitions.RemoveAt(toppingGrid.ColumnDefinitions.Count - 1);
                                    }
                                    toppingQuantities.Remove(topping);
                                }
                            };
                        }
                    }
                }
            };

            scrollViewer.Content = dialogContent;
            productDialog.Content = scrollViewer;

            productDialog.PrimaryButtonClick += async (s, args) =>
            {
                string selectedSugar = sugarOptions.SelectedItem?.ToString();
                string selectedIce = iceOptions.SelectedItem?.ToString();

                if (double.IsNaN(productQuantityBox.Value) || productQuantityBox.Value <= 0)
                {
                    args.Cancel = true;
                    productDialog.Hide();
                    await ShowErrorContentDialog(productDialog.XamlRoot, "Số lượng sản phẩm không được để trống.");
                    await productDialog.ShowAsync();
                    return;
                }

                if (selectedSugar == null || selectedIce == null)
                {
                    args.Cancel = true;
                    productDialog.Hide();
                    await ShowErrorContentDialog(productDialog.XamlRoot, "Vui lòng chọn mức đường và đá");
                    await productDialog.ShowAsync();
                    return;
                }

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

                var newItem = new InvoiceDetailModel
                {
                    ProductId = product.Id,
                    ProductVariant = selectedVariant,
                    SugarLevel = int.Parse(selectedSugar.Replace("%", "")),
                    IceLevel = int.Parse(selectedIce.Replace("%", "")),
                    Quantity = (int)productQuantityBox.Value,
                    Toppings = new List<OrderToppingModel>()
                };

                foreach (var topping in ViewModel.ToppingProducts)
                {
                    var container = toppings.ContainerFromItem(topping) as ContentPresenter;
                    if (container != null)
                    {
                        var checkBox = FindVisualChild<CheckBox>(container);
                        if (checkBox != null && checkBox.IsChecked == true)
                        {
                            int toppingQuantity = toppingQuantities.ContainsKey(topping) ? (int)toppingQuantities[topping].Value : 1;
                            newItem.Toppings.Add(new OrderToppingModel
                            {
                                ProductId = topping.Id,
                                ProductVariant = topping.ProductVariants[0],
                                Quantity = toppingQuantity
                            });
                        }
                    }
                }

                ViewModel.MapProductToVariants(newItem);

                ViewModel.OrderItems.Add(newItem);
                TotalItemsTextBlock.Text = ViewModel.TotalItems.ToString();
                TotalPriceTextBlock.Text = ConvertMoney(ViewModel.TotalPrice);

                float deliveryFee = 0f;
                if (DeliveryFee.IsEnabled)
                {
                    deliveryFee = (float)DeliveryFee.Value;
                }
                UpdateTotalAmount(ViewModel.TotalPrice, deliveryFee);
                Debug.WriteLine($"TotalItems: {ViewModel.TotalItems}, TotalPrice: {ViewModel.TotalPrice}");
                string variantInfo = selectedVariant != null ? $"{selectedVariant.Size} - {selectedVariant.Price}" : "Không chọn";
                Debug.WriteLine($"Product: {product.Name}, Variant: {variantInfo}, Quantity: {newItem.Quantity}, Sugar: {selectedSugar}, Ice: {selectedIce}, Toppings: {newItem.Toppings.Count}");
                foreach (var topping in newItem.Toppings)
                {
                    Debug.WriteLine($"Topping: {topping.ProductVariant.Product?.Name}, Quantity: {topping.Quantity}");
                }
            };

            await productDialog.ShowAsync();
        }
        private void DeleteInvoiceDetail_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is InvoiceDetailModel item)
            {
                ViewModel.OrderItems.Remove(item);
                TotalItemsTextBlock.Text = ViewModel.TotalItems.ToString();
                TotalPriceTextBlock.Text = ConvertMoney(ViewModel.TotalPrice);
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

        private void CustomerSearch_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                if (string.IsNullOrWhiteSpace(sender.Text))
                {
                    // Khi người dùng xóa ô tìm kiếm, đặt lại SelectedCustomer và trạng thái giao diện
                    ViewModel.CustomerViewModel.SelectedCustomer = null;
                    sender.ItemsSource = null; // Xóa danh sách gợi ý
                    checkBoxDelivery.IsChecked = false; // Bỏ check "Giao hàng"
                    ResetDeliveryState(); // Đặt lại trạng thái giao hàng
                }
                else
                {
                    var suggestions = ViewModel.CustomerViewModel.SearchCustomers(sender.Text);
                    sender.ItemsSource = suggestions;
                }
            }
        }

        private void CustomerSearch_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null)
            {
                var selectedCustomer = args.ChosenSuggestion as CustomerModel;
                ViewModel.CustomerViewModel.SelectedCustomer = selectedCustomer;
                sender.Text = $"{selectedCustomer.Name} - {selectedCustomer.Phone}";
            }
            else
            {
                var suggestions = ViewModel.CustomerViewModel.SearchCustomers(args.QueryText);
                if (suggestions.Any())
                {
                    var firstMatch = suggestions.First();
                    ViewModel.CustomerViewModel.SelectedCustomer = firstMatch;
                    sender.Text = $"{firstMatch.Name} - {firstMatch.Phone}";
                }
                else
                {
                    ViewModel.CustomerViewModel.SelectedCustomer = null;
                    sender.Text = string.Empty;
                    checkBoxDelivery.IsChecked = false; 
                    ResetDeliveryState();
                }
            }
        }

        private void CustomerSearch_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            var selectedCustomer = args.SelectedItem as CustomerModel;
            ViewModel.CustomerViewModel.SelectedCustomer = selectedCustomer;
            sender.Text = $"{selectedCustomer.Name} - {selectedCustomer.Phone}";
        }

        private void PrintInvoice_Click(object sender, RoutedEventArgs e)
        {
            var invoice = PrepareInvoice();
            Frame.Navigate(typeof(PrintInvoicePage), invoice);
        }

        private async void CheckoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.OrderItems == null || ViewModel.OrderItems.Count == 0)
            {
                await ShowErrorContentDialog(this.XamlRoot, "Không có sản phẩm nào để thanh toán.");
                return;
            }

            try
            {
                var newInvoice = PrepareInvoice();

                await SaveAndReset(newInvoice);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during checkout: {ex.Message}");
                await ShowErrorContentDialog(this.XamlRoot, "Đã xảy ra lỗi trong quá trình thanh toán: " + ex.Message);
            }
        }
        private InvoiceModel PrepareInvoice()
        {
            float deliveryFee = 0f;
            if (DeliveryFee.IsEnabled && !double.IsNaN(DeliveryFee.Value))
            {
                deliveryFee = (float)DeliveryFee.Value;
            }

            string paymentMethod = PaymentMethodDropDown.Content?.ToString() ?? "Tiền mặt";

            var newInvoice = new InvoiceModel
            {
                CustomerId = ViewModel.CustomerViewModel.SelectedCustomer?.Id,
                InvoiceDate = DateTime.Now,
                TotalAmount = ViewModel.TotalPrice + deliveryFee,
                DeliveryFee = deliveryFee,
                OrderType = DeliveryFee.IsEnabled ? "Giao hàng" : "Tại chỗ",
                PaymentMethod = paymentMethod,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                InvoiceDetails = new List<InvoiceDetailModel>()
            };

            foreach (var orderItem in ViewModel.OrderItems)
            {
                newInvoice.InvoiceDetails.Add(new InvoiceDetailModel
                {
                    ProductId = orderItem.ProductId,
                    SugarLevel = orderItem.SugarLevel,
                    IceLevel = orderItem.IceLevel,
                    Quantity = orderItem.Quantity,
                    ProductVariant = orderItem.ProductVariant,
                    Toppings = orderItem.Toppings
                });
            }

            return newInvoice;
        }
        private async Task SaveAndReset(InvoiceModel newInvoice)
        {
            await ViewModel.InvoiceViewModel.Add(newInvoice);

            ViewModel.OrderItems.Clear();

            TotalItemsTextBlock.Text = ViewModel.TotalItems.ToString();
            TotalPriceTextBlock.Text = ConvertMoney(ViewModel.TotalPrice);

            checkBoxDelivery.IsChecked = false;
            ResetDeliveryState();

            CustomerSearchBox.Text = string.Empty;
            ViewModel.CustomerViewModel.SelectedCustomer = null;

            PaymentMethodDropDown.Content = "Tiền mặt";

            await ShowSuccessContentDialog(this.XamlRoot, "Thanh toán thành công! Hóa đơn đã được tạo.");
        }

        private void PaymentMethodMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem selectedItem)
            {
                PaymentMethodDropDown.Content = selectedItem.Text;
            }
        }
        private async Task ShowSuccessContentDialog(XamlRoot xamlRoot, string message)
        {
            ContentDialog successDialog = new ContentDialog
            {
                Title = "Thành công",
                Content = message,
                CloseButtonText = "Đóng",
                XamlRoot = xamlRoot
            };
            await successDialog.ShowAsync();
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

        private string ConvertMoney(float value)
        {
            var converter = new MoneyFormatConverter();
            string formattedPrice = (string)converter.Convert(value, typeof(string), null, null);
            return formattedPrice;
        }

        private async void DeliveryCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            if (checkBox == null) return;

            if (ViewModel.CustomerViewModel.SelectedCustomer == null)
            {
                await ShowErrorContentDialog(this.XamlRoot, "Vui lòng chọn một khách hàng trước khi chọn giao hàng.");
                checkBox.IsChecked = false;
                return;
            }

            if (DeliveryFee != null)
            {
                DeliveryFee.IsEnabled = true;
            }

            if (AddressDisplay != null)
            {
                if (ViewModel.CustomerViewModel.SelectedCustomer != null &&
                    !string.IsNullOrWhiteSpace(ViewModel.CustomerViewModel.SelectedCustomer.Address))
                {
                    AddressDisplay.Visibility = Visibility.Visible;
                }
                else
                {
                    AddressDisplay.Visibility = Visibility.Collapsed;
                    Debug.WriteLine("No address available for the selected customer.");
                }
            }

            float deliveryFee = 0f;
            if (DeliveryFee.IsEnabled)
            {
                deliveryFee = (float)DeliveryFee.Value;
            }
            UpdateTotalAmount(ViewModel.TotalPrice, deliveryFee);
        }

        private void DeliveryCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ResetDeliveryState();
        }

        private void ResetDeliveryState()
        {
            if (DeliveryFee != null)
            {
                DeliveryFee.IsEnabled = false;
                DeliveryFee.Value = 0;
            }

            if (AddressDisplay != null)
            {
                AddressDisplay.Visibility = Visibility.Collapsed;
            }

            UpdateTotalAmount(ViewModel.TotalPrice, 0f);
        }

        private void DeliveryFee_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            float deliveryFee = (float)args.NewValue;
            UpdateTotalAmount(ViewModel.TotalPrice, deliveryFee);
        }

        private void UpdateTotalAmount(float totalPrice, float deliveryFee)
        {
            if (totalAmount != null)
            {
                totalAmount.Text = ConvertMoney(totalPrice + deliveryFee);
            }
            else
            {
                Debug.WriteLine("Error: totalAmount TextBlock is null.");
            }
        }
    }
}
