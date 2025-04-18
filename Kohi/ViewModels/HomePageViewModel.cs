﻿using Kohi.Models;
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
        //public ProductVariantViewModel ProductVariantViewModel { get; set; } 
        public OrderToppingViewModel OrderToppingViewModel { get; set; }
        public InvoiceDetailViewModel InvoiceDetailViewModel { get; set; }
        public CustomerViewModel CustomerViewModel { get; set; }
        public PaymentViewModel PaymentViewModel { get; set; }

        public InvoiceViewModel InvoiceViewModel { get; set; }
        public InvoiceTaxViewModel InvoiceTaxViewModel { get; set; }

        public FullObservableCollection<ProductModel> Products => new FullObservableCollection<ProductModel>(
            ProductViewModel.Products.Where(p => p.IsTopping != true));

        public FullObservableCollection<ProductModel> ToppingProducts => new FullObservableCollection<ProductModel>(
            ProductViewModel.Products.Where(p => p.IsTopping == true));

        public FullObservableCollection<InvoiceDetailModel> OrderItems = new FullObservableCollection<InvoiceDetailModel>();

        public float TotalPrice => OrderItems?.Sum(item => item.ProductVariant.Price + (item.Toppings?.Sum(t => t.ProductVariant.Price) ?? 0)) ?? 0;

        public int TotalItems => OrderItems?.Count ?? 0;

        public FullObservableCollection<InvoiceModel> Invoice = new FullObservableCollection<InvoiceModel>();

        public HomePageViewModel()
        {
            CategoryViewModel = new CategoryViewModel();
            ProductViewModel = new ProductViewModel();
            //ProductVariantViewModel = new ProductVariantViewModel();
            OrderToppingViewModel = new OrderToppingViewModel();
            InvoiceDetailViewModel = new InvoiceDetailViewModel();
            CustomerViewModel = new CustomerViewModel();
            PaymentViewModel = new PaymentViewModel();
            InvoiceViewModel = new InvoiceViewModel();
            InvoiceTaxViewModel = new InvoiceTaxViewModel();
        }
    }
}
