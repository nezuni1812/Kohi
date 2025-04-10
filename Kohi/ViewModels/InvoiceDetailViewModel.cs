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

            LoadData();
        }

        public async Task LoadData(int page = 1)
        {
            CurrentPage = page;
            TotalItems = _dao.InvoiceDetails.GetCount();
            var result = await Task.Run(() => _dao.InvoiceDetails.GetAll(
                pageNumber: CurrentPage,
                pageSize: PageSize
            ));

            // Lấy tất cả topping từ API
            var allToppings = await Task.Run(() => _dao.OrderToppings.GetAll(
                pageNumber: 1,
                pageSize: 1000 // Giả sử lấy số lượng lớn để bao quát
            ));

            InvoiceDetails.Clear();
            foreach (var item in result)
            {
                // Lọc topping theo InvoiceDetailId và gán vào Toppings
                var toppingsForInvoiceDetail = allToppings.Where(t => t.InvoiceDetailId == item.Id).ToList();
                item.Toppings.Clear(); // Xóa danh sách cũ (nếu có)
                foreach (var topping in toppingsForInvoiceDetail)
                {
                    item.Toppings.Add(topping);
                }
                Debug.WriteLine($"InvoiceDetail {item.Id} has {item.Toppings.Count} toppings");

                InvoiceDetails.Add(item);
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
