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
        private IDao _dao = Service.GetKeyedSingleton<IDao>();
        public CategoryViewModel CategoryViewModel { get; set; }
        public ProductViewModel ProductViewModel { get; set; }
        public OrderToppingViewModel OrderToppingViewModel { get; set; }
        public InvoiceDetailViewModel InvoiceDetailViewModel { get; set; }
        public CustomerViewModel CustomerViewModel { get; set; }
        public PaymentViewModel PaymentViewModel { get; set; }
        public InvoiceViewModel InvoiceViewModel { get; set; }
        public InvoiceTaxViewModel InvoiceTaxViewModel { get; set; }
        private FullObservableCollection<ProductModel> _allProducts => new FullObservableCollection<ProductModel>(
            _dao.Products.GetAll(1, 1000).Where(p => p.IsTopping != true));
        public FullObservableCollection<ProductModel> FilteredProducts { get; set; }
        public FullObservableCollection<ProductModel> ToppingProducts => new FullObservableCollection<ProductModel>(
            _dao.Products.GetAll(1, 1000).Where(p => p.IsTopping == true));

        public FullObservableCollection<InvoiceDetailModel> OrderItems = new FullObservableCollection<InvoiceDetailModel>();

        public float TotalPrice => OrderItems?.Sum(item =>
            item.ProductVariant.Price * item.Quantity +
            (item.Toppings?.Sum(t => t.ProductVariant.Price * t.Quantity) ?? 0)) ?? 0;
        public int TotalItems => OrderItems?.Sum(item => item.Quantity) ?? 0;

        public FullObservableCollection<InvoiceModel> Invoice = new FullObservableCollection<InvoiceModel>();

        public HomePageViewModel()
        {
            CategoryViewModel = new CategoryViewModel();
            ProductViewModel = new ProductViewModel();
            OrderToppingViewModel = new OrderToppingViewModel();
            InvoiceDetailViewModel = new InvoiceDetailViewModel();
            CustomerViewModel = new CustomerViewModel();
            PaymentViewModel = new PaymentViewModel();
            InvoiceViewModel = new InvoiceViewModel();
            InvoiceTaxViewModel = new InvoiceTaxViewModel();
            FilteredProducts = new FullObservableCollection<ProductModel>(_allProducts);
        }
        public void FilterProductsByCategory(int? categoryId)
        {
            FilteredProducts.Clear();
            if (categoryId == null)
            {
                foreach (var product in _allProducts)
                {
                    FilteredProducts.Add(product);
                }
            }
            else
            {
                var filtered = _allProducts.Where(p => p.CategoryId == categoryId);
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