using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kohi.Models;
using Kohi.Services;
using Kohi.Utils;

namespace Kohi.ViewModels
{
    public class IngredientViewModel 
    {
        private IDao _dao;
        public FullObservableCollection<IngredientModel> Ingredients { get; set; }

        public IngredientViewModel()
        {
            _dao = Service.GetKeyedSingleton<IDao>();
            Ingredients = new FullObservableCollection<IngredientModel>();

            LoadData();
        }

        private async void LoadData()
        {
            await Task.Delay(0); // Giả lập delay để giữ async
            var categories = _dao.Ingredients.GetAll();
            Ingredients.Clear();

            foreach (var category in categories)
            {
                Ingredients.Add(category);
            }
        }
    }
}
