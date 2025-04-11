using Kohi.Models;
using Kohi.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.ViewModels
{
    public class SupplierViewModel
    {
        private IDao _dao;
        public ObservableCollection<SupplierModel> Suppliers { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize); // Tổng số trang
        public SupplierViewModel()
        {
            _dao = Service.GetKeyedSingleton<IDao>();
            Suppliers = new ObservableCollection<SupplierModel>();

            LoadData();
        }

        public async Task LoadData(int page = 1)
        {
            CurrentPage = page;
            TotalItems = _dao.Suppliers.GetCount();
            var result = await Task.Run(() => _dao.Suppliers.GetAll(
                pageNumber: CurrentPage,
                pageSize: PageSize
            ));

            // Lấy tất cả inbound từ API
            var allInbounds = await Task.Run(() => _dao.Inbounds.GetAll(
                pageNumber: 1,
                pageSize: 1000 // Giả sử lấy số lượng lớn để bao quát
            ));

            Suppliers.Clear();
            foreach (var supplier in result)
            {
                // Lọc inbound theo SupplierId và gán vào Inbounds
                var inboundsForSupplier = allInbounds.Where(i => i.SupplierId == supplier.Id).ToList();
                supplier.Inbounds.Clear(); // Xóa danh sách cũ (nếu có)
                foreach (var inbound in inboundsForSupplier)
                {
                    supplier.Inbounds.Add(inbound);
                }
                Debug.WriteLine($"Supplier {supplier.Id} has {supplier.Inbounds.Count} inbounds");

                Suppliers.Add(supplier);
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
        public async Task Add(SupplierModel supplier)
        {
            try
            {
                int result = _dao.Suppliers.Insert(supplier);
                await LoadData(CurrentPage);
            }
            catch (Exception ex)
            {

            }
        }

        public async Task Delete(string id)
        {
            try
            {
                int result = _dao.Suppliers.DeleteById(id);
                await LoadData(CurrentPage);
            }
            catch (Exception ex)
            {

            }
        }

        public async Task Update(string id, SupplierModel supplier)
        {
            try
            {
                int result = _dao.Suppliers.UpdateById(id, supplier);
            }
            catch (Exception ex)
            {

            }
        }
        public async Task<List<SupplierModel>> GetAll()
        {
            try
            {
                var Suppliers = _dao.Suppliers.GetAll(1, 1000); // Đồng bộ
                return Suppliers;
            }
            catch (Exception ex)
            {
                return null; // Trả về null khi có lỗi
                // Xử lý lỗi (tùy chọn)
            }
        }
    }
}
