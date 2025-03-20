using Kohi.Models;
using Kohi.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.ViewModels
{
    public class CategoryViewModel
    {
        private IDao _dao;
        public ObservableCollection<CategoryModel> Categories { get; set; }

        public CategoryViewModel()
        {
            _dao = Service.GetKeyedSingleton<IDao>();
            Categories = new ObservableCollection<CategoryModel>();

            LoadData();
        }

        private async void LoadData()
        {
            // Giả lập tải dữ liệu không đồng bộ từ MockDao
            await Task.Delay(1); // Giả lập delay để giữ async
            var categories = _dao.Categories.GetAll();
            Categories.Clear();

            foreach (var category in categories)
            {
                Categories.Add(category);
            }
        }
    }
}
