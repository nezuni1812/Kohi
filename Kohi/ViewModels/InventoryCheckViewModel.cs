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
    public class InventoryCheckViewModel
    {
        private IDao _dao;
        public FullObservableCollection<CheckInventoryModel> CheckInventories { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10; 
        public int TotalItems { get; set; } 
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize); // Tổng số trang
        public InventoryCheckViewModel()
        {
            _dao = Service.GetKeyedSingleton<IDao>();
            CheckInventories = new FullObservableCollection<CheckInventoryModel>();

            LoadData();
        }

        public async Task LoadData(int page = 1)
        {
            CurrentPage = page;
            TotalItems = _dao.CheckInventories.GetCount();
            var result = await Task.Run(() => _dao.CheckInventories.GetAll(
                pageNumber: CurrentPage,
                pageSize: PageSize
            ));
            CheckInventories.Clear();
            foreach (var item in result)
            {
                item.Inventory = _dao.Inventories.GetById(item.InventoryId.ToString());
                if (item.Inventory != null)
                {
                    // Sử dụng item.Inventory.Quantity
                    Debug.WriteLine($"CheckInventory {item.Id}: Inventory Quantity = {item.Inventory.Quantity}");
                    // Ví dụ: Gán TheoryQuantity từ Inventory.Quantity nếu cần
                }
                else
                {
                    Debug.WriteLine($"CheckInventory {item.Id}: Inventory = null, cannot access Quantity");
                }
                item.Discrepancy = item.TheoryQuantity - item.ActualQuantity;
                CheckInventories.Add(item);
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
        public async Task Add(CheckInventoryModel checkInventory)
        {
            try
            {
                // 1. Nối Inventory trước
                var inventory = _dao.Inventories.GetById(checkInventory.InventoryId.ToString());
                if (inventory == null)
                {
                    Debug.WriteLine($"Cannot add CheckInventory: Inventory with ID {checkInventory.InventoryId} not found");
                    return;
                }

                // 2. Gán Inventory vào CheckInventory
                checkInventory.Inventory = inventory;

                // 3. Tính toán TheoryQuantity và Discrepancy trước khi Insert
                checkInventory.TheoryQuantity = inventory.Quantity; // TheoryQuantity lấy từ Inventory hiện tại
                checkInventory.Discrepancy = checkInventory.TheoryQuantity - checkInventory.ActualQuantity;

                // 4. (Tùy chọn) Cập nhật Inventory.Quantity nếu cần
                inventory.Quantity = checkInventory.ActualQuantity; // Nếu bạn muốn cập nhật số lượng thực tế
                _dao.Inventories.UpdateById(checkInventory.InventoryId.ToString(), inventory);

                // 5. Thực hiện Insert sau khi đã nối và tính toán đầy đủ
                int result = _dao.CheckInventories.Insert(checkInventory);

                // 6. Tải lại dữ liệu để phản ánh thay đổi
                await LoadData(CurrentPage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding CheckInventory: {ex.Message}");
            }
        }

        public async Task Delete(string id)
        {
            try
            {
                int result = _dao.CheckInventories.DeleteById(id);
                await LoadData(CurrentPage);
            }
            catch (Exception ex)
            {

            }
        }

        public async Task Update(string id, CheckInventoryModel checkInventory)
        {
            try
            {
                int result = _dao.CheckInventories.UpdateById(id, checkInventory);
            }
            catch (Exception ex)
            {

            }
        }
    }
}
