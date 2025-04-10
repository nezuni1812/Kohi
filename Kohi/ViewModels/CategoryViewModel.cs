using Kohi.Models;
using Kohi.Services;
using Kohi.Utils;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.Storage;
using System.Diagnostics;

namespace Kohi.ViewModels
{
    public class CategoryViewModel
    {
        private IDao _dao;
        public FullObservableCollection<CategoryModel> Categories { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize); // Tổng số trang
        public CategoryViewModel()
        {
            _dao = Service.GetKeyedSingleton<IDao>();
            Categories = new FullObservableCollection<CategoryModel>();
            LoadData();
        }

        public async Task LoadData(int page = 1)
        {
            CurrentPage = page;
            TotalItems = _dao.Categories.GetCount();
            var categoriesResult = await Task.Run(() => _dao.Categories.GetAll(
                pageNumber: CurrentPage,
                pageSize: PageSize
            ));

            // Lấy tất cả sản phẩm từ API
            var allProducts = await Task.Run(() => _dao.Products.GetAll(
                pageNumber: 1, // Lấy trang đầu tiên
                pageSize: 1000 // Giả sử lấy số lượng lớn để bao quát, tùy chỉnh theo nhu cầu
            ));

            Categories.Clear();

            foreach (var item in categoriesResult)
            {
                if (!Categories.Any(c => c.Id == item.Id))
                {
                    // Xử lý ImageUrl cho danh mục
                    if (!string.IsNullOrEmpty(item.ImageUrl))
                    {
                        try
                        {
                            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                            item.ImageUrl = System.IO.Path.Combine(localFolder.Path, item.ImageUrl);
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine("Safely skip: " + e.StackTrace);
                        }
                    }

                    // Lọc sản phẩm theo CategoryId và gán vào danh mục
                    var productsForCategory = allProducts.Where(p => p.CategoryId == item.Id).ToList();
                    item.Products.Clear(); // Xóa danh sách cũ (nếu có)
                    foreach (var product in productsForCategory)
                    {
                        item.Products.Add(product);
                    }
                    Debug.WriteLine($"Category {item.Id} has {item.Products.Count} products");
                }
                Categories.Add(item);
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

        public async Task Add(CategoryModel category)
        {
            try
            {
                int result = _dao.Categories.Insert(category);
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
                int result = _dao.Categories.DeleteById(id);
                await LoadData(CurrentPage);
            }
            catch (Exception ex)
            {

            }
        }

        public async Task Update(string id, CategoryModel category)
        {
            try
            {
                int result = _dao.Categories.UpdateById(id, category);
            }
            catch (Exception ex)
            {

            }
        }

        public async Task<CategoryModel> GetById(string id)
        {
            try
            {
                var category = _dao.Categories.GetById(id); // Đồng bộ
                return category;
            }
            catch (Exception ex)
            {
                return null; // Trả về null khi có lỗi
                // Xử lý lỗi (tùy chọn)
            }
        }
    }
}
