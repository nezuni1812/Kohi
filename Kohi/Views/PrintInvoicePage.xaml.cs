using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Kohi.Models;
using Kohi.ViewModels;
using Kohi.Views.Converter;
using System.Diagnostics;
using Microsoft.UI.Xaml.Printing;
using Windows.Graphics.Printing;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Windowing;
using Kohi.Models.BankingAPI;
using Microsoft.UI.Xaml.Media.Imaging;
using Newtonsoft.Json;
using RestSharp;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.Storage;

namespace Kohi.Views
{
    public sealed partial class PrintInvoicePage : Page
    {
        public ProductViewModel ProductViewModel { get; set; } = new ProductViewModel(1000);
        public OrderToppingViewModel OrderToppingViewModel { get; set; } = new OrderToppingViewModel();
        public InvoiceDetailViewModel InvoiceDetailViewModel { get; set; } = new InvoiceDetailViewModel();
        public CustomerViewModel CustomerViewModel { get; set; } = new CustomerViewModel();
        public InvoiceViewModel InvoiceViewModel { get; set; } = new InvoiceViewModel();
        public InvoiceModel Invoice { get; set; }
        public List<InvoiceDetailDisplayModel> InvoiceDetailDisplays { get; set; }
        private PrintManager printMan;
        private PrintDocument printDoc;
        private IPrintDocumentSource printDocSource;

        public PrintInvoicePage()
        {
            this.InitializeComponent();
            Invoice = new InvoiceModel();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            RegisterPrint();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is InvoiceModel invoice)
            {
                Invoice = invoice;
                Debug.WriteLine($"Received Invoice: Id={Invoice.Id}, TotalAmount={Invoice.TotalAmount}");
                UpdateInvoiceDisplay();
            }
            else
            {
                Debug.WriteLine("Error: No Invoice provided for printing.");
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            UnregisterPrint(); 
        }
        private void UpdateInvoiceDisplay()
        {
            if (Invoice == null)
            {
                Debug.WriteLine("Invoice is null in UpdateInvoiceDisplay.");
                return;
            }

            InvoiceDateTextBlock.Text = $"Ngày tạo: {Invoice.InvoiceDate:yyyy-MM-dd HH:mm}";
            if (Invoice.OrderType == "Giao hàng")
            {
                DeliveryFee.Visibility = Visibility.Visible;
                DeliveryFee.Text = $"Phí giao hàng: {ConvertMoney(Invoice.DeliveryFee)}";
                OrderType.Text = "GIAO HÀNG";
            }
            else
            {
                DeliveryFee.Visibility = Visibility.Collapsed;
                OrderType.Text = "TẠI CHỖ";
            }
            TotalAmountTextBlock.Text = $"Tổng tiền hàng: {ConvertMoney(Invoice.TotalAmount - Invoice.DeliveryFee)}";
            PaymentMethodTextBlock.Text = $"Thanh toán: {Invoice.PaymentMethod}";

            if (Invoice.CustomerId > 0)
            {
                Customer.Visibility = Visibility.Visible;

                CustomerModel customer = CustomerViewModel.GetById(Invoice.CustomerId.ToString());
                if (customer != null)
                {
                    CustomerName.Text = customer.Name ?? "Không có tên";
                    CustomerPhone.Text = customer.Phone ?? "Không có số";
                    if (Invoice.OrderType == "Giao hàng")
                    {
                        DeliveryAddress.Visibility = Visibility.Visible;
                        CustomerAddress.Text = customer.Address;
                    }
                }
                else
                {
                    CustomerName.Text = "Không xác định";
                    CustomerPhone.Text = "Không có số";
                }
            }
            else
            {
                DeliveryAddress.Visibility = Visibility.Collapsed;
                Customer.Visibility = Visibility.Collapsed;
            }

            if (Invoice.InvoiceDetails == null || !Invoice.InvoiceDetails.Any())
            {
                Debug.WriteLine("InvoiceDetails is null or empty.");
                InvoiceDetailDisplays = new List<InvoiceDetailDisplayModel>();
                InvoiceDetailsItemsControl.ItemsSource = InvoiceDetailDisplays;
                return;
            }

            InvoiceDetailDisplays = new List<InvoiceDetailDisplayModel>();
            foreach (var detail in Invoice.InvoiceDetails)
            {
                if (detail == null || detail.ProductVariant == null || detail.ProductVariant.Product == null)
                {
                    Debug.WriteLine($"Invalid InvoiceDetail: {detail?.ProductId}");
                    continue;
                }
                var displayModel = new InvoiceDetailDisplayModel(detail);
                InvoiceDetailDisplays.Add(displayModel);
                Debug.WriteLine($"InvoiceDetail: Product={detail.ProductVariant.Product.Name}, TotalPrice={displayModel.TotalPrice}");
                if (detail.Toppings != null && detail.Toppings.Any())
                {
                    foreach (var topping in detail.Toppings)
                    {
                        Debug.WriteLine($"Topping: Product={topping.ProductVariant?.Product?.Name}, Quantity={topping.Quantity}, TotalPrice={topping.Quantity * (topping.ProductVariant?.Price ?? 0)}");
                    }
                }
            }

            InvoiceDetailsItemsControl.ItemsSource = InvoiceDetailDisplays;
            DisplayQRCode();
        }

        private async void DisplayQRCode()
        {
            string paymentMethod = Invoice.PaymentMethod;

            // Tạo mã QR nếu phương thức thanh toán không phải "Tiền mặt"
            if (paymentMethod != "Tiền mặt")
            {
                int totalAmount = (int)Invoice.TotalAmount;
                var qrImage = await GenerateQRCodeAsync(totalAmount);
                qrPicture.Source = qrImage;
            }
        }

        private async Task<BitmapImage> GenerateQRCodeAsync(int amount)
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
        }
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
            return (string)converter.Convert(value, typeof(string), null, null);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack(); 
            }
            else
            {
                Frame.Navigate(typeof(HomePage));
            }
        }

        private void RegisterPrint()
        {
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);

            printMan = PrintManagerInterop.GetForWindow(hWnd);
            printMan.PrintTaskRequested += PrintTaskRequested;

            printDoc = new PrintDocument();
            printDocSource = printDoc.DocumentSource;
            printDoc.Paginate += Paginate;
            printDoc.GetPreviewPage += GetPreviewPage;
            printDoc.AddPages += AddPages;
        }

        private void UnregisterPrint()
        {
            if (printDoc == null)
            {
                return;
            }
            printDoc.Paginate -= Paginate;
            printDoc.GetPreviewPage -= GetPreviewPage;
            printDoc.AddPages -= AddPages;
            printDoc = null;
            printDocSource = null;

            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            PrintManager printManager = PrintManagerInterop.GetForWindow(hWnd);
            printManager.PrintTaskRequested -= PrintTaskRequested;
        }

        private async void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            if (PrintManager.IsSupported())
            {
                try
                {
                    var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
                    await PrintManagerInterop.ShowPrintUIForWindowAsync(hWnd);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Printing error: {ex.Message}");
                    ContentDialog noPrintingDialog = new ContentDialog
                    {
                        XamlRoot = this.XamlRoot,
                        Title = "Printing error",
                        Content = "\nSorry, printing can't proceed at this time.",
                        PrimaryButtonText = "OK"
                    };
                    await noPrintingDialog.ShowAsync();
                }
            }
            else
            {
                ContentDialog noPrintingDialog = new ContentDialog
                {
                    XamlRoot = this.XamlRoot,
                    Title = "Printing not supported",
                    Content = "\nSorry, printing is not supported on this device.",
                    PrimaryButtonText = "OK"
                };
                await noPrintingDialog.ShowAsync();
            }
        }

        private void PrintTaskRequested(PrintManager sender, PrintTaskRequestedEventArgs args)
        {
            var printTask = args.Request.CreatePrintTask("Print Invoice", PrintTaskSourceRequested);
            printTask.Completed += PrintTaskCompleted;
        }

        private void PrintTaskSourceRequested(PrintTaskSourceRequestedArgs args)
        {
            args.SetSource(printDocSource);
        }

        private void Paginate(object sender, PaginateEventArgs e)
        {
            printDoc.SetPreviewPageCount(1, PreviewPageCountType.Final);
        }

        private void GetPreviewPage(object sender, GetPreviewPageEventArgs e)
        {
            printDoc.SetPreviewPage(e.PageNumber, ContentToPrint);
        }

        private void AddPages(object sender, AddPagesEventArgs e)
        {
            printDoc.AddPage(ContentToPrint);
            printDoc.AddPagesComplete();
        }

        private void PrintTaskCompleted(PrintTask sender, PrintTaskCompletedEventArgs args)
        {
            if (args.Completion == PrintTaskCompletion.Failed)
            {
                DispatcherQueue.TryEnqueue(async () =>
                {
                    ContentDialog noPrintingDialog = new ContentDialog
                    {
                        XamlRoot = this.XamlRoot,
                        Title = "Printing error",
                        Content = "\nSorry, failed to print.",
                        PrimaryButtonText = "OK"
                    };
                    await noPrintingDialog.ShowAsync();
                });
            }
        }
    }

    public class InvoiceDetailDisplayModel
    {
        public InvoiceDetailModel Detail { get; set; }
        public float TotalPrice => CalculateTotalPrice();

        public InvoiceDetailDisplayModel(InvoiceDetailModel detail)
        {
            Detail = detail;
        }

        private float CalculateTotalPrice()
        {
            if (Detail == null || Detail.ProductVariant == null)
            {
                Debug.WriteLine("Detail or ProductVariant is null in CalculateTotalPrice.");
                return 0;
            }

            float productPrice = Detail.ProductVariant.Price * Detail.Quantity;
            float toppingsPrice = Detail.Toppings?.Sum(t => t.ProductVariant?.Price * t.Quantity ?? 0) ?? 0;
            return productPrice + toppingsPrice;
        }
    }
}