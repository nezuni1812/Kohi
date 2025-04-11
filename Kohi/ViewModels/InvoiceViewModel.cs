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
            try
            {
                int invoiceResult = _dao.Invoices.Insert(invoice);

                if (invoiceResult == 1)
                {
                    foreach (var detail in invoice.InvoiceDetails)
                    {
                        detail.InvoiceId = invoice.Id;
                        int detailResult = _dao.InvoiceDetails.Insert(detail);

                        if (detailResult != 1)
                        {
                            Debug.WriteLine($"Failed to insert InvoiceDetail for ProductId: {detail.ProductId}");
                            throw new Exception("Có lỗi khi lưu chi tiết hóa đơn.");
                        }
                    }

                    await LoadData(CurrentPage);
                }
                else
                {
                    Debug.WriteLine("Failed to insert Invoice.");
                    throw new Exception("Có lỗi khi tạo hóa đơn.");
                }
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
