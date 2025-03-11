using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kohi.BusinessLogic;
using Kohi.Models;

namespace Kohi.ViewModels
{
    class ProductViewModel
    {
        private ProductService _service;
        public ObservableCollection<Product> Products { get; set; }
        public string NewProductName { get; set; } = "";
        public string NewCategoryId { get; set; } = "";

        public ProductViewModel()
        {
            _service = new ProductService();
            Products = new ObservableCollection<Product>();

            LoadProducts();
        }

        private async void LoadProducts()
        {
            var products = await _service.GetProductAsync();
            Products.Clear();

            foreach (var product in products)
            {
                Products.Add(product);
            }
        }

        public async void AddProduct()
        {
            await _service.AddProductAsync(new Product { Name = NewProductName });
            LoadProducts();
            NewProductName = "";
            NewCategoryId = null;
        }
    }
}
