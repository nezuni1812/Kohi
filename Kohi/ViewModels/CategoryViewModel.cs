using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Kohi.Models;
using Kohi.Services;
using Kohi.Utils;

namespace Kohi.ViewModels
{
    public class CategoryViewModel
    {
        private IDao _dao;
        public FullObservableCollection<CategoryModel> Categories { get; set; }
        public CategoryViewModel()
        {
            _dao = Service.GetKeyedSingleton<IDao>();
            Categories = new FullObservableCollection<CategoryModel>();
            LoadCategories();
        }

        private void LoadCategories()
        {
            try
            {
                if (_dao?.Categories == null)
                {
                    Debug.WriteLine("DAO or Categories is null.");
                    return;
                }

                var categories = _dao.Categories.GetAll();
                Categories.Clear();

                foreach (var category in categories)
                {
                    Categories.Add(category);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading categories: {ex.Message}");
            }
        }
    }
}
