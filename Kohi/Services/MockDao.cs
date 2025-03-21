using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kohi.Models;

namespace Kohi.Services
{
    internal class MockDao : IDao
    {
        // Mock Repositories
        public class MockCategoryRepository : IRepository<CategoryModel>
        {
            private List<CategoryModel> _categories;

            public MockCategoryRepository() { }
            public MockCategoryRepository(List<CategoryModel> categories)
            {
                _categories = categories;
            }

            public List<CategoryModel> GetAll()
            {
                if (_categories != null) return _categories;
                return new List<CategoryModel>
                {
                    new CategoryModel { Id = 1, Name = "Cà phê", ImageUrl = "kohi_logo.png", Products = new List<ProductModel>() },
                    new CategoryModel { Id = 2, Name = "Trà sữa", ImageUrl = "kohi_logo.png", Products = new List<ProductModel>() },
                    new CategoryModel { Id = 3, Name = "Trà", ImageUrl = "kohi_logo.png", Products = new List<ProductModel>() },
                    new CategoryModel { Id = 4, Name = "Đá xay", ImageUrl = "kohi_logo.png", Products = new List<ProductModel>() }
                };
            }
        }

        public class MockCustomerRepository : IRepository<CustomerModel>
        {
            private List<CustomerModel> _customers;

            public MockCustomerRepository() { }
            public MockCustomerRepository(List<CustomerModel> customers)
            {
                _customers = customers;
            }

            public List<CustomerModel> GetAll()
            {
                if (_customers != null) return _customers;
                return new List<CustomerModel>
                {
                    new CustomerModel { Id = 1, Name = "Nguyen Van A", Email = "a@example.com", Phone = "0901234567", Address = "Hanoi", Invoices = new List<InvoiceModel>() },
                    new CustomerModel { Id = 2, Name = "Tran Thi B", Email = "b@example.com", Phone = "0912345678", Address = "Ho Chi Minh", Invoices = new List<InvoiceModel>() },
                    new CustomerModel { Id = 3, Name = "Le Van C", Email = "c@example.com", Phone = "0923456789", Address = "Da Nang", Invoices = new List<InvoiceModel>() },
                    new CustomerModel { Id = 4, Name = "Pham Thi D", Email = "d@example.com", Phone = "0934567890", Address = "Can Tho", Invoices = new List<InvoiceModel>() },
                    new CustomerModel { Id = 5, Name = "Hoang Van E", Email = "e@example.com", Phone = "0945678901", Address = "Hai Phong", Invoices = new List<InvoiceModel>() }
                };
            }
        }

        public class MockExpenseCategoryRepository : IRepository<ExpenseCategoryModel>
        {
            private List<ExpenseCategoryModel> _expenseCategories;

            public MockExpenseCategoryRepository() { }
            public MockExpenseCategoryRepository(List<ExpenseCategoryModel> expenseCategories)
            {
                _expenseCategories = expenseCategories;
            }

            public List<ExpenseCategoryModel> GetAll()
            {
                if (_expenseCategories != null) return _expenseCategories;
                return new List<ExpenseCategoryModel>
                {
                    new ExpenseCategoryModel { Id = 1, CategoryName = "Raw Materials", ExpenseTypeId = 1, Description = "Cost of ingredients", Expenses = new List<ExpenseModel>() },
                    new ExpenseCategoryModel { Id = 2, CategoryName = "Utilities", ExpenseTypeId = 2, Description = "Electricity and water", Expenses = new List<ExpenseModel>() },
                    new ExpenseCategoryModel { Id = 3, CategoryName = "Staff", ExpenseTypeId = 1, Description = "Employee salaries", Expenses = new List<ExpenseModel>() },
                    new ExpenseCategoryModel { Id = 4, CategoryName = "Marketing", ExpenseTypeId = 3, Description = "Advertising costs", Expenses = new List<ExpenseModel>() },
                    new ExpenseCategoryModel { Id = 5, CategoryName = "Equipment", ExpenseTypeId = 2, Description = "Machine maintenance", Expenses = new List<ExpenseModel>() }
                };
            }
        }

        public class MockExpenseRepository : IRepository<ExpenseModel>
        {
            private List<ExpenseModel> _expenses;

            public MockExpenseRepository() { }
            public MockExpenseRepository(List<ExpenseModel> expenses)
            {
                _expenses = expenses;
            }

            public List<ExpenseModel> GetAll()
            {
                if (_expenses != null) return _expenses;
                return new List<ExpenseModel>
                {
                    new ExpenseModel { Id = 1, ExpenseCategoryId = 1, Amount = 5000000f, ExpenseDate = DateTime.Now.AddDays(-10) },
                    new ExpenseModel { Id = 2, ExpenseCategoryId = 2, Amount = 2000000f, ExpenseDate = DateTime.Now.AddDays(-5) },
                    new ExpenseModel { Id = 3, ExpenseCategoryId = 3, Amount = 10000000f, ExpenseDate = DateTime.Now.AddDays(-2) },
                    new ExpenseModel { Id = 4, ExpenseCategoryId = 4, Amount = 3000000f, ExpenseDate = DateTime.Now.AddDays(-1) },
                    new ExpenseModel { Id = 5, ExpenseCategoryId = 5, Amount = 4000000f, ExpenseDate = DateTime.Now }
                };
            }
        }

        public class MockInboundRepository : IRepository<InboundModel>
        {
            private List<InboundModel> _inbounds;

            public MockInboundRepository() { }
            public MockInboundRepository(List<InboundModel> inbounds)
            {
                _inbounds = inbounds;
            }

            public List<InboundModel> GetAll()
            {
                if (_inbounds != null) return _inbounds;
                return new List<InboundModel>
                {
                    new InboundModel { Id = 1, IngredientId = 1, Quantity = 100, InboundDate = DateTime.Now.AddDays(-15), ExpiryDate = DateTime.Now.AddMonths(6), SupplierId = 1, Notes = "Fresh milk batch" },
                    new InboundModel { Id = 2, IngredientId = 2, Quantity = 50, InboundDate = DateTime.Now.AddDays(-10), ExpiryDate = DateTime.Now.AddMonths(5), SupplierId = 2, Notes = "Sugar supply" },
                    new InboundModel { Id = 3, IngredientId = 3, Quantity = 80, InboundDate = DateTime.Now.AddDays(-7), ExpiryDate = DateTime.Now.AddMonths(4), SupplierId = 3, Notes = "Tea leaves" },
                    new InboundModel { Id = 4, IngredientId = 4, Quantity = 60, InboundDate = DateTime.Now.AddDays(-5), ExpiryDate = DateTime.Now.AddMonths(3), SupplierId = 4, Notes = "Coffee beans" },
                    new InboundModel { Id = 5, IngredientId = 5, Quantity = 70, InboundDate = DateTime.Now.AddDays(-3), ExpiryDate = DateTime.Now.AddMonths(2), SupplierId = 5, Notes = "Fruit puree" }
                };
            }
        }

        public class MockExpenseTypeRepository : IRepository<ExpenseTypeModel>
        {
            private List<ExpenseTypeModel> _expenseTypes;

            public MockExpenseTypeRepository() { }
            public MockExpenseTypeRepository(List<ExpenseTypeModel> expenseTypes)
            {
                _expenseTypes = expenseTypes;
            }

            public List<ExpenseTypeModel> GetAll()
            {
                if (_expenseTypes != null) return _expenseTypes;
                return new List<ExpenseTypeModel>
                {
                    new ExpenseTypeModel { Id = 1, TypeName = "Operational", Description = "Daily operation costs", ExpenseCategories = new List<ExpenseCategoryModel>() },
                    new ExpenseTypeModel { Id = 2, TypeName = "Infrastructure", Description = "Facility costs", ExpenseCategories = new List<ExpenseCategoryModel>() },
                    new ExpenseTypeModel { Id = 3, TypeName = "Marketing", Description = "Promotion costs", ExpenseCategories = new List<ExpenseCategoryModel>() },
                    new ExpenseTypeModel { Id = 4, TypeName = "Miscellaneous", Description = "Other expenses", ExpenseCategories = new List<ExpenseCategoryModel>() },
                    new ExpenseTypeModel { Id = 5, TypeName = "Staff", Description = "Employee-related costs", ExpenseCategories = new List<ExpenseCategoryModel>() }
                };
            }
        }

        public class MockInvoiceRepository : IRepository<InvoiceModel>
        {
            private List<InvoiceModel> _invoices;

            public MockInvoiceRepository() { }
            public MockInvoiceRepository(List<InvoiceModel> invoices)
            {
                _invoices = invoices;
            }

            public List<InvoiceModel> GetAll()
            {
                if (_invoices != null) return _invoices;
                return new List<InvoiceModel>
                {
                    new InvoiceModel { Id = 1, CustomerId = 1, InvoiceDate = DateTime.Now.AddDays(-10), TotalAmount = 150000m, InvoiceDetails = new List<InvoiceDetailModel>() },
                    new InvoiceModel { Id = 2, CustomerId = 2, InvoiceDate = DateTime.Now.AddDays(-5), TotalAmount = 200000m, InvoiceDetails = new List<InvoiceDetailModel>() },
                    new InvoiceModel { Id = 3, CustomerId = 3, InvoiceDate = DateTime.Now.AddDays(-2), TotalAmount = 300000m, InvoiceDetails = new List<InvoiceDetailModel>() },
                    new InvoiceModel { Id = 4, CustomerId = 4, InvoiceDate = DateTime.Now.AddDays(-1), TotalAmount = 250000m, InvoiceDetails = new List<InvoiceDetailModel>() },
                    new InvoiceModel { Id = 5, CustomerId = 5, InvoiceDate = DateTime.Now, TotalAmount = 180000m, InvoiceDetails = new List<InvoiceDetailModel>() }
                };
            }
        }

        public class MockInventoryRepository : IRepository<InventoryModel>
        {
            private List<InventoryModel> _inventories;

            public MockInventoryRepository() { }
            public MockInventoryRepository(List<InventoryModel> inventories)
            {
                _inventories = inventories;
            }

            public List<InventoryModel> GetAll()
            {
                if (_inventories != null) return _inventories;
                return new List<InventoryModel>
                {
                    new InventoryModel { Id = 1, InboundId = 1, IngredientId = 1, Quantity = 90, InitialQuantity = 100, InboundDate = DateTime.Now.AddDays(-15), ExpiryDate = DateTime.Now.AddMonths(6), SupplierId = 1 },
                    new InventoryModel { Id = 2, InboundId = 2, IngredientId = 2, Quantity = 45, InitialQuantity = 50, InboundDate = DateTime.Now.AddDays(-10), ExpiryDate = DateTime.Now.AddMonths(5), SupplierId = 2 },
                    new InventoryModel { Id = 3, InboundId = 3, IngredientId = 3, Quantity = 75, InitialQuantity = 80, InboundDate = DateTime.Now.AddDays(-7), ExpiryDate = DateTime.Now.AddMonths(4), SupplierId = 3 },
                    new InventoryModel { Id = 4, InboundId = 4, IngredientId = 4, Quantity = 55, InitialQuantity = 60, InboundDate = DateTime.Now.AddDays(-5), ExpiryDate = DateTime.Now.AddMonths(3), SupplierId = 4 },
                    new InventoryModel { Id = 5, InboundId = 5, IngredientId = 5, Quantity = 65, InitialQuantity = 70, InboundDate = DateTime.Now.AddDays(-3), ExpiryDate = DateTime.Now.AddMonths(2), SupplierId = 5 }
                };
            }
        }

        public class MockIngredientRepository : IRepository<IngredientModel>
        {
            private List<IngredientModel> _ingredients;

            public MockIngredientRepository() { }
            public MockIngredientRepository(List<IngredientModel> ingredients)
            {
                _ingredients = ingredients;
            }

            public List<IngredientModel> GetAll()
            {
                if (_ingredients != null) return _ingredients;
                return new List<IngredientModel>
                {
                    new IngredientModel { Id = 1, Name = "Milk", Unit = "Liter", CostPerUnit = 20000f, SupplierId = 1, Description = "Fresh milk" },
                    new IngredientModel { Id = 2, Name = "Sugar", Unit = "Kg", CostPerUnit = 15000f, SupplierId = 2, Description = "Refined sugar" },
                    new IngredientModel { Id = 3, Name = "Tea Leaves", Unit = "Kg", CostPerUnit = 30000f, SupplierId = 3, Description = "Green tea leaves" },
                    new IngredientModel { Id = 4, Name = "Coffee Beans", Unit = "Kg", CostPerUnit = 40000f, SupplierId = 4, Description = "Arabica beans" },
                    new IngredientModel { Id = 5, Name = "Fruit Puree", Unit = "Liter", CostPerUnit = 25000f, SupplierId = 5, Description = "Mixed fruit puree" }
                };
            }
        }

        public class MockInvoiceDetailRepository : IRepository<InvoiceDetailModel>
        {
            private List<InvoiceDetailModel> _invoiceDetails;

            public MockInvoiceDetailRepository() { }
            public MockInvoiceDetailRepository(List<InvoiceDetailModel> invoiceDetails)
            {
                _invoiceDetails = invoiceDetails;
            }

            public List<InvoiceDetailModel> GetAll()
            {
                if (_invoiceDetails != null) return _invoiceDetails;
                return new List<InvoiceDetailModel>
                {
                    new InvoiceDetailModel { Id = 1, InvoiceId = 1, ProductId = 1, Quantity = 2, UnitPrice = 35000m, SugarLevel = 50, IceLevel = 70, Toppings = new List<OrderToppingModel>() },
                    new InvoiceDetailModel { Id = 2, InvoiceId = 2, ProductId = 2, Quantity = 1, UnitPrice = 40000m, SugarLevel = 30, IceLevel = 50, Toppings = new List<OrderToppingModel>() },
                    new InvoiceDetailModel { Id = 3, InvoiceId = 3, ProductId = 3, Quantity = 3, UnitPrice = 30000m, SugarLevel = 40, IceLevel = 60, Toppings = new List<OrderToppingModel>() },
                    new InvoiceDetailModel { Id = 4, InvoiceId = 4, ProductId = 1, Quantity = 1, UnitPrice = 35000m, SugarLevel = 60, IceLevel = 80, Toppings = new List<OrderToppingModel>() },
                    new InvoiceDetailModel { Id = 5, InvoiceId = 5, ProductId = 2, Quantity = 2, UnitPrice = 40000m, SugarLevel = 20, IceLevel = 40, Toppings = new List<OrderToppingModel>() }
                };
            }
        }

        public class MockRecipeDetailRepository : IRepository<RecipeDetailModel>
        {
            private List<RecipeDetailModel> _recipeDetails;

            public MockRecipeDetailRepository() { }
            public MockRecipeDetailRepository(List<RecipeDetailModel> recipeDetails)
            {
                _recipeDetails = recipeDetails;
            }

            public List<RecipeDetailModel> GetAll()
            {
                if (_recipeDetails != null) return _recipeDetails;
                return new List<RecipeDetailModel>
                {
                    new RecipeDetailModel { RecipeId = 1, IngredientId = 1, Quantity = 200, Unit = "ml" },
                    new RecipeDetailModel { RecipeId = 2, IngredientId = 2, Quantity = 50, Unit = "g" },
                    new RecipeDetailModel { RecipeId = 3, IngredientId = 3, Quantity = 100, Unit = "g" },
                    new RecipeDetailModel { RecipeId = 4, IngredientId = 4, Quantity = 150, Unit = "g" },
                    new RecipeDetailModel { RecipeId = 1, IngredientId = 5, Quantity = 100, Unit = "ml" }
                };
            }
        }

        public class MockPaymentRepository : IRepository<PaymentModel>
        {
            private List<PaymentModel> _payments;

            public MockPaymentRepository() { }
            public MockPaymentRepository(List<PaymentModel> payments)
            {
                _payments = payments;
            }

            public List<PaymentModel> GetAll()
            {
                if (_payments != null) return _payments;
                return new List<PaymentModel>
                {
                    new PaymentModel { Id = 1, InvoiceId = 1, PaymentDate = DateTime.Now.AddDays(-9), Amount = 150000m, PaymentMethod = "Cash" },
                    new PaymentModel { Id = 2, InvoiceId = 2, PaymentDate = DateTime.Now.AddDays(-4), Amount = 200000m, PaymentMethod = "Credit Card" },
                    new PaymentModel { Id = 3, InvoiceId = 3, PaymentDate = DateTime.Now.AddDays(-1), Amount = 300000m, PaymentMethod = "Mobile" },
                    new PaymentModel { Id = 4, InvoiceId = 4, PaymentDate = DateTime.Now, Amount = 250000m, PaymentMethod = "Cash" },
                    new PaymentModel { Id = 5, InvoiceId = 5, PaymentDate = DateTime.Now, Amount = 180000m, PaymentMethod = "Bank Transfer" }
                };
            }
        }

        public class MockOrderToppingRepository : IRepository<OrderToppingModel>
        {
            private List<OrderToppingModel> _orderToppings;

            public MockOrderToppingRepository() { }
            public MockOrderToppingRepository(List<OrderToppingModel> orderToppings)
            {
                _orderToppings = orderToppings;
            }

            public List<OrderToppingModel> GetAll()
            {
                if (_orderToppings != null) return _orderToppings;
                return new List<OrderToppingModel>
                {
                    new OrderToppingModel { Id = 1, InvoiceDetailId = 1, ProductId = 2, Quantity = 1, UnitPrice = 5000m, IsFree = false },
                    new OrderToppingModel { Id = 2, InvoiceDetailId = 2, ProductId = 2, Quantity = 2, UnitPrice = 5000m, IsFree = true },
                    new OrderToppingModel { Id = 3, InvoiceDetailId = 3, ProductId = 2, Quantity = 1, UnitPrice = 5000m, IsFree = false },
                    new OrderToppingModel { Id = 4, InvoiceDetailId = 4, ProductId = 2, Quantity = 1, UnitPrice = 5000m, IsFree = true },
                    new OrderToppingModel { Id = 5, InvoiceDetailId = 5, ProductId = 2, Quantity = 2, UnitPrice = 5000m, IsFree = false }
                };
            }
        }

        public class MockSupplierRepository : IRepository<SupplierModel>
        {
            private List<SupplierModel> _suppliers;

            public MockSupplierRepository() { }
            public MockSupplierRepository(List<SupplierModel> suppliers)
            {
                _suppliers = suppliers;
            }

            public List<SupplierModel> GetAll()
            {
                if (_suppliers != null) return _suppliers;
                return new List<SupplierModel>
                {
                    new SupplierModel { Id = 1, Name = "Vinamilk", Email = "contact@vinamilk.com", Phone = "02412345678", Address = "Hanoi" },
                    new SupplierModel { Id = 2, Name = "SugarCo", Email = "info@sugarco.com", Phone = "02823456789", Address = "Ho Chi Minh" },
                    new SupplierModel { Id = 3, Name = "TeaFarm", Email = "sales@teafarm.com", Phone = "02334567890", Address = "Da Nang" },
                    new SupplierModel { Id = 4, Name = "CoffeeBean", Email = "support@coffeebean.com", Phone = "02545678901", Address = "Can Tho" },
                    new SupplierModel { Id = 5, Name = "FruitPuree", Email = "order@fruitpuree.com", Phone = "02756789012", Address = "Hai Phong" }
                };
            }
        }

        public class MockProductRepository : IRepository<ProductModel>
        {
            private List<ProductModel> _products;

            public MockProductRepository() { }
            public MockProductRepository(List<ProductModel> products)
            {
                _products = products;
            }

            public List<ProductModel> GetAll()
            {
                if (_products != null) return _products;
                return new List<ProductModel>
                {
                    new ProductModel { Id = 1, Name = "Black Coffee", Price = 35000f, Cost = 21000f, IsActive = true, CategoryId = 1, ImageUrl = "kohi_logo.png", InvoiceDetails = new List<InvoiceDetailModel>() },
                    new ProductModel { Id = 2, Name = "Milk Tea", Price = 40000f, Cost = 24000f, IsActive = true, CategoryId = 2, ImageUrl = "kohi_logo.png", InvoiceDetails = new List<InvoiceDetailModel>() },
                    new ProductModel { Id = 3, Name = "Green Tea", Price = 30000f, Cost = 18000f, IsActive = true, CategoryId = 3, ImageUrl = "kohi_logo.png", InvoiceDetails = new List<InvoiceDetailModel>() },
                    new ProductModel { Id = 4, Name = "Mango Smoothie", Price = 45000f, Cost = 27000f, IsActive = true, CategoryId = 4, ImageUrl = "kohi_logo.png", InvoiceDetails = new List<InvoiceDetailModel>() },
                    new ProductModel { Id = 5, Name = "Espresso", Price = 40000f, Cost = 24000f, IsActive = true, CategoryId = 1, ImageUrl = "kohi_logo.png", InvoiceDetails = new List<InvoiceDetailModel>() }
                };
            }
        }

        public class MockOutboundRepository : IRepository<OutboundModel>
        {
            private List<OutboundModel> _outbounds;

            public MockOutboundRepository() { }
            public MockOutboundRepository(List<OutboundModel> outbounds)
            {
                _outbounds = outbounds;
            }

            public List<OutboundModel> GetAll()
            {
                if (_outbounds != null) return _outbounds;
                return new List<OutboundModel>
                {
                    new OutboundModel { Id = 1, InventoryId = 1, Quantity = 10, OutboundDate = DateTime.Now.AddDays(-14), Purpose = "Production", Notes = "For black coffee" },
                    new OutboundModel { Id = 2, InventoryId = 2, Quantity = 5, OutboundDate = DateTime.Now.AddDays(-9), Purpose = "Production", Notes = "For milk tea" },
                    new OutboundModel { Id = 3, InventoryId = 3, Quantity = 8, OutboundDate = DateTime.Now.AddDays(-6), Purpose = "Production", Notes = "For green tea" },
                    new OutboundModel { Id = 4, InventoryId = 4, Quantity = 6, OutboundDate = DateTime.Now.AddDays(-4), Purpose = "Production", Notes = "For mango smoothie" },
                    new OutboundModel { Id = 5, InventoryId = 5, Quantity = 7, OutboundDate = DateTime.Now.AddDays(-2), Purpose = "Production", Notes = "For espresso" }
                };
            }
        }

        public class MockRecipeRepository : IRepository<RecipeModel>
        {
            private List<RecipeModel> _recipes;

            public MockRecipeRepository() { }
            public MockRecipeRepository(List<RecipeModel> recipes)
            {
                _recipes = recipes;
            }

            public List<RecipeModel> GetAll()
            {
                if (_recipes != null) return _recipes;
                return new List<RecipeModel>
                {
                    new RecipeModel { Id = 1, ProductId = 1, Ingredients = new List<RecipeDetailModel>() },
                    new RecipeModel { Id = 2, ProductId = 2, Ingredients = new List<RecipeDetailModel>() },
                    new RecipeModel { Id = 3, ProductId = 3, Ingredients = new List<RecipeDetailModel>() },
                    new RecipeModel { Id = 4, ProductId = 4, Ingredients = new List<RecipeDetailModel>() },
                    new RecipeModel { Id = 5, ProductId = 5, Ingredients = new List<RecipeDetailModel>() }
                };
            }
        }

        public class MockTaxRepository : IRepository<TaxModel>
        {
            private List<TaxModel> _taxes;

            public MockTaxRepository() { }
            public MockTaxRepository(List<TaxModel> taxes)
            {
                _taxes = taxes;
            }

            public List<TaxModel> GetAll()
            {
                if (_taxes != null) return _taxes;
                return new List<TaxModel>
                {
                    new TaxModel { Id = 1, TaxName = "VAT 10%", TaxRate = 0.10m, InvoiceTaxes = new List<InvoiceTaxModel>() },
                    new TaxModel { Id = 2, TaxName = "Service Tax 5%", TaxRate = 0.05m, InvoiceTaxes = new List<InvoiceTaxModel>() },
                    new TaxModel { Id = 3, TaxName = "Special Tax 2%", TaxRate = 0.02m, InvoiceTaxes = new List<InvoiceTaxModel>() },
                    new TaxModel { Id = 4, TaxName = "No Tax", TaxRate = 0.00m, InvoiceTaxes = new List<InvoiceTaxModel>() },
                    new TaxModel { Id = 5, TaxName = "VAT 8%", TaxRate = 0.08m, InvoiceTaxes = new List<InvoiceTaxModel>() }
                };
            }
        }

        public class MockInvoiceTaxRepository : IRepository<InvoiceTaxModel>
        {
            private List<InvoiceTaxModel> _invoiceTaxes;

            public MockInvoiceTaxRepository() { }
            public MockInvoiceTaxRepository(List<InvoiceTaxModel> invoiceTaxes)
            {
                _invoiceTaxes = invoiceTaxes;
            }

            public List<InvoiceTaxModel> GetAll()
            {
                if (_invoiceTaxes != null) return _invoiceTaxes;
                return new List<InvoiceTaxModel>
                {
                    new InvoiceTaxModel { Id = 1, InvoiceId = 1, TaxId = 1 },
                    new InvoiceTaxModel { Id = 2, InvoiceId = 2, TaxId = 2 },
                    new InvoiceTaxModel { Id = 3, InvoiceId = 3, TaxId = 3 },
                    new InvoiceTaxModel { Id = 4, InvoiceId = 4, TaxId = 4 },
                    new InvoiceTaxModel { Id = 5, InvoiceId = 5, TaxId = 5 }
                };
            }
        }

        // Constructor để nối Navigation Properties
        public MockDao()
        {
            // Khởi tạo dữ liệu độc lập
            var categories = new MockCategoryRepository().GetAll();
            var customers = new MockCustomerRepository().GetAll();
            var expenseCategories = new MockExpenseCategoryRepository().GetAll();
            var expenses = new MockExpenseRepository().GetAll();
            var inbounds = new MockInboundRepository().GetAll();
            var expenseTypes = new MockExpenseTypeRepository().GetAll();
            var invoices = new MockInvoiceRepository().GetAll();
            var inventories = new MockInventoryRepository().GetAll();
            var ingredients = new MockIngredientRepository().GetAll();
            var invoiceDetails = new MockInvoiceDetailRepository().GetAll();
            var recipeDetails = new MockRecipeDetailRepository().GetAll();
            var payments = new MockPaymentRepository().GetAll();
            var orderToppings = new MockOrderToppingRepository().GetAll();
            var suppliers = new MockSupplierRepository().GetAll();
            var products = new MockProductRepository().GetAll();
            var outbounds = new MockOutboundRepository().GetAll();
            var recipes = new MockRecipeRepository().GetAll();
            var taxes = new MockTaxRepository().GetAll();
            var invoiceTaxes = new MockInvoiceTaxRepository().GetAll();

            // Nối Navigation Properties

            // CategoryModel - ProductModel (1-n)
            foreach (var category in categories)
            {
                category.Products = products.Where(p => p.CategoryId == category.Id).ToList();
                foreach (var product in category.Products)
                {
                    product.Category = category;
                }
            }

            // CustomerModel - InvoiceModel (1-n)
            foreach (var customer in customers)
            {
                customer.Invoices = invoices.Where(i => i.CustomerId == customer.Id).ToList();
                foreach (var invoice in customer.Invoices)
                {
                    invoice.Customer = customer;
                }
            }

            // ExpenseCategoryModel - ExpenseModel (1-n)
            foreach (var expenseCategory in expenseCategories)
            {
                expenseCategory.Expenses = expenses.Where(e => e.ExpenseCategoryId == expenseCategory.Id).ToList();
                foreach (var expense in expenseCategory.Expenses)
                {
                    expense.ExpenseCategory = expenseCategory;
                }
            }

            // ExpenseTypeModel - ExpenseCategoryModel (1-n)
            foreach (var expenseType in expenseTypes)
            {
                expenseType.ExpenseCategories = expenseCategories.Where(ec => ec.ExpenseTypeId == expenseType.Id).ToList();
                foreach (var expenseCategory in expenseType.ExpenseCategories)
                {
                    expenseCategory.ExpenseType = expenseType;
                }
            }

            // InboundModel - IngredientModel, SupplierModel (1-1)
            foreach (var inbound in inbounds)
            {
                inbound.Ingredient = ingredients.FirstOrDefault(i => i.Id == inbound.IngredientId);
                inbound.Supplier = suppliers.FirstOrDefault(s => s.Id == inbound.SupplierId);
            }

            // InvoiceModel - InvoiceDetailModel (1-n)
            foreach (var invoice in invoices)
            {
                invoice.InvoiceDetails = invoiceDetails.Where(id => id.InvoiceId == invoice.Id).ToList();
                foreach (var invoiceDetail in invoice.InvoiceDetails)
                {
                    invoiceDetail.Invoice = invoice;
                }
            }

            // InventoryModel - InboundModel, IngredientModel, SupplierModel (1-1)
            foreach (var inventory in inventories)
            {
                inventory.Inbound = inbounds.FirstOrDefault(ib => ib.Id == inventory.InboundId);
                inventory.Ingredient = ingredients.FirstOrDefault(i => i.Id == inventory.IngredientId);
                inventory.Supplier = suppliers.FirstOrDefault(s => s.Id == inventory.SupplierId);
            }

            // IngredientModel - SupplierModel (1-1)
            foreach (var ingredient in ingredients)
            {
                ingredient.Supplier = suppliers.FirstOrDefault(s => s.Id == ingredient.SupplierId);
            }

            // InvoiceDetailModel - ProductModel (1-1), OrderToppingModel (1-n)
            foreach (var invoiceDetail in invoiceDetails)
            {
                invoiceDetail.Product = products.FirstOrDefault(p => p.Id == invoiceDetail.ProductId);
                invoiceDetail.Toppings = orderToppings.Where(ot => ot.InvoiceDetailId == invoiceDetail.Id).ToList();
                foreach (var topping in invoiceDetail.Toppings)
                {
                    topping.InvoiceDetail = invoiceDetail;
                }
            }

            // ProductModel - InvoiceDetailModel (1-n)
            foreach (var product in products)
            {
                product.InvoiceDetails = invoiceDetails.Where(id => id.ProductId == product.Id).ToList();
                foreach (var invoiceDetail in product.InvoiceDetails)
                {
                    invoiceDetail.Product = product;
                }
            }

            // RecipeDetailModel - RecipeModel, IngredientModel (n-n)
            foreach (var recipeDetail in recipeDetails)
            {
                recipeDetail.Recipe = recipes.FirstOrDefault(r => r.Id == recipeDetail.RecipeId);
                recipeDetail.Ingredient = ingredients.FirstOrDefault(i => i.Id == recipeDetail.IngredientId);
            }

            // RecipeModel - RecipeDetailModel (1-n)
            foreach (var recipe in recipes)
            {
                recipe.Ingredients = recipeDetails.Where(rd => rd.RecipeId == recipe.Id).ToList();
                recipe.Product = products.FirstOrDefault(p => p.Id == recipe.ProductId);
            }

            // PaymentModel - InvoiceModel (1-1)
            foreach (var payment in payments)
            {
                payment.Invoice = invoices.FirstOrDefault(i => i.Id == payment.InvoiceId);
            }

            // OrderToppingModel - ProductModel (1-1)
            foreach (var orderTopping in orderToppings)
            {
                orderTopping.Product = products.FirstOrDefault(p => p.Id == orderTopping.ProductId);
            }

            // OutboundModel - InventoryModel (1-1)
            foreach (var outbound in outbounds)
            {
                outbound.Inventory = inventories.FirstOrDefault(iv => iv.Id == outbound.InventoryId);
            }

            // TaxModel - InvoiceTaxModel (1-n)
            foreach (var tax in taxes)
            {
                tax.InvoiceTaxes = invoiceTaxes.Where(it => it.TaxId == tax.Id).ToList();
                foreach (var invoiceTax in tax.InvoiceTaxes)
                {
                    invoiceTax.Tax = tax;
                }
            }

            // InvoiceTaxModel - InvoiceModel (1-1)
            foreach (var invoiceTax in invoiceTaxes)
            {
                invoiceTax.Invoice = invoices.FirstOrDefault(i => i.Id == invoiceTax.InvoiceId);
            }

            // Gán lại các repository với dữ liệu đã nối
            Categories = new MockCategoryRepository(categories);
            Customers = new MockCustomerRepository(customers);
            ExpenseCategories = new MockExpenseCategoryRepository(expenseCategories);
            Expenses = new MockExpenseRepository(expenses);
            Inbounds = new MockInboundRepository(inbounds);
            ExpenseTypes = new MockExpenseTypeRepository(expenseTypes);
            Invoices = new MockInvoiceRepository(invoices);
            Inventories = new MockInventoryRepository(inventories);
            Ingredients = new MockIngredientRepository(ingredients);
            InvoiceDetails = new MockInvoiceDetailRepository(invoiceDetails);
            RecipeDetails = new MockRecipeDetailRepository(recipeDetails);
            Payments = new MockPaymentRepository(payments);
            OrderToppings = new MockOrderToppingRepository(orderToppings);
            Suppliers = new MockSupplierRepository(suppliers);
            Products = new MockProductRepository(products);
            Outbounds = new MockOutboundRepository(outbounds);
            Recipes = new MockRecipeRepository(recipes);
            Taxes = new MockTaxRepository(taxes);
            InvoiceTaxes = new MockInvoiceTaxRepository(invoiceTaxes);
        }

        public IRepository<ProductModel> Products { get; set; }
        public IRepository<CategoryModel> Categories { get; set; }
        public IRepository<CustomerModel> Customers { get; set; }
        public IRepository<ExpenseCategoryModel> ExpenseCategories { get; set; }
        public IRepository<ExpenseModel> Expenses { get; set; }
        public IRepository<InboundModel> Inbounds { get; set; }
        public IRepository<ExpenseTypeModel> ExpenseTypes { get; set; }
        public IRepository<InvoiceModel> Invoices { get; set; }
        public IRepository<InventoryModel> Inventories { get; set; }
        public IRepository<IngredientModel> Ingredients { get; set; }
        public IRepository<InvoiceDetailModel> InvoiceDetails { get; set; }
        public IRepository<RecipeDetailModel> RecipeDetails { get; set; }
        public IRepository<PaymentModel> Payments { get; set; }
        public IRepository<OrderToppingModel> OrderToppings { get; set; }
        public IRepository<SupplierModel> Suppliers { get; set; }
        public IRepository<OutboundModel> Outbounds { get; set; }
        public IRepository<RecipeModel> Recipes { get; set; }
        public IRepository<TaxModel> Taxes { get; set; }
        public IRepository<InvoiceTaxModel> InvoiceTaxes { get; set; }
    }
}