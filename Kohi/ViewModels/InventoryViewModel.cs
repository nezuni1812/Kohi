using Kohi.Models;
using Kohi.Services;
using Kohi.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.ViewModels
{
    public class InventoryViewModel
    {
        private IDao _dao;
        public FullObservableCollection<InventoryModel> Inventories { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; } 
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize); // Tổng số trang
        public InventoryViewModel()
        {
            _dao = Service.GetKeyedSingleton<IDao>();
            Inventories = new FullObservableCollection<InventoryModel>();

            LoadData();
        }

        public async Task LoadData(int page = 1)
        {
            CurrentPage = page;
            TotalItems = _dao.Inventories.GetCount();
            var result = await Task.Run(() => _dao.Inventories.GetAll(
                pageNumber: CurrentPage,
                pageSize: PageSize
            ));

            var allInbounds = await Task.Run(() => _dao.Inbounds.GetAll(1, 1000));
            var allIngredients = await Task.Run(() => _dao.Ingredients.GetAll(1, 1000));
            var allSuppliers = await Task.Run(() => _dao.Suppliers.GetAll(1, 1000));

            Inventories.Clear();
            foreach (var item in result)
            {
                item.Inbound = allInbounds.FirstOrDefault(i => i.Id == item.InboundId);
                if (item.Inbound != null)
                {
                    item.Inbound.Ingredient = allIngredients.FirstOrDefault(i => i.Id == item.Inbound.IngredientId);
                    item.Inbound.Supplier = allSuppliers.FirstOrDefault(s => s.Id == item.Inbound.SupplierId);
                }
                Inventories.Add(item);
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
        public async Task Add(InventoryModel inventory)
        {
            try
            {
                int result = _dao.Inventories.Insert(inventory);
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
                int result = _dao.Inventories.DeleteById(id);
                await LoadData(CurrentPage);
            }
            catch (Exception ex)
            {

            }
        }

        public async Task Update(string id, InventoryModel inventory)
        {
            try
            {
                int result = _dao.Inventories.UpdateById(id, inventory);
            }
            catch (Exception ex)
            {

            }
        }
    }
}
