using Kohi.Models;
using Kohi.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kohi.Utils;

namespace Kohi.ViewModels
{
    public class HomePageViewModel
    {
        public CategoryViewModel CategoryViewModel { get; set; }
        public ProductViewModel ProductViewModel { get; set; }

        public FullObservableCollection<ProductModel> Products => new FullObservableCollection<ProductModel>(
            ProductViewModel.Products.Where(p => p.IsTopping != true));

        public FullObservableCollection<ProductModel> ToppingProducts => new FullObservableCollection<ProductModel>(
            ProductViewModel.Products.Where(p => p.IsTopping == true));

        public HomePageViewModel()
        {
            CategoryViewModel = new CategoryViewModel();
            ProductViewModel = new ProductViewModel();
        }
    }
}
