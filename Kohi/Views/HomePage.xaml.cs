using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Kohi.Models;
using Kohi.Services;
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
using Newtonsoft.Json;
using RestSharp;
using Windows.Storage;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Kohi.Models.BankingAPI;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using System.Data;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using DocumentFormat.OpenXml.Bibliography;
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
        private readonly DistanceService _distanceService = new DistanceService();
        public class SalesData
        {
            [LoadColumn(0)]
            public string Product { get; set; }

            [LoadColumn(1)]
            public float DayOfWeek { get; set; }

            [LoadColumn(2)]
            public float Month { get; set; }

            [LoadColumn(3)]
            public float Temperature { get; set; }

            [LoadColumn(4)]
            public float SalesCount { get; set; } // This is the label (target)
        }

        public class SalesPrediction
        {
            [ColumnName("Score")]
            public float PredictedSales { get; set; }
        }

        static async Task<TransformerChain<RegressionPredictionTransformer<LinearRegressionModelParameters>>> Fitting(EstimatorChain<RegressionPredictionTransformer<LinearRegressionModelParameters>> trainingPipeline, IDataView trainingDataView)
        {
            return await Task.Run(() => trainingPipeline.Fit(trainingDataView));
        }

        async void actuallTrainAndPredict()
        {
            MLContext mlContext = new MLContext(seed: 0);

            if (mlContext != null)
            {
                Debug.WriteLine("Path: " + Environment.CurrentDirectory);
                IDataView trainingDataView = mlContext.Data.LoadFromTextFile<SalesData>(
                    path: "C:\\Users\\jiji\\source\\repos\\Kohi\\Kohi\\sales_train.csv",
                    hasHeader: true,
                    separatorChar: ',');


                Debug.WriteLine("Process pipeline");
                var dataProcessPipeline = mlContext.Transforms.CopyColumns("Label", nameof(SalesData.SalesCount))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("ProductEncoded", nameof(SalesData.Product)))
                .Append(mlContext.Transforms.Concatenate("Features", "ProductEncoded", nameof(SalesData.DayOfWeek),
                                                    nameof(SalesData.Month), nameof(SalesData.Temperature)))
                .Append(mlContext.Transforms.NormalizeMinMax("Features"));

                var trainer = mlContext.Regression.Trainers.Sdca(labelColumnName: "Label", featureColumnName: "Features");

                var trainingPipeline = dataProcessPipeline.Append(trainer);

                Debug.WriteLine("Train");

                var trainedModel = await Fitting(trainingPipeline, trainingDataView);

                Debug.WriteLine("Prediction model");
                var predictionEngine = mlContext.Model.CreatePredictionEngine<SalesData, SalesPrediction>(trainedModel);

                var newData = new SalesData
                {
                    Product = "Latte",
                    DayOfWeek = 5,
                    Month = 4,
                    Temperature = 18.5f
                };

                Debug.WriteLine("Predict");
                SalesPrediction prediction = predictionEngine.Predict(newData);
                Debug.WriteLine($"Predicted sales for Latte: {prediction.PredictedSales:F2}");
            }
        }
        private async void trainAndPredict(object sender, RoutedEventArgs e)
        {
            actuallTrainAndPredict();
            Debug.WriteLine("Called to train and predict");
        }

        public bool IsLoading { get; set; } = false;
        public HomePage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //ViewModel.AddProduct();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await LoadDataWithProgress();
        }
        private async Task LoadDataWithProgress()
        {
            try
            {
                IsLoading = true;
                ProgressRing.IsActive = true;

                await Task.WhenAll(
                    ViewModel.ProductViewModel.LoadData(),
                    ViewModel.CategoryViewModel.LoadData()
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error reloading customers: {ex.Message}");
                await ShowErrorContentDialog(this.XamlRoot, $"Lỗi khi tải dữ liệu: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                ProgressRing.IsActive = false;
            }
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
                float deliveryFee = 0f;
                if (DeliveryFee.IsEnabled)
                {
                    deliveryFee = (float)DeliveryFee.Value;
                }
                UpdateTotalAmount(ViewModel.TotalPrice, deliveryFee);
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

        private async void CustomerSearch_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                if (string.IsNullOrWhiteSpace(sender.Text))
                {
                    // Khi người dùng xóa ô tìm kiếm, đặt lại SelectedCustomer và trạng thái giao diện
                    ViewModel.CustomerViewModel.SelectedCustomer = null;
                    sender.ItemsSource = null;
                    checkBoxDelivery.IsChecked = false; 
                    ResetDeliveryState(); 
                }
                else
                {
                    await ViewModel.CustomerViewModel.LoadData();
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

        private async void PrintInvoice_Click(object sender, RoutedEventArgs e)
        {
            var userPaymentSettings = RestoreUserPaymentSettings();
            if (userPaymentSettings == null)
            {
                await ShowErrorContentDialog(this.XamlRoot, "Chưa có thông tin chuyển khoản. Hãy vào trang Thiết lập để thêm thông tin");
                return;
            }
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
                // Lấy phương thức thanh toán từ DropDownButton
                //string paymentMethod = PaymentMethodDropDown.Content?.ToString() ?? "Tiền mặt";

                // Tạo mã QR nếu phương thức thanh toán không phải "Tiền mặt"
                /*if (paymentMethod != "Tiền mặt")
                {
                    float deliveryFee = 0f;
                    if (DeliveryFee.IsEnabled && !double.IsNaN(DeliveryFee.Value))
                    {
                        deliveryFee = (float)DeliveryFee.Value;
                    }
                    int totalAmount = (int)(ViewModel.TotalPrice + deliveryFee);
                    var qrImage = await GenerateQRCodeAsync(totalAmount);
                    if (qrImage != null)
                    {
                        await ShowQRCodeDialogAsync(qrImage, totalAmount);
                    }
                }*/
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
            UpdateTotalAmount(0, 0);
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

            string shopAddress = RestoreAddress();
            if (shopAddress == string.Empty)
            {
                await ShowErrorContentDialog(this.XamlRoot, "Chưa có địa chỉ của quán. Hãy vào trang Thiết lập để thêm thông tin");
                return;
            }

            float deliveryFee = 0f;
            if (DeliveryFee.IsEnabled)
            {
                deliveryFee = (float)DeliveryFee.Value;
            }

            string customerAddress = CustomerAddressTextBlock.Text;
            Debug.WriteLine($"Địa chỉ khách hàng: {CustomerAddressTextBlock.Text}");
            Debug.WriteLine($"Địa chỉ cửa hàng: {shopAddress}");
            try
            {
                double distance = await _distanceService.CalculateDistanceAsync(customerAddress, shopAddress);
                DeliveryDistance.Text = distance.ToString("F2"); // Hiển thị khoảng cách với 2 chữ số thập phân
                Debug.WriteLine(distance);
                UpdateTotalAmount(ViewModel.TotalPrice, deliveryFee);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi tính khoảng cách: {ex.Message}");
                await ShowErrorContentDialog(this.XamlRoot, "Không thể tính khoảng cách. Vui lòng kiểm tra lại địa chỉ.");
                checkBox.IsChecked = false;
                ResetDeliveryState();
            }
        }

        private string RestoreAddress()
        {
            var settings = ApplicationData.Current.LocalSettings;
            if (settings.Values.ContainsKey("UserPayment"))
            {
                try
                {
                    string json = settings.Values["UserPayment"]?.ToString();
                    if (string.IsNullOrEmpty(json))
                    {
                        Debug.WriteLine("Lỗi: Dữ liệu UserPayment trong LocalSettings rỗng.");
                        return string.Empty; // Hoặc trả về địa chỉ mặc định của cửa hàng
                    }

                    var saved = JsonConvert.DeserializeObject<UserPaymentSettings>(json);
                    if (saved == null || string.IsNullOrWhiteSpace(saved.Address))
                    {
                        Debug.WriteLine("Lỗi: Địa chỉ trong UserPayment không hợp lệ hoặc rỗng.");
                        return string.Empty; // Hoặc trả về địa chỉ mặc định
                    }

                    Debug.WriteLine($"Địa chỉ cửa hàng lấy được: {saved.Address}");
                    return saved.Address;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Lỗi khôi phục địa chỉ từ LocalSettings: {ex.Message}");
                    return string.Empty; // Hoặc trả về địa chỉ mặc định
                }
            }

            Debug.WriteLine("Lỗi: Không tìm thấy key UserPayment trong LocalSettings.");
            return string.Empty; // Hoặc trả về địa chỉ mặc định
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
        /*private async Task<BitmapImage> GenerateQRCodeAsync(int amount)
        {
            try
            {
                // Lấy thông tin từ LocalSettings
                var userPaymentSettings = RestoreUserPaymentSettings();
                if (userPaymentSettings == null)
                {
                    throw new Exception("Không tìm thấy thông tin tài khoản trong LocalSettings.");
                }

                // Tạo request cho API VietQR
                var apiRequest = new ApiBankingRequestModel
                {
                    acqId = Convert.ToInt32(userPaymentSettings.BankBin),
                    accountNo = long.Parse(userPaymentSettings.AccountNo),
                    accountName = userPaymentSettings.AccountName,
                    amount = amount,
                    format = "text",
                    template = userPaymentSettings.Template ?? "print"
                };

                var jsonRequest = JsonConvert.SerializeObject(apiRequest);
                var client = new RestClient("https://api.vietqr.io/v2/generate");
                var request = new RestRequest();

                request.Method = Method.Post;
                request.AddHeader("Accept", "application/json");
                request.AddParameter("application/json", jsonRequest, ParameterType.RequestBody);

                var response = await client.ExecuteAsync(request);
                if (response.IsSuccessful)
                {
                    var content = response.Content;
                    var dataResult = JsonConvert.DeserializeObject<ApiBankingResponseModel>(content);
                    return await Base64ToImageAsync(dataResult.data.qrDataURL.Replace("data:image/png;base64,", ""));
                }
                else
                {
                    throw new Exception("Lỗi khi gọi API VietQR: " + response.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                await ShowErrorContentDialog(this.XamlRoot, $"Lỗi tạo mã QR: {ex.Message}");
                return null;
            }
        }
        private async Task<BitmapImage> Base64ToImageAsync(string base64String)
        {
            try
            {
                if (string.IsNullOrEmpty(base64String))
                {
                    throw new Exception("Chuỗi Base64 không hợp lệ");
                }

                byte[] imageBytes = Convert.FromBase64String(base64String);
                BitmapImage image = new BitmapImage();

                using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
                {
                    using (DataWriter writer = new DataWriter(stream.GetOutputStreamAt(0)))
                    {
                        writer.WriteBytes(imageBytes);
                        await writer.StoreAsync();
                    }

                    stream.Seek(0);
                    await image.SetSourceAsync(stream);
                }

                return image;
            }
            catch (FormatException ex)
            {
                throw new Exception("Lỗi: Dữ liệu Base64 không hợp lệ", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi chuyển đổi Base64 thành hình ảnh: {ex.Message}", ex);
            }
        }*/
        private UserPaymentSettings RestoreUserPaymentSettings()
        {
            var settings = ApplicationData.Current.LocalSettings;
            if (settings.Values.ContainsKey("UserPayment"))
            {
                try
                {
                    string json = settings.Values["UserPayment"]?.ToString();
                    return JsonConvert.DeserializeObject<UserPaymentSettings>(json);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Lỗi khôi phục dữ liệu: {ex.Message}");
                    return null;
                }
            }
            return null;
        }
        /*private async Task ShowQRCodeDialogAsync(BitmapImage qrImage, int amount)
        {
            ContentDialog qrDialog = new ContentDialog
            {
                Title = "Mã QR Thanh Toán",
                Content = new StackPanel
                {
                    Children =
            {
                new TextBlock
                {
                    Text = $"Số tiền: {ConvertMoney(amount)}",
                    Margin = new Thickness(0, 0, 0, 10)
                },
                new Image
                {
                    Source = qrImage,
                    Width = 200,
                    Height = 200,
                    Stretch = Stretch.Uniform
                }
            }
                },
                CloseButtonText = "Đóng",
                XamlRoot = this.XamlRoot
            };

            await qrDialog.ShowAsync();
        }*/
    }
}
