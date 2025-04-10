using Kohi.Models;
using Kohi.Services;
using Kohi.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Kohi.ViewModels
{
    public class InvoiceDetailViewModel
    {
        private IDao _dao;
        public FullObservableCollection<InvoiceDetailModel> InvoiceDetails { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize); // Tổng số trang
        public InvoiceDetailViewModel()
        {
            _dao = Service.GetKeyedSingleton<IDao>();
            InvoiceDetails = new FullObservableCollection<InvoiceDetailModel>();
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            await LoadData();
        }

        public async Task LoadData(int page = 1)
        {
            try
            {
                // Debug.WriteLine("Starting LoadData in InvoiceDetailViewModel...");
                CurrentPage = page;
                TotalItems = _dao.InvoiceDetails.GetCount();
                // Debug.WriteLine($"TotalItems: {TotalItems}");

                var result = await Task.Run(() => _dao.InvoiceDetails.GetAll(
                    pageNumber: CurrentPage,
                    pageSize: PageSize
                ));
                // Debug.WriteLine($"Loaded {result?.Count ?? 0} invoice details from DAO");

                var allInvoices = await Task.Run(() => _dao.Invoices.GetAll(
                    pageNumber: 1,
                    pageSize: 1000
                ));
                // Debug.WriteLine($"Loaded {allInvoices?.Count ?? 0} invoices from DAO");

                var allProductVariants = await Task.Run(() => _dao.ProductVariants.GetAll(
                    pageNumber: 1,
                    pageSize: 1000
                ));
                // Debug.WriteLine($"Loaded {allProductVariants?.Count ?? 0} product variants from DAO");

                var allProducts = await Task.Run(() => _dao.Products.GetAll(
                    pageNumber: 1,
                    pageSize: 1000
                ));
                // Debug.WriteLine($"Loaded {allProducts?.Count ?? 0} products from DAO");

                var allToppings = await Task.Run(() => _dao.OrderToppings.GetAll(
                    pageNumber: 1,
                    pageSize: 1000
                ));
                // Debug.WriteLine($"Loaded {allToppings?.Count ?? 0} toppings from DAO");

                InvoiceDetails.Clear();
                if (result != null)
                {
                    foreach (var item in result)
                    {
                        item.Invoice = allInvoices.FirstOrDefault(i => i.Id == item.InvoiceId);
                        // Debug.WriteLine($"InvoiceDetail {item.Id} mapped to Invoice {item.Invoice?.Id ?? -1}");

                        item.ProductVariant = allProductVariants.FirstOrDefault(p => p.Id == item.ProductId);
                        // Debug.WriteLine($"InvoiceDetail {item.Id} mapped to ProductVariant {item.ProductVariant?.Id ?? -1}");

                        if (item.ProductVariant != null)
                        {
                            item.ProductVariant.Product = allProducts.FirstOrDefault(p => p.Id == item.ProductVariant.ProductId);
                            // Debug.WriteLine($"ProductVariant {item.ProductVariant.Id} mapped to Product {item.ProductVariant.Product?.Id ?? -1}");
                        }

                        var toppingsForInvoiceDetail = allToppings.Where(t => t.InvoiceDetailId == item.Id).ToList();
                        item.Toppings.Clear();
                        foreach (var topping in toppingsForInvoiceDetail)
                        {
                            item.Toppings.Add(topping);
                        }
                        // Debug.WriteLine($"InvoiceDetail {item.Id} has {item.Toppings.Count} toppings");

                        InvoiceDetails.Add(item);
                    }
                }
                else
                {
                    Debug.WriteLine("Result is null, no invoice details to process.");
                }
                Debug.WriteLine("LoadData completed.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in LoadData: {ex.Message}");
            }
        }

        // Phương thức để chuyển đến trang tiếp theo
        public async Task NextPage()
        {
            if (CurrentPage < TotalPages)
            {
                await LoadData(CurrentPage + 1);
            }
        }

        // Phương thức để quay lại trang trước
        public async Task PreviousPage()
        {
            if (CurrentPage > 1)
            {
                await LoadData(CurrentPage - 1);
            }
        }

        // Phương thức để chuyển đến trang cụ thể
        public async Task GoToPage(int page)
        {
            if (page >= 1 && page <= TotalPages)
            {
                await LoadData(page);
            }
        }
    }
}