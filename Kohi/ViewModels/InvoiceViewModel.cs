using Kohi.Models;
using Kohi.Services;
using Kohi.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.ViewModels
{
    public class InvoiceViewModel
    {
        private IDao _dao;
        public FullObservableCollection<InvoiceModel> Invoices { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize); // Tổng số trang
        public InvoiceViewModel()
        {
            _dao = Service.GetKeyedSingleton<IDao>();
            Invoices = new FullObservableCollection<InvoiceModel>();

            LoadData();
        }

        public async Task LoadData(int page = 1)
        {
            CurrentPage = page;
            TotalItems = _dao.Invoices.GetCount();
            var result = await Task.Run(() => _dao.Invoices.GetAll(
                pageNumber: CurrentPage,
                pageSize: PageSize
            ));

            // Lấy tất cả chi tiết hóa đơn từ API
            var allInvoiceDetails = await Task.Run(() => _dao.InvoiceDetails.GetAll(
                pageNumber: 1,
                pageSize: 1000 // Giả sử lấy số lượng lớn để bao quát
            ));

            Invoices.Clear();
            foreach (var item in result)
            {
                // Lọc chi tiết hóa đơn theo InvoiceId và gán vào InvoiceDetails
                var detailsForInvoice = allInvoiceDetails.Where(d => d.InvoiceId == item.Id).ToList();
                item.InvoiceDetails.Clear(); // Xóa danh sách cũ (nếu có)
                foreach (var detail in detailsForInvoice)
                {
                    item.InvoiceDetails.Add(detail);
                }
                Debug.WriteLine($"Invoice {item.Id} has {item.InvoiceDetails.Count} details");

                Invoices.Add(item);
            }
        }

        public async Task Add(InvoiceModel invoice)
        {
            if (invoice == null)
            {
                Debug.WriteLine("Add failed: Invoice is null.");
                throw new ArgumentNullException(nameof(invoice), "Hóa đơn không được null.");
            }

            if (invoice.InvoiceDetails == null || !invoice.InvoiceDetails.Any())
            {
                Debug.WriteLine("Add failed: InvoiceDetails is null or empty.");
                throw new ArgumentException("Hóa đơn phải có ít nhất một chi tiết.", nameof(invoice.InvoiceDetails));
            }

            try
            {
                int invoiceId = _dao.Invoices.Insert(invoice);
                if (invoiceId <= 0)
                {
                    Debug.WriteLine("Add failed: Could not insert invoice.");
                    throw new Exception("Không thể tạo hóa đơn. Vui lòng thử lại.");
                }

                foreach (var detail in invoice.InvoiceDetails)
                {
                    detail.InvoiceId = invoiceId;
                    int detailResult = _dao.InvoiceDetails.Insert(detail);
                    if (detailResult <= 0)
                    {
                        Debug.WriteLine($"Add failed: Could not insert InvoiceDetail for ProductId: {detail.ProductId}.");
                        throw new Exception($"Lỗi khi lưu chi tiết hóa đơn cho sản phẩm {detail.ProductId}.");
                    }

                    if (detail.Toppings != null && detail.Toppings.Any())
                    {
                        foreach (var topping in detail.Toppings)
                        {
                            topping.InvoiceDetailId = detailResult; // Gán InvoiceDetailId
                            int toppingId = _dao.OrderToppings.Insert(topping);
                            if (toppingId <= 0)
                            {
                                Debug.WriteLine($"Add failed: Could not insert OrderTopping for ProductId: {topping.ProductId}.");
                                throw new Exception($"Lỗi khi lưu topping cho chi tiết hóa đơn {detail.Id}.");
                            }
                            topping.Id = toppingId;
                        }
                    }
                }

                // Tải lại dữ liệu sau khi lưu thành công
                await LoadData(CurrentPage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding invoice: {ex.Message}");
                throw;
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
        public async Task<List<InvoiceModel>> GetAll()
        {
            try
            {
                var Invoices = _dao.Invoices.GetAll(1, 1000); // Đồng bộ
                return Invoices;
            }
            catch (Exception ex)
            {
                return null; // Trả về null khi có lỗi
                // Xử lý lỗi (tùy chọn)
            }
        }

        public async Task<List<InvoiceModel>> GetAllWithDetails()
        {
            try
            {
                var invoices = _dao.Invoices.GetAll(1, 1000); // Đồng bộ
                if (invoices == null || !invoices.Any())
                {
                    Debug.WriteLine("GetAll: No invoices found.");
                    return new List<InvoiceModel>();
                }
                Debug.WriteLine($"GetAll: Loaded {invoices.Count} invoices");

                var allInvoiceDetails = _dao.InvoiceDetails.GetAll(1, 1000); // Đồng bộ
                if (allInvoiceDetails == null || !allInvoiceDetails.Any())
                {
                    Debug.WriteLine("GetAll: No invoice details found.");
                }
                else
                {
                    Debug.WriteLine($"GetAll: Loaded {allInvoiceDetails.Count} invoice details");
                }

                var allOrderToppings = _dao.OrderToppings.GetAll(1, 1000); // Đồng bộ
                if (allOrderToppings == null || !allOrderToppings.Any())
                {
                    Debug.WriteLine("GetAll: No order toppings found.");
                }
                else
                {
                    Debug.WriteLine($"GetAll: Loaded {allOrderToppings.Count} order toppings");
                }

                var allProductVariants = _dao.ProductVariants.GetAll(1, 1000); // Đồng bộ
                if (allProductVariants == null || !allProductVariants.Any())
                {
                    Debug.WriteLine("GetAll: No product variants found.");
                }
                else
                {
                    Debug.WriteLine($"GetAll: Loaded {allProductVariants.Count} product variants");
                }

                var allProducts = _dao.Products.GetAll(1, 1000); // Đồng bộ
                if (allProducts == null || !allProducts.Any())
                {
                    Debug.WriteLine("GetAll: No products found.");
                }
                else
                {
                    Debug.WriteLine($"GetAll: Loaded {allProducts.Count} products");
                }

                foreach (var invoice in invoices)
                {
                    var detailsForInvoice = allInvoiceDetails?.Where(d => d.InvoiceId == invoice.Id).ToList() ?? new List<InvoiceDetailModel>();
                    invoice.InvoiceDetails = detailsForInvoice;

                    foreach (var detail in invoice.InvoiceDetails)
                    {
                        detail.ProductVariant = allProductVariants?.FirstOrDefault(pv => pv.Id == detail.ProductId);
                        if (detail.ProductVariant == null)
                        {
                            Debug.WriteLine($"InvoiceDetail {detail.Id} has no matching ProductVariant for ProductId {detail.ProductId}");
                        }
                        else
                        {
                            detail.ProductVariant.Product = allProducts?.FirstOrDefault(p => p.Id == detail.ProductVariant.ProductId);
                            if (detail.ProductVariant.Product == null)
                            {
                                Debug.WriteLine($"ProductVariant {detail.ProductVariant.Id} has no matching Product for ProductId {detail.ProductVariant.ProductId}");
                            }
                        }

                        detail.Toppings = allOrderToppings?.Where(t => t.InvoiceDetailId == detail.Id).ToList() ?? new List<OrderToppingModel>();
                        foreach (var topping in detail.Toppings)
                        {
                            topping.ProductVariant = allProductVariants?.FirstOrDefault(pv => pv.Id == topping.ProductId);
                            if (topping.ProductVariant == null)
                            {
                                Debug.WriteLine($"OrderTopping {topping.Id} has no matching ProductVariant for ProductId {topping.ProductId}");
                            }
                            else
                            {
                                topping.ProductVariant.Product = allProducts?.FirstOrDefault(p => p.Id == topping.ProductVariant.ProductId);
                                if (topping.ProductVariant.Product == null)
                                {
                                    Debug.WriteLine($"ProductVariant {topping.ProductVariant.Id} has no matching Product for ProductId {topping.ProductVariant.ProductId}");
                                }
                            }
                        }

                        Debug.WriteLine($"InvoiceDetail {detail.Id} has {detail.Toppings.Count} toppings");
                    }

                    Debug.WriteLine($"Invoice {invoice.Id} has {invoice.InvoiceDetails.Count} details");
                }

                return invoices;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetAll: {ex.Message}");
                return new List<InvoiceModel>();
            }
        }

        public async Task<InvoiceModel> GetDetailsById(int invoiceId)
        {
            try
            {
                var invoice = _dao.Invoices.GetById(invoiceId.ToString());
                if (invoice == null)
                {
                    Debug.WriteLine($"GetDetailsById: No invoice found for Id {invoiceId}.");
                    return null;
                }
                Debug.WriteLine($"GetDetailsById: Found invoice {invoice.Id}");

                var allInvoiceDetails = await Task.Run(() => _dao.InvoiceDetails.GetAll(
                    pageNumber: 1,
                    pageSize: 1000
                ));
                if (allInvoiceDetails == null || !allInvoiceDetails.Any())
                {
                    Debug.WriteLine($"GetDetailsById: No invoice details found.");
                    invoice.InvoiceDetails = new List<InvoiceDetailModel>();
                    return invoice;
                }

                var allOrderToppings = await Task.Run(() => _dao.OrderToppings.GetAll(
                    pageNumber: 1,
                    pageSize: 1000
                ));
                if (allOrderToppings == null || !allOrderToppings.Any())
                {
                    Debug.WriteLine($"GetDetailsById: No order toppings found.");
                }
                else
                {
                    Debug.WriteLine($"GetDetailsById: Loaded {allOrderToppings.Count} order toppings");
                }

                var allProductVariants = await Task.Run(() => _dao.ProductVariants.GetAll(
                    pageNumber: 1,
                    pageSize: 1000
                ));
                if (allProductVariants == null || !allProductVariants.Any())
                {
                    Debug.WriteLine($"GetDetailsById: No product variants found.");
                }
                else
                {
                    Debug.WriteLine($"GetDetailsById: Loaded {allProductVariants.Count} product variants");
                }

                var allProducts = await Task.Run(() => _dao.Products.GetAll(
                    pageNumber: 1,
                    pageSize: 1000
                ));
                if (allProducts == null || !allProducts.Any())
                {
                    Debug.WriteLine($"GetDetailsById: No products found.");
                }
                else
                {
                    Debug.WriteLine($"GetDetailsById: Loaded {allProducts.Count} products");
                }

                var detailsForInvoice = allInvoiceDetails.Where(d => d.InvoiceId == invoice.Id).ToList();
                invoice.InvoiceDetails = detailsForInvoice;
                Debug.WriteLine($"GetDetailsById: Invoice {invoice.Id} has {invoice.InvoiceDetails.Count} details");

                foreach (var detail in invoice.InvoiceDetails)
                {
                    detail.ProductVariant = allProductVariants?.FirstOrDefault(pv => pv.Id == detail.ProductId);
                    if (detail.ProductVariant == null)
                    {
                        Debug.WriteLine($"GetDetailsById: InvoiceDetail {detail.Id} has no matching ProductVariant for ProductId {detail.ProductId}");
                    }
                    else
                    {
                        detail.ProductVariant.Product = allProducts?.FirstOrDefault(p => p.Id == detail.ProductVariant.ProductId);
                        if (detail.ProductVariant.Product == null)
                        {
                            Debug.WriteLine($"GetDetailsById: ProductVariant {detail.ProductVariant.Id} has no matching Product for ProductId {detail.ProductVariant.ProductId}");
                        }
                    }

                    detail.Toppings = allOrderToppings?.Where(t => t.InvoiceDetailId == detail.Id).ToList() ?? new List<OrderToppingModel>();
                    foreach (var topping in detail.Toppings)
                    {
                        topping.ProductVariant = allProductVariants?.FirstOrDefault(pv => pv.Id == topping.ProductId);
                        if (topping.ProductVariant == null)
                        {
                            Debug.WriteLine($"GetDetailsById: OrderTopping {topping.Id} has no matching ProductVariant for ProductId {topping.ProductId}");
                        }
                        else
                        {
                            topping.ProductVariant.Product = allProducts?.FirstOrDefault(p => p.Id == topping.ProductVariant.ProductId);
                            if (topping.ProductVariant.Product == null)
                            {
                                Debug.WriteLine($"GetDetailsById: ProductVariant {topping.ProductVariant.Id} has no matching Product for ProductId {topping.ProductVariant.ProductId}");
                            }
                        }
                    }

                    Debug.WriteLine($"GetDetailsById: InvoiceDetail {detail.Id} has {detail.Toppings.Count} toppings");
                }

                return invoice;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetDetailsById for InvoiceId {invoiceId}: {ex.Message}");
                return null;
            }
        }
    }
}
