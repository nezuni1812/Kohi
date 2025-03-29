using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kohi.Models;
using static Kohi.Services.MockDao;

namespace Kohi.Services
{
    internal class APIDao : IDao
    {
        public class APICategoryRepository : IRepository<CategoryModel>
        {
            public int DeleteById(string id)
            {
                throw new NotImplementedException();
            }

            public List<CategoryModel> GetAll(int pageNumber = 1, int pageSize = 10, string sortBy = null, bool sortDescending = false, string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                throw new NotImplementedException();
            }

            public CategoryModel GetById(string id)
            {
                throw new NotImplementedException();
            }

            public int GetCount(string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                throw new NotImplementedException();
            }

            public int Insert(string id, CategoryModel info)
            {
                throw new NotImplementedException();
            }

            public int UpdateById(string id, CategoryModel info)
            {
                throw new NotImplementedException();
            }
        }

        public APIDao()
        {
            var categories = new APICategoryRepository().GetAll();
            //var customers = new MockCustomerRepository().GetAll();
            //var expenseCategories = new MockExpenseCategoryRepository().GetAll();
            //var expenses = new MockExpenseRepository().GetAll();
            //var inbounds = new MockInboundRepository().GetAll();
            //var invoices = new MockInvoiceRepository().GetAll();
            //var inventories = new MockInventoryRepository().GetAll();
            //var ingredients = new MockIngredientRepository().GetAll();
            //var invoiceDetails = new MockInvoiceDetailRepository().GetAll();
            //var recipeDetails = new MockRecipeDetailRepository().GetAll();
            //var payments = new MockPaymentRepository().GetAll();
            //var orderToppings = new MockOrderToppingRepository().GetAll();
            //var suppliers = new MockSupplierRepository().GetAll();
            //var products = new MockProductRepository().GetAll();
            //var outbounds = new MockOutboundRepository().GetAll();
            //var productVariants = new MockProductVariantRepository().GetAll();
            //var taxes = new MockTaxRepository().GetAll();
            //var invoiceTaxes = new MockInvoiceTaxRepository().GetAll();
            //var checkInventories = new MockCheckInventoryRepository().GetAll();
        }

        //Properties
        public IRepository<CategoryModel> Categories { get; set; }
        public IRepository<ProductModel> Products { get; set; }
        public IRepository<CustomerModel> Customers { get; set; }
        public IRepository<InvoiceModel> Invoices { get; set; }
        public IRepository<ExpenseCategoryModel> ExpenseCategories { get; set; }
        public IRepository<ExpenseModel> Expenses { get; set; }
        public IRepository<PaymentModel> Payments { get; set; }
        public IRepository<SupplierModel> Suppliers { get; set; }
        public IRepository<InboundModel> Inbounds { get; set; }
        public IRepository<InventoryModel> Inventories { get; set; }
        public IRepository<IngredientModel> Ingredients { get; set; }
        public IRepository<InvoiceDetailModel> InvoiceDetails { get; set; }
        public IRepository<RecipeDetailModel> RecipeDetails { get; set; }
        public IRepository<OrderToppingModel> OrderToppings { get; set; }
        public IRepository<OutboundModel> Outbounds { get; set; }
        public IRepository<ProductVariantModel> ProductVariants { get; set; }
        public IRepository<TaxModel> Taxes { get; set; }
        public IRepository<InvoiceTaxModel> InvoiceTaxes { get; set; }
        public IRepository<CheckInventoryModel> CheckInventories { get; set; }
    }
}
