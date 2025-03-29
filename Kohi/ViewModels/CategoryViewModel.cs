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
            var result = await Task.Run(() => _dao.Categories.GetAll(
                pageNumber: CurrentPage,
                pageSize: PageSize
            ));

            Categories.Clear();

            foreach (var item in result)
            {
                // Kiểm tra xem danh mục đã tồn tại trong danh sách chưa
                if (!Categories.Any(c => c.Id == item.Id))
                {
                    // Đường dẫn đầy đủ tới hình ảnh trong LocalFolder
                    if (!string.IsNullOrEmpty(item.ImageUrl))
                    {
                        // Tạo đường dẫn đầy đủ
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
                int result = _dao.Categories.Insert($"{TotalItems + 1}", category);
            }
            catch (Exception ex)
            {

            }
        }
    }
}
