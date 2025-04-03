using Kohi.Models;
using Kohi.Services;
using Kohi.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.ViewModels
{
    public class OutboundViewModel
    {
        private IDao _dao;
        public FullObservableCollection<OutboundModel> Outbounds { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10; 
        public int TotalItems { get; set; } 
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize); // Tổng số trang
        public OutboundViewModel()
        {
            _dao = Service.GetKeyedSingleton<IDao>();
            Outbounds = new FullObservableCollection<OutboundModel>();

            LoadData();
        }

        public async Task LoadData(int page = 1)
        {
            CurrentPage = page;
            TotalItems = _dao.Outbounds.GetCount(); // Lấy tổng số khách hàng từ DAO
            var result = await Task.Run(() => _dao.Outbounds.GetAll(
                pageNumber: CurrentPage,
                pageSize: PageSize
            )); // Lấy danh sách khách hàng phân trang
            Outbounds.Clear();
            foreach (var item in result)
            {
                item.Inventory = _dao.Inventories.GetById(item.InventoryId.ToString());
                Outbounds.Add(item);
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
        public async Task Add(OutboundModel outbound)
        {
            try
            {
                int result = _dao.Outbounds.Insert(outbound);
                var inventory = _dao.Inventories.GetById(outbound.InventoryId.ToString());
                inventory.Quantity -= outbound.Quantity;
                _dao.Inventories.UpdateById(outbound.InventoryId.ToString(), inventory);
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
                int result = _dao.Outbounds.DeleteById(id);
                await LoadData(CurrentPage);
            }
            catch (Exception ex)
            {

            }
        }

        public async Task Update(string id, OutboundModel outbound)
        {
            try
            {
                int result = _dao.Outbounds.UpdateById(id, outbound);
            }
            catch (Exception ex)
            {

            }
        }
    }
}
