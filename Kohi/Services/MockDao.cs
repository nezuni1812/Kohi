using System;
using System.Collections.Generic;
using System.Linq;
using Kohi.Models;

namespace Kohi.Services
{
    public class MockDao : IDao
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

            public List<CategoryModel> GetAll(
                int pageNumber = 1,
                int pageSize = 10,
                string sortBy = null,
                bool sortDescending = false,
                string filterField = null,
                string filterValue = null,
                string searchKeyword = null)
            {
                var data = _categories ?? new List<CategoryModel>
                {
                    new CategoryModel { Id = 1, Name = "Cà phê", ImageUrl = "kohi_logo.png", Products = new List<ProductModel>() },
                    new CategoryModel { Id = 2, Name = "Trà sữa", ImageUrl = "kohi_logo.png", Products = new List<ProductModel>() },
                    new CategoryModel { Id = 3, Name = "Trà", ImageUrl = "kohi_logo.png", Products = new List<ProductModel>() },
                    new CategoryModel { Id = 4, Name = "Đá xay", ImageUrl = "kohi_logo.png", Products = new List<ProductModel>() }
                };

                return ApplyQuery(data, pageNumber, pageSize, sortBy, sortDescending, filterField, filterValue, searchKeyword);
            }

            public CategoryModel GetById(string id)
            {
                if (!int.TryParse(id, out int intId)) return null;
                var data = _categories ?? GetAll(); // Nếu chưa có dữ liệu, lấy mặc định
                return data.FirstOrDefault(c => c.Id == intId);
            }

            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var data = _categories ?? GetAll();
                var item = data.FirstOrDefault(c => c.Id == intId);
                if (item == null) return 0;
                data.Remove(item);
                _categories = data; // Cập nhật lại danh sách
                return 1;
            }

            public int UpdateById(string id, CategoryModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data = _categories ?? GetAll();
                var item = data.FirstOrDefault(c => c.Id == intId);
                if (item == null) return 0;
                item.Name = info.Name;
                item.ImageUrl = info.ImageUrl;
                item.Products = info.Products; // Cập nhật navigation property nếu cần
                _categories = data;
                return 1;
            }

            public int Insert(string id, CategoryModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data = _categories ?? GetAll();
                if (data.Any(c => c.Id == intId)) return 0; // Không cho phép trùng Id
                info.Id = intId;
                data.Add(info);
                _categories = data;
                return 1;
            }
            public int GetCount(string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                var data = _categories ?? GetAll();
                return ApplyCountQuery(data, filterField, filterValue, searchKeyword);
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

            public List<CustomerModel> GetAll(
                int pageNumber = 1,
                int pageSize = 10,
                string sortBy = null,
                bool sortDescending = false,
                string filterField = null,
                string filterValue = null,
                string searchKeyword = null)
            {
                var data = _customers ?? new List<CustomerModel>
                {
                    new CustomerModel { Id = 1, Name = "Nguyen Van A", Email = "a@example.com", Phone = "0901234567", Address = "Hanoi", Invoices = new List<InvoiceModel>() },
                    new CustomerModel { Id = 2, Name = "Tran Thi B", Email = "b@example.com", Phone = "0912345678", Address = "Ho Chi Minh", Invoices = new List<InvoiceModel>() },
                    new CustomerModel { Id = 3, Name = "Le Van C", Email = "c@example.com", Phone = "0923456789", Address = "Da Nang", Invoices = new List<InvoiceModel>() },
                    new CustomerModel { Id = 4, Name = "Pham Thi D", Email = "d@example.com", Phone = "0934567890", Address = "Can Tho", Invoices = new List<InvoiceModel>() },
                    new CustomerModel { Id = 5, Name = "Hoang Van E", Email = "e@example.com", Phone = "0945678901", Address = "Hai Phong", Invoices = new List<InvoiceModel>() }
                };

                return ApplyQuery(data, pageNumber, pageSize, sortBy, sortDescending, filterField, filterValue, searchKeyword);
            }

            public CustomerModel GetById(string id)
            {
                if (!int.TryParse(id, out int intId)) return null;
                var data = _customers ?? GetAll();
                return data.FirstOrDefault(c => c.Id == intId);
            }

            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var data = _customers ?? GetAll();
                var item = data.FirstOrDefault(c => c.Id == intId);
                if (item == null) return 0;
                data.Remove(item);
                _customers = data;
                return 1;
            }

            public int UpdateById(string id, CustomerModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data = _customers ?? GetAll();
                var item = data.FirstOrDefault(c => c.Id == intId);
                if (item == null) return 0;
                item.Name = info.Name;
                item.Email = info.Email;
                item.Phone = info.Phone;
                item.Address = info.Address;
                item.Invoices = info.Invoices;
                _customers = data;
                return 1;
            }

            public int Insert(string id, CustomerModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data = _customers ?? GetAll();
                if (data.Any(c => c.Id == intId)) return 0;
                info.Id = intId;
                data.Add(info);
                _customers = data;
                return 1;
            }
            public int GetCount(string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                var data = _customers ?? GetAll();
                return ApplyCountQuery(data, filterField, filterValue, searchKeyword);
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

            public List<ExpenseCategoryModel> GetAll(
                int pageNumber = 1,
                int pageSize = 10,
                string sortBy = null,
                bool sortDescending = false,
                string filterField = null,
                string filterValue = null,
                string searchKeyword = null)
            {
                var data = _expenseCategories ?? new List<ExpenseCategoryModel>
                {
                    new ExpenseCategoryModel { Id = 1, CategoryName = "Raw Materials", ExpenseTypeId = 1, Description = "Cost of ingredients", Expenses = new List<ExpenseModel>() },
                    new ExpenseCategoryModel { Id = 2, CategoryName = "Utilities", ExpenseTypeId = 2, Description = "Electricity and water", Expenses = new List<ExpenseModel>() },
                    new ExpenseCategoryModel { Id = 3, CategoryName = "Staff", ExpenseTypeId = 1, Description = "Employee salaries", Expenses = new List<ExpenseModel>() },
                    new ExpenseCategoryModel { Id = 4, CategoryName = "Marketing", ExpenseTypeId = 3, Description = "Advertising costs", Expenses = new List<ExpenseModel>() },
                    new ExpenseCategoryModel { Id = 5, CategoryName = "Equipment", ExpenseTypeId = 2, Description = "Machine maintenance", Expenses = new List<ExpenseModel>() }
                };

                return ApplyQuery(data, pageNumber, pageSize, sortBy, sortDescending, filterField, filterValue, searchKeyword);
            }

            public ExpenseCategoryModel GetById(string id)
            {
                if (!int.TryParse(id, out int intId)) return null;
                var data = _expenseCategories ?? GetAll();
                return data.FirstOrDefault(c => c.Id == intId);
            }

            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var data = _expenseCategories ?? GetAll();
                var item = data.FirstOrDefault(c => c.Id == intId);
                if (item == null) return 0;
                data.Remove(item);
                _expenseCategories = data;
                return 1;
            }

            public int UpdateById(string id, ExpenseCategoryModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data = _expenseCategories ?? GetAll();
                var item = data.FirstOrDefault(c => c.Id == intId);
                if (item == null) return 0;
                item.CategoryName = info.CategoryName;
                item.ExpenseTypeId = info.ExpenseTypeId;
                item.Description = info.Description;
                item.Expenses = info.Expenses;
                _expenseCategories = data;
                return 1;
            }

            public int Insert(string id, ExpenseCategoryModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data = _expenseCategories ?? GetAll();
                if (data.Any(c => c.Id == intId)) return 0;
                info.Id = intId;
                data.Add(info);
                _expenseCategories = data;
                return 1;
            }
            public int GetCount(string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                var data = _expenseCategories ?? GetAll();
                return ApplyCountQuery(data, filterField, filterValue, searchKeyword);
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

            public List<ExpenseModel> GetAll(
                int pageNumber = 1,
                int pageSize = 10,
                string sortBy = null,
                bool sortDescending = false,
                string filterField = null,
                string filterValue = null,
                string searchKeyword = null)
            {
                var data = _expenses ?? new List<ExpenseModel>
                {
                    new ExpenseModel { Id = 1, ExpenseCategoryId = 1, Amount = 5000000f, ExpenseDate = DateTime.Now.AddDays(-10) },
                    new ExpenseModel { Id = 2, ExpenseCategoryId = 2, Amount = 2000000f, ExpenseDate = DateTime.Now.AddDays(-5) },
                    new ExpenseModel { Id = 3, ExpenseCategoryId = 3, Amount = 10000000f, ExpenseDate = DateTime.Now.AddDays(-2) },
                    new ExpenseModel { Id = 4, ExpenseCategoryId = 4, Amount = 3000000f, ExpenseDate = DateTime.Now.AddDays(-1) },
                    new ExpenseModel { Id = 5, ExpenseCategoryId = 5, Amount = 4000000f, ExpenseDate = DateTime.Now }
                };

                return ApplyQuery(data, pageNumber, pageSize, sortBy, sortDescending, filterField, filterValue, searchKeyword);
            }

            public ExpenseModel GetById(string id)
            {
                if (!int.TryParse(id, out int intId)) return null;
                var data = _expenses ?? GetAll();
                return data.FirstOrDefault(e => e.Id == intId);
            }

            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var data = _expenses ?? GetAll();
                var item = data.FirstOrDefault(e => e.Id == intId);
                if (item == null) return 0;
                data.Remove(item);
                _expenses = data;
                return 1;
            }

            public int UpdateById(string id, ExpenseModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data = _expenses ?? GetAll();
                var item = data.FirstOrDefault(e => e.Id == intId);
                if (item == null) return 0;
                item.ExpenseCategoryId = info.ExpenseCategoryId;
                item.Amount = info.Amount;
                item.ExpenseDate = info.ExpenseDate;
                _expenses = data;
                return 1;
            }

            public int Insert(string id, ExpenseModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data = _expenses ?? GetAll();
                if (data.Any(e => e.Id == intId)) return 0;
                info.Id = intId;
                data.Add(info);
                _expenses = data;
                return 1;
            }
            public int GetCount(string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                var data = _expenses ?? GetAll();
                return ApplyCountQuery(data, filterField, filterValue, searchKeyword);
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

            public List<InboundModel> GetAll(
                int pageNumber = 1,
                int pageSize = 10,
                string sortBy = null,
                bool sortDescending = false,
                string filterField = null,
                string filterValue = null,
                string searchKeyword = null)
            {
                var data = _inbounds ?? new List<InboundModel>
                {
                    new InboundModel { Id = 1, IngredientId = 1, Quantity = 100, InboundDate = DateTime.Now.AddDays(-15), ExpiryDate = DateTime.Now.AddMonths(6), SupplierId = 1, Notes = "Fresh milk batch" },
                    new InboundModel { Id = 2, IngredientId = 2, Quantity = 50, InboundDate = DateTime.Now.AddDays(-10), ExpiryDate = DateTime.Now.AddMonths(5), SupplierId = 2, Notes = "Sugar supply" },
                    new InboundModel { Id = 3, IngredientId = 3, Quantity = 80, InboundDate = DateTime.Now.AddDays(-7), ExpiryDate = DateTime.Now.AddMonths(4), SupplierId = 3, Notes = "Tea leaves" },
                    new InboundModel { Id = 4, IngredientId = 4, Quantity = 60, InboundDate = DateTime.Now.AddDays(-5), ExpiryDate = DateTime.Now.AddMonths(3), SupplierId = 4, Notes = "Coffee beans" },
                    new InboundModel { Id = 5, IngredientId = 5, Quantity = 70, InboundDate = DateTime.Now.AddDays(-3), ExpiryDate = DateTime.Now.AddMonths(2), SupplierId = 5, Notes = "Fruit puree" }
                };

                return ApplyQuery(data, pageNumber, pageSize, sortBy, sortDescending, filterField, filterValue, searchKeyword);
            }

            public InboundModel GetById(string id)
            {
                if (!int.TryParse(id, out int intId)) return null;
                var data = _inbounds ?? GetAll();
                return data.FirstOrDefault(i => i.Id == intId);
            }

            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var data = _inbounds ?? GetAll();
                var item = data.FirstOrDefault(i => i.Id == intId);
                if (item == null) return 0;
                data.Remove(item);
                _inbounds = data;
                return 1;
            }

            public int UpdateById(string id, InboundModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data = _inbounds ?? GetAll();
                var item = data.FirstOrDefault(i => i.Id == intId);
                if (item == null) return 0;
                item.IngredientId = info.IngredientId;
                item.Quantity = info.Quantity;
                item.InboundDate = info.InboundDate;
                item.ExpiryDate = info.ExpiryDate;
                item.SupplierId = info.SupplierId;
                item.Notes = info.Notes;
                _inbounds = data;
                return 1;
            }

            public int Insert(string id, InboundModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data = _inbounds ?? GetAll();
                if (data.Any(i => i.Id == intId)) return 0;
                info.Id = intId;
                data.Add(info);
                _inbounds = data;
                return 1;
            }
            public int GetCount(string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                var data = _inbounds ?? GetAll();
                return ApplyCountQuery(data, filterField, filterValue, searchKeyword);
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

            public List<InvoiceModel> GetAll(
                int pageNumber = 1,
                int pageSize = 10,
                string sortBy = null,
                bool sortDescending = false,
                string filterField = null,
                string filterValue = null,
                string searchKeyword = null)
            {
                var data = _invoices ?? new List<InvoiceModel>
                {
                    new InvoiceModel { Id = 1, CustomerId = 1, InvoiceDate = DateTime.Now.AddDays(-10), TotalAmount = 150000m, InvoiceDetails = new List<InvoiceDetailModel>() },
                    new InvoiceModel { Id = 2, CustomerId = 2, InvoiceDate = DateTime.Now.AddDays(-5), TotalAmount = 200000m, InvoiceDetails = new List<InvoiceDetailModel>() },
                    new InvoiceModel { Id = 3, CustomerId = 3, InvoiceDate = DateTime.Now.AddDays(-2), TotalAmount = 300000m, InvoiceDetails = new List<InvoiceDetailModel>() },
                    new InvoiceModel { Id = 4, CustomerId = 4, InvoiceDate = DateTime.Now.AddDays(-1), TotalAmount = 250000m, InvoiceDetails = new List<InvoiceDetailModel>() },
                    new InvoiceModel { Id = 5, CustomerId = 5, InvoiceDate = DateTime.Now, TotalAmount = 180000m, InvoiceDetails = new List<InvoiceDetailModel>() }
                };

                return ApplyQuery(data, pageNumber, pageSize, sortBy, sortDescending, filterField, filterValue, searchKeyword);
            }

            public InvoiceModel GetById(string id)
            {
                if (!int.TryParse(id, out int intId)) return null;
                var data = _invoices ?? GetAll();
                return data.FirstOrDefault(i => i.Id == intId);
            }

            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var data = _invoices ?? GetAll();
                var item = data.FirstOrDefault(i => i.Id == intId);
                if (item == null) return 0;
                data.Remove(item);
                _invoices = data;
                return 1;
            }

            public int UpdateById(string id, InvoiceModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data = _invoices ?? GetAll();
                var item = data.FirstOrDefault(i => i.Id == intId);
                if (item == null) return 0;
                item.CustomerId = info.CustomerId;
                item.InvoiceDate = info.InvoiceDate;
                item.TotalAmount = info.TotalAmount;
                item.InvoiceDetails = info.InvoiceDetails;
                _invoices = data;
                return 1;
            }

            public int Insert(string id, InvoiceModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data = _invoices ?? GetAll();
                if (data.Any(i => i.Id == intId)) return 0;
                info.Id = intId;
                data.Add(info);
                _invoices = data;
                return 1;
            }
            public int GetCount(string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                var data = _invoices ?? GetAll();
                return ApplyCountQuery(data, filterField, filterValue, searchKeyword);
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

            public List<InventoryModel> GetAll(
                int pageNumber = 1,
                int pageSize = 10,
                string sortBy = null,
                bool sortDescending = false,
                string filterField = null,
                string filterValue = null,
                string searchKeyword = null)
            {
                var data = _inventories ?? new List<InventoryModel>
                {
                    new InventoryModel { Id = 1, InboundId = 1, Quantity = 90,  InboundDate = DateTime.Now.AddDays(-15), ExpiryDate = DateTime.Now.AddMonths(6)},
                    new InventoryModel { Id = 2, InboundId = 2, Quantity = 45,  InboundDate = DateTime.Now.AddDays(-10), ExpiryDate = DateTime.Now.AddMonths(5)},
                    new InventoryModel { Id = 3, InboundId = 3, Quantity = 75,  InboundDate = DateTime.Now.AddDays(-7), ExpiryDate = DateTime.Now.AddMonths(4)},
                    new InventoryModel { Id = 4, InboundId = 4, Quantity = 55,  InboundDate = DateTime.Now.AddDays(-5), ExpiryDate = DateTime.Now.AddMonths(3)},
                    new InventoryModel { Id = 5, InboundId = 5, Quantity = 65,  InboundDate = DateTime.Now.AddDays(-3), ExpiryDate = DateTime.Now.AddMonths(2)}
                };

                return ApplyQuery(data, pageNumber, pageSize, sortBy, sortDescending, filterField, filterValue, searchKeyword);
            }

            public InventoryModel GetById(string id)
            {
                if (!int.TryParse(id, out int intId)) return null;
                var data = _inventories ?? GetAll();
                return data.FirstOrDefault(i => i.Id == intId);
            }

            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var data = _inventories ?? GetAll();
                var item = data.FirstOrDefault(i => i.Id == intId);
                if (item == null) return 0;
                data.Remove(item);
                _inventories = data;
                return 1;
            }

            public int UpdateById(string id, InventoryModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data = _inventories ?? GetAll();
                var item = data.FirstOrDefault(i => i.Id == intId);
                if (item == null) return 0;
                item.InboundId = info.InboundId;
                item.Quantity = info.Quantity;
                item.InboundDate = info.InboundDate;
                item.ExpiryDate = info.ExpiryDate;
                _inventories = data;
                return 1;
            }

            public int Insert(string id, InventoryModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data = _inventories ?? GetAll();
                if (data.Any(i => i.Id == intId)) return 0;
                info.Id = intId;
                data.Add(info);
                _inventories = data;
                return 1;
            }
            public int GetCount(string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                var data = _inventories ?? GetAll();
                return ApplyCountQuery(data, filterField, filterValue, searchKeyword);
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

            public List<IngredientModel> GetAll(
                int pageNumber = 1,
                int pageSize = 10,
                string sortBy = null,
                bool sortDescending = false,
                string filterField = null,
                string filterValue = null,
                string searchKeyword = null)
            {
                var data = _ingredients ?? new List<IngredientModel>
                {
                    new IngredientModel { Id = 1, Name = "Milk", Unit = "Liter", CostPerUnit = 20000f, Description = "Fresh milk" },
                    new IngredientModel { Id = 2, Name = "Sugar", Unit = "Kg", CostPerUnit = 15000f, Description = "Refined sugar" },
                    new IngredientModel { Id = 3, Name = "Tea Leaves", Unit = "Kg", CostPerUnit = 30000f, Description = "Green tea leaves" },
                    new IngredientModel { Id = 4, Name = "Coffee Beans", Unit = "Kg", CostPerUnit = 40000f, Description = "Arabica beans" },
                    new IngredientModel { Id = 5, Name = "Fruit Puree", Unit = "Liter", CostPerUnit = 25000f, Description = "Mixed fruit puree" }
                };

                return ApplyQuery(data, pageNumber, pageSize, sortBy, sortDescending, filterField, filterValue, searchKeyword);
            }

            public IngredientModel GetById(string id)
            {
                if (!int.TryParse(id, out int intId)) return null;
                var data = _ingredients ?? GetAll();
                return data.FirstOrDefault(i => i.Id == intId);
            }

            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var data = _ingredients ?? GetAll();
                var item = data.FirstOrDefault(i => i.Id == intId);
                if (item == null) return 0;
                data.Remove(item);
                _ingredients = data;
                return 1;
            }

            public int UpdateById(string id, IngredientModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data = _ingredients ?? GetAll();
                var item = data.FirstOrDefault(i => i.Id == intId);
                if (item == null) return 0;
                item.Name = info.Name;
                item.Unit = info.Unit;
                item.CostPerUnit = info.CostPerUnit;
                item.Description = info.Description;
                _ingredients = data;
                return 1;
            }

            public int Insert(string id, IngredientModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data = _ingredients ?? GetAll();
                if (data.Any(i => i.Id == intId)) return 0;
                info.Id = intId;
                data.Add(info);
                _ingredients = data;
                return 1;
            }
            public int GetCount(string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                var data = _ingredients ?? GetAll();
                return ApplyCountQuery(data, filterField, filterValue, searchKeyword);
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

            public List<InvoiceDetailModel> GetAll(
                int pageNumber = 1,
                int pageSize = 10,
                string sortBy = null,
                bool sortDescending = false,
                string filterField = null,
                string filterValue = null,
                string searchKeyword = null)
            {
                var data = _invoiceDetails ?? new List<InvoiceDetailModel>
                {
                    new InvoiceDetailModel { Id = 1, InvoiceId = 1, ProductId = 1, Quantity = 2, UnitPrice = 35000m, SugarLevel = 50, IceLevel = 70, Toppings = new List<OrderToppingModel>() },
                    new InvoiceDetailModel { Id = 2, InvoiceId = 2, ProductId = 2, Quantity = 1, UnitPrice = 40000m, SugarLevel = 30, IceLevel = 50, Toppings = new List<OrderToppingModel>() },
                    new InvoiceDetailModel { Id = 3, InvoiceId = 3, ProductId = 3, Quantity = 3, UnitPrice = 30000m, SugarLevel = 40, IceLevel = 60, Toppings = new List<OrderToppingModel>() },
                    new InvoiceDetailModel { Id = 4, InvoiceId = 4, ProductId = 1, Quantity = 1, UnitPrice = 35000m, SugarLevel = 60, IceLevel = 80, Toppings = new List<OrderToppingModel>() },
                    new InvoiceDetailModel { Id = 5, InvoiceId = 5, ProductId = 2, Quantity = 2, UnitPrice = 40000m, SugarLevel = 20, IceLevel = 40, Toppings = new List<OrderToppingModel>() }
                };

                return ApplyQuery(data, pageNumber, pageSize, sortBy, sortDescending, filterField, filterValue, searchKeyword);
            }

            public InvoiceDetailModel GetById(string id)
            {
                if (!int.TryParse(id, out int intId)) return null;
                var data = _invoiceDetails ?? GetAll();
                return data.FirstOrDefault(i => i.Id == intId);
            }

            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var data = _invoiceDetails ?? GetAll();
                var item = data.FirstOrDefault(i => i.Id == intId);
                if (item == null) return 0;
                data.Remove(item);
                _invoiceDetails = data;
                return 1;
            }

            public int UpdateById(string id, InvoiceDetailModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data = _invoiceDetails ?? GetAll();
                var item = data.FirstOrDefault(i => i.Id == intId);
                if (item == null) return 0;
                item.InvoiceId = info.InvoiceId;
                item.ProductId = info.ProductId;
                item.Quantity = info.Quantity;
                item.UnitPrice = info.UnitPrice;
                item.SugarLevel = info.SugarLevel;
                item.IceLevel = info.IceLevel;
                item.Toppings = info.Toppings;
                _invoiceDetails = data;
                return 1;
            }

            public int Insert(string id, InvoiceDetailModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data = _invoiceDetails ?? GetAll();
                if (data.Any(i => i.Id == intId)) return 0;
                info.Id = intId;
                data.Add(info);
                _invoiceDetails = data;
                return 1;
            }
            public int GetCount(string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                var data = _invoiceDetails ?? GetAll();
                return ApplyCountQuery(data, filterField, filterValue, searchKeyword);
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

            public List<RecipeDetailModel> GetAll(
                int pageNumber = 1,
                int pageSize = 10,
                string sortBy = null,
                bool sortDescending = false,
                string filterField = null,
                string filterValue = null,
                string searchKeyword = null)
            {
                var data = _recipeDetails ?? new List<RecipeDetailModel>
                {
                    new RecipeDetailModel { RecipeId = 1, IngredientId = 1, Quantity = 200, Unit = "ml" },
                    new RecipeDetailModel { RecipeId = 2, IngredientId = 2, Quantity = 50, Unit = "g" },
                    new RecipeDetailModel { RecipeId = 3, IngredientId = 3, Quantity = 100, Unit = "g" },
                    new RecipeDetailModel { RecipeId = 4, IngredientId = 4, Quantity = 150, Unit = "g" },
                    new RecipeDetailModel { RecipeId = 1, IngredientId = 5, Quantity = 100, Unit = "ml" }
                };

                return ApplyQuery(data, pageNumber, pageSize, sortBy, sortDescending, filterField, filterValue, searchKeyword);
            }

            public RecipeDetailModel GetById(string id)
            {
                if (!int.TryParse(id, out int intId)) return null;
                var data = _recipeDetails ?? GetAll();
                return data.FirstOrDefault(r => r.RecipeId == intId || r.IngredientId == intId); // Giả định RecipeId là khóa chính
            }

            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var data = _recipeDetails ?? GetAll();
                var item = data.FirstOrDefault(r => r.RecipeId == intId || r.IngredientId == intId);
                if (item == null) return 0;
                data.Remove(item);
                _recipeDetails = data;
                return 1;
            }

            public int UpdateById(string id, RecipeDetailModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data = _recipeDetails ?? GetAll();
                var item = data.FirstOrDefault(r => r.RecipeId == intId || r.IngredientId == intId);
                if (item == null) return 0;
                item.RecipeId = info.RecipeId;
                item.IngredientId = info.IngredientId;
                item.Quantity = info.Quantity;
                item.Unit = info.Unit;
                _recipeDetails = data;
                return 1;
            }

            public int Insert(string id, RecipeDetailModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data = _recipeDetails ?? GetAll();
                if (data.Any(r => r.RecipeId == info.RecipeId && r.IngredientId == info.IngredientId)) return 0; // Không trùng RecipeId và IngredientId
                data.Add(info);
                _recipeDetails = data;
                return 1;
            }
            public int GetCount(string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                var data = _recipeDetails ?? GetAll();
                return ApplyCountQuery(data, filterField, filterValue, searchKeyword);
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

            public List<PaymentModel> GetAll(
                int pageNumber = 1,
                int pageSize = 10,
                string sortBy = null,
                bool sortDescending = false,
                string filterField = null,
                string filterValue = null,
                string searchKeyword = null)
            {
                var data = _payments ?? new List<PaymentModel>
                {
                    new PaymentModel { Id = 1, InvoiceId = 1, PaymentDate = DateTime.Now.AddDays(-9), Amount = 150000m, PaymentMethod = "Cash" },
                    new PaymentModel { Id = 2, InvoiceId = 2, PaymentDate = DateTime.Now.AddDays(-4), Amount = 200000m, PaymentMethod = "Credit Card" },
                    new PaymentModel { Id = 3, InvoiceId = 3, PaymentDate = DateTime.Now.AddDays(-1), Amount = 300000m, PaymentMethod = "Mobile" },
                    new PaymentModel { Id = 4, InvoiceId = 4, PaymentDate = DateTime.Now, Amount = 250000m, PaymentMethod = "Cash" },
                    new PaymentModel { Id = 5, InvoiceId = 5, PaymentDate = DateTime.Now, Amount = 180000m, PaymentMethod = "Bank Transfer" }
                };

                return ApplyQuery(data, pageNumber, pageSize, sortBy, sortDescending, filterField, filterValue, searchKeyword);
            }

            public PaymentModel GetById(string id)
            {
                if (!int.TryParse(id, out int intId)) return null;
                var data = _payments ?? GetAll();
                return data.FirstOrDefault(p => p.Id == intId);
            }

            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var data = _payments ?? GetAll();
                var item = data.FirstOrDefault(p => p.Id == intId);
                if (item == null) return 0;
                data.Remove(item);
                _payments = data;
                return 1;
            }

            public int UpdateById(string id, PaymentModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data = _payments ?? GetAll();
                var item = data.FirstOrDefault(p => p.Id == intId);
                if (item == null) return 0;
                item.InvoiceId = info.InvoiceId;
                item.PaymentDate = info.PaymentDate;
                item.Amount = info.Amount;
                item.PaymentMethod = info.PaymentMethod;
                _payments = data;
                return 1;
            }

            public int Insert(string id, PaymentModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data = _payments ?? GetAll();
                if (data.Any(p => p.Id == intId)) return 0;
                info.Id = intId;
                data.Add(info);
                _payments = data;
                return 1;
            }
            public int GetCount(string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                var data = _payments ?? GetAll();
                return ApplyCountQuery(data, filterField, filterValue, searchKeyword);
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

            public List<OrderToppingModel> GetAll(
                int pageNumber = 1,
                int pageSize = 10,
                string sortBy = null,
                bool sortDescending = false,
                string filterField = null,
                string filterValue = null,
                string searchKeyword = null)
            {
                var data = _orderToppings ?? new List<OrderToppingModel>
                {
                    new OrderToppingModel { Id = 1, InvoiceDetailId = 1, ProductId = 2, Quantity = 1, UnitPrice = 5000m, IsFree = false },
                    new OrderToppingModel { Id = 2, InvoiceDetailId = 2, ProductId = 2, Quantity = 2, UnitPrice = 5000m, IsFree = true },
                    new OrderToppingModel { Id = 3, InvoiceDetailId = 3, ProductId = 2, Quantity = 1, UnitPrice = 5000m, IsFree = false },
                    new OrderToppingModel { Id = 4, InvoiceDetailId = 4, ProductId = 2, Quantity = 1, UnitPrice = 5000m, IsFree = true },
                    new OrderToppingModel { Id = 5, InvoiceDetailId = 5, ProductId = 2, Quantity = 2, UnitPrice = 5000m, IsFree = false }
                };

                return ApplyQuery(data, pageNumber, pageSize, sortBy, sortDescending, filterField, filterValue, searchKeyword);
            }

            public OrderToppingModel GetById(string id)
            {
                if (!int.TryParse(id, out int intId)) return null;
                var data = _orderToppings ?? GetAll();
                return data.FirstOrDefault(o => o.Id == intId);
            }

            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var data = _orderToppings ?? GetAll();
                var item = data.FirstOrDefault(o => o.Id == intId);
                if (item == null) return 0;
                data.Remove(item);
                _orderToppings = data;
                return 1;
            }

            public int UpdateById(string id, OrderToppingModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data = _orderToppings ?? GetAll();
                var item = data.FirstOrDefault(o => o.Id == intId);
                if (item == null) return 0;
                item.InvoiceDetailId = info.InvoiceDetailId;
                item.ProductId = info.ProductId;
                item.Quantity = info.Quantity;
                item.UnitPrice = info.UnitPrice;
                item.IsFree = info.IsFree;
                _orderToppings = data;
                return 1;
            }

            public int Insert(string id, OrderToppingModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data = _orderToppings ?? GetAll();
                if (data.Any(o => o.Id == intId)) return 0;
                info.Id = intId;
                data.Add(info);
                _orderToppings = data;
                return 1;
            }

            public int GetCount(string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                var data = _orderToppings ?? GetAll();
                return ApplyCountQuery(data, filterField, filterValue, searchKeyword);
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

            public List<SupplierModel> GetAll(
                int pageNumber = 1,
                int pageSize = 10,
                string sortBy = null,
                bool sortDescending = false,
                string filterField = null,
                string filterValue = null,
                string searchKeyword = null)
            {
                var data = _suppliers ?? new List<SupplierModel>
                {
                    new SupplierModel { Id = 1, Name = "Vinamilk", Email = "contact@vinamilk.com", Phone = "02412345678", Address = "Hanoi" },
                    new SupplierModel { Id = 2, Name = "SugarCo", Email = "info@sugarco.com", Phone = "02823456789", Address = "Ho Chi Minh" },
                    new SupplierModel { Id = 3, Name = "TeaFarm", Email = "sales@teafarm.com", Phone = "02334567890", Address = "Da Nang" },
                    new SupplierModel { Id = 4, Name = "CoffeeBean", Email = "support@coffeebean.com", Phone = "02545678901", Address = "Can Tho" },
                    new SupplierModel { Id = 5, Name = "FruitPuree", Email = "order@fruitpuree.com", Phone = "02756789012", Address = "Hai Phong" }
                };

                return ApplyQuery(data, pageNumber, pageSize, sortBy, sortDescending, filterField, filterValue, searchKeyword);
            }

            public SupplierModel GetById(string id)
            {
                if (!int.TryParse(id, out int intId)) return null;
                var data = _suppliers ?? GetAll();
                return data.FirstOrDefault(s => s.Id == intId);
            }

            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var data = _suppliers ?? GetAll();
                var item = data.FirstOrDefault(s => s.Id == intId);
                if (item == null) return 0;
                data.Remove(item);
                _suppliers = data;
                return 1;
            }

            public int UpdateById(string id, SupplierModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data = _suppliers ?? GetAll();
                var item = data.FirstOrDefault(s => s.Id == intId);
                if (item == null) return 0;
                item.Name = info.Name;
                item.Email = info.Email;
                item.Phone = info.Phone;
                item.Address = info.Address;
                _suppliers = data;
                return 1;
            }

            public int Insert(string id, SupplierModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data = _suppliers ?? GetAll();
                if (data.Any(s => s.Id == intId)) return 0;
                info.Id = intId;
                data.Add(info);
                _suppliers = data;
                return 1;
            }

            public int GetCount(string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                var data = _suppliers ?? GetAll();
                return ApplyCountQuery(data, filterField, filterValue, searchKeyword);
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

            public List<ProductModel> GetAll(
                int pageNumber = 1,
                int pageSize = 10,
                string sortBy = null,
                bool sortDescending = false,
                string filterField = null,
                string filterValue = null,
                string searchKeyword = null)
            {
                var data = _products ?? new List<ProductModel>
                {
                    new ProductModel { Id = 1, Name = "Black Coffee", Price = 35000f, Cost = 21000f, IsActive = true, CategoryId = 1, ImageUrl = "kohi_logo.png", InvoiceDetails = new List<InvoiceDetailModel>() },
                    new ProductModel { Id = 2, Name = "Milk Tea", Price = 40000f, Cost = 24000f, IsActive = false, CategoryId = 2, ImageUrl = "kohi_logo.png", InvoiceDetails = new List<InvoiceDetailModel>() },
                    new ProductModel { Id = 3, Name = "Green Tea", Price = 30000f, Cost = 18000f, IsActive = true, CategoryId = 3, ImageUrl = "kohi_logo.png", InvoiceDetails = new List<InvoiceDetailModel>() },
                    new ProductModel { Id = 4, Name = "Mango Smoothie", Price = 45000f, Cost = 27000f, IsActive = false, CategoryId = 4, ImageUrl = "kohi_logo.png", InvoiceDetails = new List<InvoiceDetailModel>() },
                    new ProductModel { Id = 5, Name = "Espresso", Price = 40000f, Cost = 24000f, IsActive = true, CategoryId = 1, ImageUrl = "kohi_logo.png", InvoiceDetails = new List<InvoiceDetailModel>() }
                };

                return ApplyQuery(data, pageNumber, pageSize, sortBy, sortDescending, filterField, filterValue, searchKeyword);
            }

            public ProductModel GetById(string id)
            {
                if (!int.TryParse(id, out int intId)) return null;
                var data = _products ?? GetAll();
                return data.FirstOrDefault(p => p.Id == intId);
            }

            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var data = _products ?? GetAll();
                var item = data.FirstOrDefault(p => p.Id == intId);
                if (item == null) return 0;
                data.Remove(item);
                _products = data;
                return 1;
            }

            public int UpdateById(string id, ProductModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data = _products ?? GetAll();
                var item = data.FirstOrDefault(p => p.Id == intId);
                if (item == null) return 0;
                item.Name = info.Name;
                item.Price = info.Price;
                item.Cost = info.Cost;
                item.IsActive = info.IsActive;
                item.CategoryId = info.CategoryId;
                item.ImageUrl = info.ImageUrl;
                item.InvoiceDetails = info.InvoiceDetails;
                _products = data;
                return 1;
            }

            public int Insert(string id, ProductModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data = _products ?? GetAll();
                if (data.Any(p => p.Id == intId)) return 0;
                info.Id = intId;
                data.Add(info);
                _products = data;
                return 1;
            }

            public int GetCount(string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                var data = _products ?? GetAll();
                return ApplyCountQuery(data, filterField, filterValue, searchKeyword);
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

            public List<OutboundModel> GetAll(
                int pageNumber = 1,
                int pageSize = 10,
                string sortBy = null,
                bool sortDescending = false,
                string filterField = null,
                string filterValue = null,
                string searchKeyword = null)
            {
                var data = _outbounds ?? new List<OutboundModel>
                {
                    new OutboundModel { Id = 1, InventoryId = 1, Quantity = 10, OutboundDate = DateTime.Now.AddDays(-14), Purpose = "Production", Notes = "For black coffee" },
                    new OutboundModel { Id = 2, InventoryId = 2, Quantity = 5, OutboundDate = DateTime.Now.AddDays(-9), Purpose = "Production", Notes = "For milk tea" },
                    new OutboundModel { Id = 3, InventoryId = 3, Quantity = 8, OutboundDate = DateTime.Now.AddDays(-6), Purpose = "Production", Notes = "For green tea" },
                    new OutboundModel { Id = 4, InventoryId = 4, Quantity = 6, OutboundDate = DateTime.Now.AddDays(-4), Purpose = "Production", Notes = "For mango smoothie" },
                    new OutboundModel { Id = 5, InventoryId = 5, Quantity = 7, OutboundDate = DateTime.Now.AddDays(-2), Purpose = "Production", Notes = "For espresso" }
                };

                return ApplyQuery(data, pageNumber, pageSize, sortBy, sortDescending, filterField, filterValue, searchKeyword);
            }

            public OutboundModel GetById(string id)
            {
                if (!int.TryParse(id, out int intId)) return null;
                var data = _outbounds ?? GetAll();
                return data.FirstOrDefault(o => o.Id == intId);
            }

            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var data = _outbounds ?? GetAll();
                var item = data.FirstOrDefault(o => o.Id == intId);
                if (item == null) return 0;
                data.Remove(item);
                _outbounds = data;
                return 1;
            }

            public int UpdateById(string id, OutboundModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data = _outbounds ?? GetAll();
                var item = data.FirstOrDefault(o => o.Id == intId);
                if (item == null) return 0;
                item.InventoryId = info.InventoryId;
                item.Quantity = info.Quantity;
                item.OutboundDate = info.OutboundDate;
                item.Purpose = info.Purpose;
                item.Notes = info.Notes;
                _outbounds = data;
                return 1;
            }

            public int Insert(string id, OutboundModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data = _outbounds ?? GetAll();
                if (data.Any(o => o.Id == intId)) return 0;
                info.Id = intId;
                data.Add(info);
                _outbounds = data;
                return 1;
            }

            public int GetCount(string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                var data = _outbounds ?? GetAll();
                return ApplyCountQuery(data, filterField, filterValue, searchKeyword);
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

            public List<RecipeModel> GetAll(
                int pageNumber = 1,
                int pageSize = 10,
                string sortBy = null,
                bool sortDescending = false,
                string filterField = null,
                string filterValue = null,
                string searchKeyword = null)
            {
                var data = _recipes ?? new List<RecipeModel>
                {
                    new RecipeModel { Id = 1, ProductId = 1, Ingredients = new List<RecipeDetailModel>() },
                    new RecipeModel { Id = 2, ProductId = 2, Ingredients = new List<RecipeDetailModel>() },
                    new RecipeModel { Id = 3, ProductId = 3, Ingredients = new List<RecipeDetailModel>() },
                    new RecipeModel { Id = 4, ProductId = 4, Ingredients = new List<RecipeDetailModel>() },
                    new RecipeModel { Id = 5, ProductId = 5, Ingredients = new List<RecipeDetailModel>() }
                };

                return ApplyQuery(data, pageNumber, pageSize, sortBy, sortDescending, filterField, filterValue, searchKeyword);
            }

            public RecipeModel GetById(string id)
            {
                if (!int.TryParse(id, out int intId)) return null;
                var data = _recipes ?? GetAll();
                return data.FirstOrDefault(r => r.Id == intId);
            }

            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var data = _recipes ?? GetAll();
                var item = data.FirstOrDefault(r => r.Id == intId);
                if (item == null) return 0;
                data.Remove(item);
                _recipes = data;
                return 1;
            }

            public int UpdateById(string id, RecipeModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data = _recipes ?? GetAll();
                var item = data.FirstOrDefault(r => r.Id == intId);
                if (item == null) return 0;
                item.ProductId = info.ProductId;
                item.Ingredients = info.Ingredients;
                _recipes = data;
                return 1;
            }

            public int Insert(string id, RecipeModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data = _recipes ?? GetAll();
                if (data.Any(r => r.Id == intId)) return 0;
                info.Id = intId;
                data.Add(info);
                _recipes = data;
                return 1;
            }

            public int GetCount(string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                var data = _recipes ?? GetAll();
                return ApplyCountQuery(data, filterField, filterValue, searchKeyword);
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

            public List<TaxModel> GetAll(
                int pageNumber = 1,
                int pageSize = 10,
                string sortBy = null,
                bool sortDescending = false,
                string filterField = null,
                string filterValue = null,
                string searchKeyword = null)
            {
                var data = _taxes ?? new List<TaxModel>
                {
                    new TaxModel { Id = 1, TaxName = "VAT 10%", TaxRate = 0.10m, InvoiceTaxes = new List<InvoiceTaxModel>() },
                    new TaxModel { Id = 2, TaxName = "Service Tax 5%", TaxRate = 0.05m, InvoiceTaxes = new List<InvoiceTaxModel>() },
                    new TaxModel { Id = 3, TaxName = "Special Tax 2%", TaxRate = 0.02m, InvoiceTaxes = new List<InvoiceTaxModel>() },
                    new TaxModel { Id = 4, TaxName = "No Tax", TaxRate = 0.00m, InvoiceTaxes = new List<InvoiceTaxModel>() },
                    new TaxModel { Id = 5, TaxName = "VAT 8%", TaxRate = 0.08m, InvoiceTaxes = new List<InvoiceTaxModel>() }
                };

                return ApplyQuery(data, pageNumber, pageSize, sortBy, sortDescending, filterField, filterValue, searchKeyword);
            }

            public TaxModel GetById(string id)
            {
                if (!int.TryParse(id, out int intId)) return null;
                var data = _taxes ?? GetAll();
                return data.FirstOrDefault(t => t.Id == intId);
            }

            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var data = _taxes ?? GetAll();
                var item = data.FirstOrDefault(t => t.Id == intId);
                if (item == null) return 0;
                data.Remove(item);
                _taxes = data;
                return 1;
            }

            public int UpdateById(string id, TaxModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data = _taxes ?? GetAll();
                var item = data.FirstOrDefault(t => t.Id == intId);
                if (item == null) return 0;
                item.TaxName = info.TaxName;
                item.TaxRate = info.TaxRate;
                item.InvoiceTaxes = info.InvoiceTaxes;
                _taxes = data;
                return 1;
            }

            public int Insert(string id, TaxModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data = _taxes ?? GetAll();
                if (data.Any(t => t.Id == intId)) return 0;
                info.Id = intId;
                data.Add(info);
                _taxes = data;
                return 1;
            }

            public int GetCount(string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                var data = _taxes ?? GetAll();
                return ApplyCountQuery(data, filterField, filterValue, searchKeyword);
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

            public List<InvoiceTaxModel> GetAll(
                int pageNumber = 1,
                int pageSize = 10,
                string sortBy = null,
                bool sortDescending = false,
                string filterField = null,
                string filterValue = null,
                string searchKeyword = null)
            {
                var data = _invoiceTaxes ?? new List<InvoiceTaxModel>
                {
                    new InvoiceTaxModel { Id = 1, InvoiceId = 1, TaxId = 1 },
                    new InvoiceTaxModel { Id = 2, InvoiceId = 2, TaxId = 2 },
                    new InvoiceTaxModel { Id = 3, InvoiceId = 3, TaxId = 3 },
                    new InvoiceTaxModel { Id = 4, InvoiceId = 4, TaxId = 4 },
                    new InvoiceTaxModel { Id = 5, InvoiceId = 5, TaxId = 5 }
                };

                return ApplyQuery(data, pageNumber, pageSize, sortBy, sortDescending, filterField, filterValue, searchKeyword);
            }

            public InvoiceTaxModel GetById(string id)
            {
                if (!int.TryParse(id, out int intId)) return null;
                var data = _invoiceTaxes ?? GetAll();
                return data.FirstOrDefault(i => i.Id == intId);
            }

            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var data = _invoiceTaxes ?? GetAll();
                var item = data.FirstOrDefault(i => i.Id == intId);
                if (item == null) return 0;
                data.Remove(item);
                _invoiceTaxes = data;
                return 1;
            }

            public int UpdateById(string id, InvoiceTaxModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data = _invoiceTaxes ?? GetAll();
                var item = data.FirstOrDefault(i => i.Id == intId);
                if (item == null) return 0;
                item.InvoiceId = info.InvoiceId;
                item.TaxId = info.TaxId;
                _invoiceTaxes = data;
                return 1;
            }

            public int Insert(string id, InvoiceTaxModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data = _invoiceTaxes ?? GetAll();
                if (data.Any(i => i.Id == intId)) return 0;
                info.Id = intId;
                data.Add(info);
                _invoiceTaxes = data;
                return 1;
            }

            public int GetCount(string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                var data = _invoiceTaxes ?? GetAll();
                return ApplyCountQuery(data, filterField, filterValue, searchKeyword);
            }
        }

        public class MockCheckInventoryRepository : IRepository<CheckInventoryModel>
        {
            private List<CheckInventoryModel> _checkInventories;

            public MockCheckInventoryRepository() { }
            public MockCheckInventoryRepository(List<CheckInventoryModel> checkInventories)
            {
                _checkInventories = checkInventories;
            }

            public List<CheckInventoryModel> GetAll(
                int pageNumber = 1,
                int pageSize = 10,
                string sortBy = null,
                bool sortDescending = false,
                string filterField = null,
                string filterValue = null,
                string searchKeyword = null)
            {
                var data = _checkInventories ?? new List<CheckInventoryModel>
                {
                    new CheckInventoryModel { Id = 1, InventoryId = 2, ActualQuantity = 43, CheckDate = DateTime.Now.AddDays(-2), Notes = "Thiếu do hao hụt"},
                    new CheckInventoryModel { Id = 2, InventoryId = 4, ActualQuantity = 55, CheckDate = DateTime.Now.AddDays(-1), Notes = "Đủ"},
                    new CheckInventoryModel { Id = 3, InventoryId = 3, ActualQuantity = 76, CheckDate = DateTime.Now, Notes = "Dư có thể hoàn kho"},
                };

                return ApplyQuery(data, pageNumber, pageSize, sortBy, sortDescending, filterField, filterValue, searchKeyword);
            }

            public CheckInventoryModel GetById(string id)
            {
                if (!int.TryParse(id, out int intId)) return null;
                var data = _checkInventories ?? GetAll();
                return data.FirstOrDefault(i => i.Id == intId);
            }

            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var data = _checkInventories ?? GetAll();
                var item = data.FirstOrDefault(i => i.Id == intId);
                if (item == null) return 0;
                data.Remove(item);
                _checkInventories = data;
                return 1;
            }

            public int UpdateById(string id, CheckInventoryModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data = _checkInventories ?? GetAll();
                var item = data.FirstOrDefault(i => i.Id == intId);
                if (item == null) return 0;
                item.InventoryId = info.InventoryId;
                item.ActualQuantity = info.ActualQuantity;
                item.CheckDate = info.CheckDate;
                item.Notes = info.Notes;
                item.Inventory = info.Inventory; // Update navigation property if needed
                _checkInventories = data;
                return 1;
            }

            public int Insert(string id, CheckInventoryModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data = _checkInventories ?? GetAll();
                if (data.Any(i => i.Id == intId)) return 0;
                info.Id = intId;
                data.Add(info);
                _checkInventories = data;
                return 1;
            }

            public int GetCount(string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                var data = _checkInventories ?? GetAll();
                return ApplyCountQuery(data, filterField, filterValue, searchKeyword);
            }
        }
        // Helper method to apply pagination, sorting, filtering, and searching
        private static List<T> ApplyQuery<T>(
            List<T> data,
            int pageNumber,
            int pageSize,
            string sortBy,
            bool sortDescending,
            string filterField,
            string filterValue,
            string searchKeyword)
        {
            IEnumerable<T> query = data; // Thay IQueryable<T> bằng IEnumerable<T>

            // Filtering
            if (!string.IsNullOrEmpty(filterField) && !string.IsNullOrEmpty(filterValue))
            {
                query = query.Where(item => item.GetType()
                    .GetProperty(filterField)?.GetValue(item)?.ToString() == filterValue);
            }

            // Searching
            if (!string.IsNullOrEmpty(searchKeyword))
            {
                query = query.Where(item => item.GetType()
                    .GetProperties()
                    .Any(prop => prop.GetValue(item)?.ToString()
                    .Contains(searchKeyword, StringComparison.OrdinalIgnoreCase) == true));
            }

            // Sorting
            if (!string.IsNullOrEmpty(sortBy))
            {
                query = sortDescending
                    ? query.OrderByDescending(item => item.GetType().GetProperty(sortBy)?.GetValue(item))
                    : query.OrderBy(item => item.GetType().GetProperty(sortBy)?.GetValue(item));
            }

            // Pagination
            query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);

            return query.ToList();
        }

        private static int ApplyCountQuery<T>(
            List<T> data,
            string filterField = null,
            string filterValue = null,
            string searchKeyword = null)
        {
            IEnumerable<T> query = data;

            // Filtering
            if (!string.IsNullOrEmpty(filterField) && !string.IsNullOrEmpty(filterValue))
            {
                query = query.Where(item => item.GetType()
                    .GetProperty(filterField)?.GetValue(item)?.ToString() == filterValue);
            }

            // Searching
            if (!string.IsNullOrEmpty(searchKeyword))
            {
                query = query.Where(item => item.GetType()
                    .GetProperties()
                    .Any(prop => prop.GetValue(item)?.ToString()
                    .Contains(searchKeyword, StringComparison.OrdinalIgnoreCase) == true));
            }

            return query.Count();
        }

        // Constructor to link Navigation Properties
        public MockDao()
        {
            // Initialize independent data
            var categories = new MockCategoryRepository().GetAll();
            var customers = new MockCustomerRepository().GetAll();
            var expenseCategories = new MockExpenseCategoryRepository().GetAll();
            var expenses = new MockExpenseRepository().GetAll();
            var inbounds = new MockInboundRepository().GetAll();
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
            var checkInventories = new MockCheckInventoryRepository().GetAll();

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

            // CheckInventoryModel - InventoryModel (1-1)
            foreach (var checkInventory in checkInventories)
            {
                checkInventory.Inventory = inventories.FirstOrDefault(i => i.Id == checkInventory.InventoryId);
            }

            // Assign repositories with linked data
            Categories = new MockCategoryRepository(categories);
            Customers = new MockCustomerRepository(customers);
            ExpenseCategories = new MockExpenseCategoryRepository(expenseCategories);
            Expenses = new MockExpenseRepository(expenses);
            Inbounds = new MockInboundRepository(inbounds);
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
            CheckInventories = new MockCheckInventoryRepository(checkInventories);
        }

        // Properties
        public IRepository<ProductModel> Products { get; set; }
        public IRepository<CategoryModel> Categories { get; set; }
        public IRepository<CustomerModel> Customers { get; set; }
        public IRepository<ExpenseCategoryModel> ExpenseCategories { get; set; }
        public IRepository<ExpenseModel> Expenses { get; set; }
        public IRepository<InboundModel> Inbounds { get; set; }
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
        public IRepository<CheckInventoryModel> CheckInventories { get; set; }

    }
}