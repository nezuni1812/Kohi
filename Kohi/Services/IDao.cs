﻿using Kohi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.Services
{
    public interface IDao
    {
        IRepository<CategoryModel> Categories { get; set; }
        IRepository<ProductModel> Products { get; set; }
        IRepository<CustomerModel> Customers { get; set; }
        IRepository<InvoiceModel> Invoices { get; set; }
        IRepository<ExpenseCategoryModel> ExpenseCategories { get; set; }
        IRepository<ExpenseModel> Expenses { get; set; }
        IRepository<PaymentModel> Payments { get; set; }
        IRepository<SupplierModel> Suppliers { get; set; }
        IRepository<InboundModel> Inbounds { get; set; }
        IRepository<InventoryModel> Inventories { get; set; }
        IRepository<IngredientModel> Ingredients { get; set; }
        IRepository<InvoiceDetailModel> InvoiceDetails { get; set; }
        IRepository<RecipeDetailModel> RecipeDetails { get; set; } // Sửa từ ProductVariantModel thành RecipeDetailModel
        IRepository<OrderToppingModel> OrderToppings { get; set; }
        IRepository<OutboundModel> Outbounds { get; set; }
        IRepository<ProductVariantModel> ProductVariants { get; set; } // Đổi Recipes thành ProductVariants cho rõ ràng
        IRepository<TaxModel> Taxes { get; set; }
        IRepository<InvoiceTaxModel> InvoiceTaxes { get; set; }
        IRepository<CheckInventoryModel> CheckInventories { get; set; }
    }
}