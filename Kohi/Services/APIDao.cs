﻿using System;
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

        private static void LogRequestDetails(HttpRequestMessage request)
        {
            Debug.WriteLine("Request Method: " + request.Method);

            Debug.WriteLine("Request URL: " + request.RequestUri);

            Debug.WriteLine("Request Headers:");
            foreach (var header in request.Headers)
            {
                Debug.WriteLine($"{header.Key}: {string.Join(", ", header.Value)}");
            }

            if (request.Content != null)
            {
                string requestBody = request.Content.ReadAsStringAsync().Result;
                Debug.WriteLine("Request Body: ");
                Debug.WriteLine(requestBody);
            }
        }

        public class APICategoryRepository : IRepository<CategoryModel>
        {
            public int DeleteById(string id)
            {
                var reponse = client.DeleteAsync($"{baseURL}/categories?id=eq.{id}").Result;

                if (reponse.IsSuccessStatusCode)
                {
                    return 1;
                }
                else
                {
                    Debug.WriteLine("Failed deletion");
                    Debug.WriteLine(reponse.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }

            public List<CategoryModel> GetAll(int pageNumber = 1, int pageSize = 10, string sortBy = null, bool sortDescending = false, string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                HttpResponseMessage response = client.GetAsync($"{baseURL}/categories?limit={pageSize}&offset={(pageNumber - 1) * pageSize}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;

                    List<CategoryModel> categories = JsonConvert.DeserializeObject<List<CategoryModel>>(jsonResponse);

                    return categories;
                }
                else
                {
                    Debug.WriteLine("Error: " + response.StatusCode);
                    return new List<CategoryModel>();
                }
            }

            public CategoryModel GetById(string id)
            {
                HttpResponseMessage response = client.GetAsync($"{baseURL}/categories?id=eq.{id}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;
                    List<CategoryModel> categories = JsonConvert.DeserializeObject<List<CategoryModel>>(jsonResponse);
                    if (categories?.Count > 0)
                    {
                        return categories[0];
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    Debug.WriteLine("Error: " + response.StatusCode);
                    return null;
                }
            }

            public int GetCount(string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                client.DefaultRequestHeaders.Add("Prefer", "count=exact");
                var response = client.GetAsync($"{baseURL}/categories").Result;
                if (response.IsSuccessStatusCode)
                {
                    int total = Int32.Parse(response.Headers.GetValues("Content-Range").ToString().Split("/")[^1]);
                    return total;
                }
                else
                {
                    Debug.WriteLine("Failed count total");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }

            public int Insert(string id, CategoryModel info)
            {
                JObject jsonObject = JObject.FromObject(info);
                //Remove all object's properties that are not match with the columns
                jsonObject.Remove("Id");
                jsonObject.Remove("Products");
                JObject newJsonObject = new JObject();
                //keys must match with the columns in the database (lowercase)
                foreach (var pair in jsonObject.Properties())
                {
                    newJsonObject.Add(pair.Name.ToLower(), pair.Value);
                }

                string jsonContent = newJsonObject.ToString();
                StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{baseURL}/categories")
                {
                    Content = content
                };
                LogRequestDetails(request);
                HttpResponseMessage response = client.Send(request);

                if (response.IsSuccessStatusCode)
                {
                    // Handle the successful response (e.g., read the response content)
                    string responseContent = response.Content.ReadAsStringAsync().Result;
                    Debug.WriteLine("Category inserted successfully: " + responseContent);
                    return 1;
                }
                else
                {
                    Debug.WriteLine("Failed insertion");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }

            public int UpdateById(string id, CategoryModel info)
            {
                JObject jsonObject = JObject.FromObject(info);
                //Remove all object's properties that are not match with the columns
                jsonObject.Remove("Id");
                jsonObject.Remove("Products");
                JObject newJsonObject = new JObject();
                //keys must match with the columns in the database (lowercase)
                foreach (var pair in jsonObject.Properties())
                {
                    newJsonObject.Add(pair.Name.ToLower(), pair.Value);
                }
                StringContent content = new StringContent(newJsonObject.ToString(), Encoding.UTF8, "application/json");
                var reponse = client.PatchAsync($"{baseURL}/categories?id=eq.{id}", content).Result;

                if (reponse.IsSuccessStatusCode)
                {
                    return 1;
                }
                else
                {
                    Debug.WriteLine(reponse.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }
        }

        public class APIProductRepository : IRepository<ProductModel>
        {
            private List<ProductModel> _products;

            public APIProductRepository() { }
            public APIProductRepository(List<ProductModel> products)
            {
                _products = products;
            }

            public int DeleteById(string id)
            {
                var reponse = client.DeleteAsync($"{baseURL}/products?id=eq.{id}").Result;

                if (reponse.IsSuccessStatusCode)
                {
                    return 1;
                }
                else
                {
                    Debug.WriteLine("Failed deletion");
                    Debug.WriteLine(reponse.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }
            public List<ProductModel> GetAll(int pageNumber = 1, int pageSize = 10, string sortBy = null, bool sortDescending = false, string filterField = null, string filterValue = null, string searchKeyword = null)
            {

                HttpResponseMessage response = client.GetAsync($"{baseURL}/products?limit={pageSize}&offset={(pageNumber - 1) * pageSize}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;

                    List<ProductModel> products = JsonConvert.DeserializeObject<List<ProductModel>>(jsonResponse);

                    _products = products;

                    return products;
                }
                else
                {
                    Debug.WriteLine("Error: " + response.StatusCode);
                    return new List<ProductModel>();
                }
            }
            public ProductModel GetById(string id)
            {
                HttpResponseMessage response = client.GetAsync($"{baseURL}/products?id=eq.{id}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;
                    List<ProductModel> products = JsonConvert.DeserializeObject<List<ProductModel>>(jsonResponse);
                    if (products?.Count > 0)
                    {
                        return products[0];
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    Debug.WriteLine("Error: " + response.StatusCode);
                    return null;
                }
            }
            public int GetCount(string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                client.DefaultRequestHeaders.Add("Prefer", "count=exact");
                var response = client.GetAsync($"{baseURL}/products").Result;
                if (response.IsSuccessStatusCode)
                {
                    int total = Int32.Parse(response.Headers.GetValues("Content-Range").ToString().Split("/")[^1]);
                    return total;
                }
                else
                {
                    Debug.WriteLine("Failed count total");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    return 0;
                }

            }

            public int Insert(string id, ProductModel info)
            {
                Debug.WriteLine("Insertion");
                JObject jsonObject = JObject.FromObject(info);
                //Remove all object's properties that are not match with the columns
                jsonObject.Remove("Category");
                jsonObject.Remove("ProductVariants");
                jsonObject.Remove("Id");
                JObject newJsonObject = new JObject();
                //keys must match with the columns in the database (lowercase)
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
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                }
                return 1;
            }
            public int UpdateById(string id, ProductModel info)
            {
                JObject jsonObject = JObject.FromObject(info);
                //Remove all object's properties that are not match with the columns
                jsonObject.Remove("Category");
                jsonObject.Remove("ProductVariants");
                jsonObject.Remove("Id");
                JObject newJsonObject = new JObject();
                //keys must match with the columns in the database (lowercase)
                foreach (var pair in jsonObject.Properties())
                {
                    newJsonObject.Add(pair.Name.ToLower(), pair.Value);
                }
                StringContent content = new StringContent(newJsonObject.ToString(), Encoding.UTF8, "application/json");
                var reponse = client.PatchAsync($"{baseURL}/products?id=eq.{id}", content).Result;

                if (reponse.IsSuccessStatusCode)
                {
                    return 1;
                }
                else
                {
                    Debug.WriteLine(reponse.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }
        }

        public APIDao()
        {
            client.DefaultRequestHeaders.Add("Prefer", "return=representation");

            var categories = new APICategoryRepository().GetAll();
            //var customers = new APICustomerRepository().GetAll();
            //var expenseCategories = new APIExpenseCategoryRepository().GetAll();
            //var expenses = new APIExpenseRepository().GetAll();
            //var inbounds = new APIInboundRepository().GetAll();
            //var invoices = new APIInvoiceRepository().GetAll();
            //var inventories = new APIInventoryRepository().GetAll();
            //var ingredients = new APIIngredientRepository().GetAll();
            //var invoiceDetails = new APIInvoiceDetailRepository().GetAll();
            //var recipeDetails = new APIRecipeDetailRepository().GetAll();
            //var payments = new APIPaymentRepository().GetAll();
            //var orderToppings = new APIOrderToppingRepository().GetAll();
            //var suppliers = new APISupplierRepository().GetAll();
            var products = new APIProductRepository().GetAll();
            //var outbounds = new APIOutboundRepository().GetAll();
            //var productVariants = new APIProductVariantRepository().GetAll();
            //var taxes = new APITaxRepository().GetAll();
            //var invoiceTaxes = new APIInvoiceTaxRepository().GetAll();
            //var checkInventories = new APICheckInventoryRepository().GetAll();

            Categories = new APICategoryRepository(categories);
            //Customers = new APICustomerRepository(customers);
            //ExpenseCategories = new APIExpenseCategoryRepository(expenseCategories);
            //Expenses = new APIExpenseRepository(expenses);
            //Inbounds = new APIInboundRepository(inbounds);
            //Invoices = new APIInvoiceRepository(invoices);
            //Inventories = new APIInventoryRepository(inventories);
            //Ingredients = new APIIngredientRepository(ingredients);
            //InvoiceDetails = new APIInvoiceDetailRepository(invoiceDetails);
            //RecipeDetails = new APIRecipeDetailRepository(recipeDetails);
            //Payments = new APIPaymentRepository(payments);
            //OrderToppings = new APIOrderToppingRepository(orderToppings);
            //Suppliers = new APISupplierRepository(suppliers);
            Products = new APIProductRepository(products);
            //Outbounds = new APIOutboundRepository(outbounds);
            //ProductVariants = new APIProductVariantRepository(productVariants);
            //Taxes = new APITaxRepository(taxes);
            //InvoiceTaxes = new APIInvoiceTaxRepository(invoiceTaxes);
            //CheckInventories = new APICheckInventoryRepository(checkInventories);
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
