using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Kohi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static Kohi.Services.MockDao;

namespace Kohi.Services
{
    internal class APIDao : IDao
    {
        private static readonly HttpClient client = new HttpClient();
        private static String baseURL = "http://localhost:3000";
        private static async Task<HttpResponseMessage> APIQueryCallAsync(string URL)
        {
            HttpResponseMessage response = await client.GetAsync(URL);

            return response;
        }

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

        public class APIProductRepository : IRepository<ProductModel>
        {
            private List<ProductModel> _products;
                
            public APIProductRepository()
            {
            }
            public APIProductRepository(List<ProductModel> products)
            {
                _products = products;
            }

            public int DeleteById(string id)
            {
                throw new NotImplementedException();
            }
            public List<ProductModel> GetAll(int pageNumber = 1, int pageSize = 10, string sortBy = null, bool sortDescending = false, string filterField = null, string filterValue = null, string searchKeyword = null)
            {

                HttpResponseMessage response = client.GetAsync($"{baseURL}/products?limit={pageSize}&offset={(pageNumber - 1)*pageSize}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;

                    List<ProductModel> products = JsonConvert.DeserializeObject<List<ProductModel>>(jsonResponse);

                    _products = products;

                    return products;
                }
                else
                {
                    Console.WriteLine("Error: " + response.StatusCode);
                    return new List<ProductModel>();
                }
            }
            public ProductModel GetById(string id)
            {
                throw new NotImplementedException();
            }
            public int GetCount(string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                return _products.Count;
            }
            private void LogRequestDetails(HttpRequestMessage request)
            {
                // Log the HTTP Method (e.g., GET, POST, etc.)
                Debug.WriteLine("Request Method: " + request.Method);

                // Log the Request URL
                Debug.WriteLine("Request URL: " + request.RequestUri);

                // Log the Request Headers
                Debug.WriteLine("Request Headers:");
                foreach (var header in request.Headers)
                {
                    Debug.WriteLine($"{header.Key}: {string.Join(", ", header.Value)}");
                }

                // Log the Request Body (if any)
                if (request.Content != null)
                {
                    string requestBody = request.Content.ReadAsStringAsync().Result;
                    Debug.WriteLine("Request Body: ");
                    Debug.WriteLine(requestBody);
                }
            }
            public int Insert(string id, ProductModel info)
            {
                Debug.WriteLine("Insertion");
                JObject jsonObject = JObject.FromObject(info);
                jsonObject.Remove("Category");
                jsonObject.Remove("ProductVariants");
                jsonObject.Remove("Id");
                JObject newJsonObject = new JObject();
                foreach (var pair in jsonObject.Properties())
                {
                    newJsonObject.Add(pair.Name.ToLower(), pair.Value);
                }

                string jsonContent = newJsonObject.ToString();
                StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{baseURL}/products")
                {
                    Content = content
                };
                LogRequestDetails(request);
                HttpResponseMessage response = client.Send(request);

                ProductModel product;
                if (response.IsSuccessStatusCode)
                {
                    // Handle the successful response (e.g., read the response content)
                    string responseContent = response.Content.ReadAsStringAsync().Result;
                    product = JsonConvert.DeserializeObject<List<ProductModel>>(responseContent)[0];
                    Debug.WriteLine("Person inserted successfully: " + product.Id);

                }
                else
                {
                    Debug.WriteLine("Failed insertion");
                    Debug.WriteLine(response.ReasonPhrase);
                }
                return 1;
            }
            public int UpdateById(string id, ProductModel info)
            {
                throw new NotImplementedException();
            }
        }

        public APIDao()
        {
            client.DefaultRequestHeaders.Add("Prefer", "return=representation");

            //var categories = new APICategoryRepository().GetAll();
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
            var products = new APIProductRepository().GetAll();
            //var outbounds = new MockOutboundRepository().GetAll();
            //var productVariants = new MockProductVariantRepository().GetAll();
            //var taxes = new MockTaxRepository().GetAll();
            //var invoiceTaxes = new MockInvoiceTaxRepository().GetAll();
            //var checkInventories = new MockCheckInventoryRepository().GetAll();

            //Categories = new MockCategoryRepository(categories);
            //Customers = new MockCustomerRepository(customers);
            //ExpenseCategories = new MockExpenseCategoryRepository(expenseCategories);
            //Expenses = new MockExpenseRepository(expenses);
            //Inbounds = new MockInboundRepository(inbounds);
            //Invoices = new MockInvoiceRepository(invoices);
            //Inventories = new MockInventoryRepository(inventories);
            //Ingredients = new MockIngredientRepository(ingredients);
            //InvoiceDetails = new MockInvoiceDetailRepository(invoiceDetails);
            //RecipeDetails = new MockRecipeDetailRepository(recipeDetails);
            //Payments = new MockPaymentRepository(payments);
            //OrderToppings = new MockOrderToppingRepository(orderToppings);
            //Suppliers = new MockSupplierRepository(suppliers);
            Products = new APIProductRepository(products);
            //Outbounds = new MockOutboundRepository(outbounds);
            //ProductVariants = new MockProductVariantRepository(productVariants);
            //Taxes = new MockTaxRepository(taxes);
            //InvoiceTaxes = new MockInvoiceTaxRepository(invoiceTaxes);
            //CheckInventories = new MockCheckInventoryRepository(checkInventories);
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
