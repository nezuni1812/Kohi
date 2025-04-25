using Kohi.Models;
using Kohi.Services;
using Kohi.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.ViewModels
{
    public class InvoiceTaxViewModel
    {
        private IDao _dao;
        public FullObservableCollection<InvoiceTaxModel> InvoiceTaxs { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize); // Tổng số trang
        public InvoiceTaxViewModel()
        {
            _dao = Service.GetKeyedSingleton<IDao>();
            InvoiceTaxs = new FullObservableCollection<InvoiceTaxModel>();

            LoadData();
        }

        public async Task LoadData(int page = 1)
        {
            CurrentPage = page;
            TotalItems = _dao.InvoiceTaxes.GetCount(); // Lấy tổng số khách hàng từ DAO
            var result = await Task.Run(() => _dao.InvoiceTaxes.GetAll(
                pageNumber: CurrentPage,
                pageSize: PageSize
            )); // Lấy danh sách khách hàng phân trang
            InvoiceTaxs.Clear();
            foreach (var item in result)
            {
                InvoiceTaxs.Add(item);
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
        public async Task<List<InvoiceTaxModel>> GetAll()
        {
            try
            {
                var InvoiceTaxes = _dao.InvoiceTaxes.GetAll(1, 1000); // Đồng bộ
                return InvoiceTaxes;
            }
            catch (Exception ex)
            {
                return null; // Trả về null khi có lỗi
                // Xử lý lỗi (tùy chọn)
            }
        }
    }
}
