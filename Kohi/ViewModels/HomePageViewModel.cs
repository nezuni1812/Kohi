using Kohi.Models;
using Kohi.Services;
using Kohi.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.ViewModels
{
    public class HomePageViewModel
    {
        public CategoryViewModel CategoryViewModel { get; set; } = new CategoryViewModel();
        public ProductViewModel ProductViewModel { get; set; } = new ProductViewModel(1000);
        // public OrderToppingViewModel OrderToppingViewModel { get; set; } = new OrderToppingViewModel();
        // public InvoiceDetailViewModel InvoiceDetailViewModel { get; set; } = new InvoiceDetailViewModel();
        public CustomerViewModel CustomerViewModel { get; set; } = new CustomerViewModel();
        // public PaymentViewModel PaymentViewModel { get; set; } = new PaymentViewModel();
        public InvoiceViewModel InvoiceViewModel { get; set; } = new InvoiceViewModel();
        // public InvoiceTaxViewModel InvoiceTaxViewModel { get; set; } = new InvoiceTaxViewModel();
        private FullObservableCollection<ProductModel> _allProducts => new FullObservableCollection<ProductModel>(
            ProductViewModel.Products.Where(p => p.IsTopping != true));
        public FullObservableCollection<ProductModel> FilteredProducts { get; set; }
        public FullObservableCollection<ProductModel> ToppingProducts => new FullObservableCollection<ProductModel>(
            ProductViewModel.Products.Where(p => p.IsTopping == true));

        public FullObservableCollection<InvoiceDetailModel> OrderItems = new FullObservableCollection<InvoiceDetailModel>();

        public float TotalPrice => OrderItems?.Sum(item =>
            item.ProductVariant.Price * item.Quantity +
            (item.Toppings?.Sum(t => t.ProductVariant.Price * t.Quantity) ?? 0)) ?? 0;
        public int TotalItems => OrderItems?.Sum(item => item.Quantity) ?? 0;

        public FullObservableCollection<InvoiceModel> Invoice = new FullObservableCollection<InvoiceModel>();

        private IEnumerable<ProductModel> GetAllProducts()
        {
            return ProductViewModel.Products.Where(p => p.IsTopping != true);
        }

        public HomePageViewModel()
        {
            FilteredProducts = new FullObservableCollection<ProductModel>(_allProducts);
        }

        public void FilterProductsByCategory(int? categoryId)
        {
            FilteredProducts.Clear();
            if (categoryId == null)
            {
                foreach (var product in GetAllProducts())
                {
                    FilteredProducts.Add(product);
                }
            }
            else
            {
                var filtered = GetAllProducts().Where(p => p.CategoryId == categoryId);
                foreach (var product in filtered)
                {
                    FilteredProducts.Add(product);
                }
            }
        }

        public void MapProductToVariants(InvoiceDetailModel item)
        {
            if (item.ProductVariant != null)
            {
                item.ProductVariant.Product = ProductViewModel.Products.FirstOrDefault(p => p.Id == item.ProductVariant.ProductId);
            }

            foreach (var topping in item.Toppings)
            {
                if (topping.ProductVariant != null)
                {
                    topping.ProductVariant.Product = ProductViewModel.Products.FirstOrDefault(p => p.Id == topping.ProductVariant.ProductId);
                }
            }
        }
    }
}