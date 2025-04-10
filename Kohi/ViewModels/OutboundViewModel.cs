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
            TotalItems = _dao.Outbounds.GetCount();
            var result = await Task.Run(() => _dao.Outbounds.GetAll(
                pageNumber: CurrentPage,
                pageSize: PageSize
            ));

            // Lấy tất cả dữ liệu một lần
            var allInventories = await Task.Run(() => _dao.Inventories.GetAll(1, 1000));
            var allInbounds = await Task.Run(() => _dao.Inbounds.GetAll(1, 1000));
            var allIngredients = await Task.Run(() => _dao.Ingredients.GetAll(1, 1000));
            var allSuppliers = await Task.Run(() => _dao.Suppliers.GetAll(1, 1000));

            Outbounds.Clear();
            foreach (var item in result)
            {
                // Nối Inventory
                item.Inventory = allInventories.FirstOrDefault(i => i.Id == item.InventoryId);
                if (item.Inventory != null)
                {
                    // Nối Inbound vào Inventory
                    item.Inventory.Inbound = allInbounds.FirstOrDefault(i => i.Id == item.Inventory.InboundId);
                    if (item.Inventory.Inbound != null)
                    {
                        // Nối Ingredient và Supplier vào Inbound
                        item.Inventory.Inbound.Ingredient = allIngredients.FirstOrDefault(i => i.Id == item.Inventory.Inbound.IngredientId);
                        item.Inventory.Inbound.Supplier = allSuppliers.FirstOrDefault(s => s.Id == item.Inventory.Inbound.SupplierId);
                        Debug.WriteLine($"Outbound {item.Id}: Inventory = {item.Inventory.Id}, Inbound = {item.Inventory.Inbound.Id}, Ingredient = {(item.Inventory.Inbound.Ingredient != null ? item.Inventory.Inbound.Ingredient.Name : "null")}, Supplier = {(item.Inventory.Inbound.Supplier != null ? item.Inventory.Inbound.Supplier.Name : "null")}");
                    }
                    else
                    {
                        Debug.WriteLine($"Outbound {item.Id}: Inventory = {item.Inventory.Id}, Inbound = null");
                    }
                }
                else
                {
                    Debug.WriteLine($"Outbound {item.Id}: Inventory = null");
                }
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
