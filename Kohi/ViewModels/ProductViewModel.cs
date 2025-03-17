//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Kohi.BusinessLogic;
//using Kohi.Models;

//namespace Kohi.ViewModels
//{
//    class ProductViewModel
//    {
//        private ProductService _service;
//        public ObservableCollection<ProductModel> Products { get; set; }
//        public string NewProductName { get; set; } = "";
//        public string NewCategoryId { get; set; } = "";

//        public ProductViewModel()
//        {
//            _service = new ProductService();
//            Products = new ObservableCollection<ProductModel>();

//            LoadProducts();
//        }

//        private async void LoadProducts()
//        {
//            var products = await _service.GetProductAsync();
//            Products.Clear();

//            foreach (var product in products)
//            {
//                Products.Add(product);
//            }
//        }

//        public async void AddProduct()
//        {
//            await _service.AddProductAsync(new ProductModel { Name = NewProductName });
//            LoadProducts();
//            NewProductName = "";
//            NewCategoryId = null;
//        }
//    }
//}


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kohi.Models;
using Kohi.Services;

namespace Kohi.ViewModels
{
    public class ProductViewModel
    {
        private IDao _dao;
        public ObservableCollection<ProductModel> Products { get; set; }

        public ProductViewModel()
        {
            _dao = Service.GetKeyedSingleton<IDao>();
            Products = new ObservableCollection<ProductModel>();

            LoadProducts();
        }

        private async void LoadProducts()
        {
            // Giả lập tải dữ liệu không đồng bộ từ MockDao
            await Task.Delay(1); // Giả lập delay để giữ async
            var products = _dao.Products.GetAll();
            Products.Clear();

            foreach (var product in products)
            {
                Products.Add(product);
            }
        }
    }
}