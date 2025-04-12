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
            TotalAmountTextBlock.Text = $"Tổng tiền: {ConvertMoney(Invoice.TotalAmount - Invoice.DeliveryFee)}";
            PaymentMethodTextBlock.Text = $"{Invoice.PaymentMethod}: {ConvertMoney(Invoice.TotalAmount)}";

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
        }

        private string ConvertMoney(float value)
        {
            var converter = new MoneyFormatConverter();
            return (string)converter.Convert(value, typeof(string), null, null);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(HomePage));
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