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
    }
}
