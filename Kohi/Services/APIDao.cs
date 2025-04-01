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
using static System.Runtime.InteropServices.JavaScript.JSType;
using WinRT.Interop;
using static Kohi.Services.MockDao;

namespace Kohi.Services
{
    internal class APIDao : IDao
    {
        private static readonly HttpClient client = new HttpClient();
        private static string baseURL = "http://localhost:3000";
        private static JsonSerializer serializer = new JsonSerializer
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

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
            List<CategoryModel> _categories;
            private static int _nextId;
            public APICategoryRepository() { }
            public APICategoryRepository(List<CategoryModel> list) { _categories = list; }

            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var reponse = client.DeleteAsync($"{baseURL}/categories?id=eq.{id}").Result;

                if (reponse.IsSuccessStatusCode)
                {
                    var item = _categories.FirstOrDefault(r => r.Id == intId);
                    _categories.Remove(item);
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
                if (_categories != null)
                {
                    return _categories;
                }
                HttpResponseMessage response = client.GetAsync($"{baseURL}/categories?limit={pageSize}&offset={(pageNumber - 1) * pageSize}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;

                    List<CategoryModel> categories = JsonConvert.DeserializeObject<List<CategoryModel>>(jsonResponse);

                    _categories = categories;

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
                if (!int.TryParse(id, out int intId)) return null;
                var data = _categories ?? GetAll();
                return data.FirstOrDefault(p => p.Id == intId);

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
                    Debug.WriteLine("Failed to get category by ID");
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
                    int total = Int32.Parse(response.Content.Headers.GetValues("Content-Range").FirstOrDefault().Split('/')[^1]);
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
                if (info == null) return 0;

                _categories.Add(info);

                JObject jsonObject = JObject.FromObject(info, serializer);
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
                    JArray jsonArray = JArray.Parse(responseContent);
                    _categories[^1].Id = jsonArray[0]["id"].ToObject<int>();
                    return 1;
                }
                else
                {
                    Debug.WriteLine("Failed insertion");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    _categories.RemoveAt(_categories.Count - 1);
                    return 0;
                }
            }

            public int Insert(CategoryModel info)
            {
                return Insert("", info);
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
                if (!int.TryParse(id, out int intId)) return 0;
                var reponse = client.DeleteAsync($"{baseURL}/products?id=eq.{id}").Result;

                if (reponse.IsSuccessStatusCode)
                {
                    var item = _products.FirstOrDefault(r => r.Id == intId);
                    _products.Remove(item);
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
                if (_products != null)
                {
                    return _products;
                }
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
                if (!int.TryParse(id, out int intId)) return null;
                var data = _products ?? GetAll();
                return data.FirstOrDefault(p => p.Id == intId);

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
                    int total = Int32.Parse(response.Content.Headers.GetValues("Content-Range").FirstOrDefault().Split('/')[^1]);
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
                if (info == null) return 0;

                info.Id = _products.Count + 1;
                _products.Add(info);

                JObject jsonObject = JObject.FromObject(info, serializer);
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
                    JArray jsonArray = JArray.Parse(responseContent);
                    _products[^1].Id = jsonArray[0]["id"].ToObject<int>();
                    return 1;
                }
                else
                {
                    Debug.WriteLine("Failed insertion");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                }
                _products.RemoveAt(_products.Count - 1);
                return 0;
            }

            public int Insert(ProductModel info)
            {
                return Insert("", info);
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

        public class APIInventoryRepository : IRepository<InventoryModel>
        {
            private List<InventoryModel> _inventories;

            public APIInventoryRepository() { }
            public APIInventoryRepository(List<InventoryModel> list) { _inventories = list; }

            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var reponse = client.DeleteAsync($"{baseURL}/inventories?id=eq.{id}").Result;

                if (reponse.IsSuccessStatusCode)
                {
                    var item = _inventories.FirstOrDefault(r => r.Id == intId);
                    _inventories.Remove(item);
                    return 1;
                }
                else
                {
                    Debug.WriteLine("Failed deletion");
                    Debug.WriteLine(reponse.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }

            public List<InventoryModel> GetAll(int pageNumber = 1, int pageSize = 10, string sortBy = null, bool sortDescending = false, string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                if (_inventories != null)
                {
                    return _inventories;
                }
                HttpResponseMessage response = client.GetAsync($"{baseURL}/inventories?limit={pageSize}&offset={(pageNumber - 1) * pageSize}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;

                    List<InventoryModel> inventories = JsonConvert.DeserializeObject<List<InventoryModel>>(jsonResponse);

                    _inventories = inventories;

                    return inventories;
                }
                else
                {
                    Debug.WriteLine("Error: " + response.StatusCode);
                    return new List<InventoryModel>();
                }
            }

            public InventoryModel GetById(string id)
            {
                if (!int.TryParse(id, out int intId)) return null;
                var data = _inventories ?? GetAll();
                return data.FirstOrDefault(p => p.Id == intId);

                HttpResponseMessage response = client.GetAsync($"{baseURL}/inventories?id=eq.{id}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;
                    List<InventoryModel> inventories = JsonConvert.DeserializeObject<List<InventoryModel>>(jsonResponse);
                    if (inventories?.Count > 0)
                    {
                        return inventories[0];
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    Debug.WriteLine("Failed to get inventory by ID");
                    Debug.WriteLine("Error: " + response.StatusCode);
                    return null;
                }
            }

            public int GetCount(string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                client.DefaultRequestHeaders.Add("Prefer", "count=exact");
                var response = client.GetAsync($"{baseURL}/inventories").Result;
                if (response.IsSuccessStatusCode)
                {
                    int total = Int32.Parse(response.Content.Headers.GetValues("Content-Range").FirstOrDefault().Split('/')[^1]);
                    return total;
                }
                else
                {
                    Debug.WriteLine("Failed count total");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }

            public int Insert(string id, InventoryModel info)
            {
                if (info == null)
                {
                    return 0;
                }

                info.Id = _inventories.Count + 1;
                _inventories.Add(info);

                JObject jsonObject = JObject.FromObject(info, serializer);
                //Remove all object's properties that are not match with the columns
                jsonObject.Remove("Id");
                jsonObject.Remove("Inbound");
                JObject newJsonObject = new JObject();
                //keys must match with the columns in the database (lowercase)
                foreach (var pair in jsonObject.Properties())
                {
                    newJsonObject.Add(pair.Name.ToLower(), pair.Value);
                }

                string jsonContent = newJsonObject.ToString();
                StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{baseURL}/inventories")
                {
                    Content = content
                };
                LogRequestDetails(request);
                HttpResponseMessage response = client.Send(request);

                if (response.IsSuccessStatusCode)
                {
                    // Handle the successful response (e.g., read the response content)
                    string responseContent = response.Content.ReadAsStringAsync().Result;
                    Debug.WriteLine("Inventory inserted successfully: " + responseContent);
                    JArray jsonArray = JArray.Parse(responseContent);
                    _inventories[^1].Id = jsonArray[0]["id"].ToObject<int>();
                    return 1;
                }
                else
                {
                    Debug.WriteLine("Failed insertion");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    _inventories.RemoveAt(_inventories.Count - 1);
                    return 0;
                }
            }

            public int Insert(InventoryModel info)
            {
                return Insert("", info);
            }

            public int UpdateById(string id, InventoryModel info)
            {
                JObject jsonObject = JObject.FromObject(info);
                //Remove all object's properties that are not match with the columns
                jsonObject.Remove("Id");
                jsonObject.Remove("Inbound");
                JObject newJsonObject = new JObject();
                //keys must match with the columns in the database (lowercase)
                foreach (var pair in jsonObject.Properties())
                {
                    newJsonObject.Add(pair.Name.ToLower(), pair.Value);
                }

                string jsonContent = newJsonObject.ToString();
                StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, $"{baseURL}/inventories?id=eq.{id}")
                {
                    Content = content
                };
                LogRequestDetails(request);
                HttpResponseMessage response = client.Send(request);

                if (response.IsSuccessStatusCode)
                {
                    // Handle the successful response (e.g., read the response content)
                    string responseContent = response.Content.ReadAsStringAsync().Result;
                    Debug.WriteLine("Inventory inserted successfully: " + responseContent);
                    return 1;
                }
                else
                {
                    Debug.WriteLine("Failed update");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }
        }

        public class APIIngredientRepository : IRepository<IngredientModel>
        {
            private List<IngredientModel> _ingredients;
            public APIIngredientRepository() { }
            public APIIngredientRepository(List<IngredientModel> list) { _ingredients = list; }
            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var reponse = client.DeleteAsync($"{baseURL}/ingredients?id=eq.{id}").Result;

                if (reponse.IsSuccessStatusCode)
                {
                    var item = _ingredients.FirstOrDefault(r => r.Id == intId);
                    _ingredients.Remove(item);
                    return 1;
                }
                else
                {
                    Debug.WriteLine("Failed deletion");
                    Debug.WriteLine(reponse.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }

            public List<IngredientModel> GetAll(int pageNumber = 1, int pageSize = 10, string sortBy = null, bool sortDescending = false, string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                if (_ingredients != null)
                {
                    return _ingredients;
                }
                HttpResponseMessage response = client.GetAsync($"{baseURL}/ingredients?limit={pageSize}&offset={(pageNumber - 1) * pageSize}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;

                    List<IngredientModel> ingredients = JsonConvert.DeserializeObject<List<IngredientModel>>(jsonResponse);

                    _ingredients = ingredients;

                    return ingredients;
                }
                else
                {
                    Debug.WriteLine("Error: " + response.StatusCode);
                    return new List<IngredientModel>();
                }
            }

            public IngredientModel GetById(string id)
            {
                if (!int.TryParse(id, out int intId)) return null;
                var data = _ingredients ?? GetAll();
                return data.FirstOrDefault(p => p.Id == intId);

                HttpResponseMessage response = client.GetAsync($"{baseURL}/ingredients?id=eq.{id}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;
                    List<IngredientModel> products = JsonConvert.DeserializeObject<List<IngredientModel>>(jsonResponse);
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
                var response = client.GetAsync($"{baseURL}/ingredients").Result;
                if (response.IsSuccessStatusCode)
                {
                    int total = Int32.Parse(response.Content.Headers.GetValues("Content-Range").FirstOrDefault().Split('/')[^1]);
                    return total;
                }
                else
                {
                    Debug.WriteLine("Failed count total");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }

            public int Insert(string id, IngredientModel info)
            {
                Debug.WriteLine("Insertion");
                if (info == null) return 0;

                info.Id = _ingredients.Count + 1;
                _ingredients.Add(info);

                JObject jsonObject = JObject.FromObject(info, serializer);
                //Remove all object's properties that are not match with the columns
                jsonObject.Remove("Id");
                JObject newJsonObject = new JObject();
                //keys must match with the columns in the database (lowercase)
                foreach (var pair in jsonObject.Properties())
                {
                    newJsonObject.Add(pair.Name.ToLower(), pair.Value);
                }

                string jsonContent = newJsonObject.ToString();
                StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{baseURL}/ingredients")
                {
                    Content = content
                };
                LogRequestDetails(request);
                HttpResponseMessage response = client.Send(request);

                IngredientModel ingredient;
                if (response.IsSuccessStatusCode)
                {
                    // Handle the successful response (e.g., read the response content)
                    string responseContent = response.Content.ReadAsStringAsync().Result;
                    ingredient = JsonConvert.DeserializeObject<List<IngredientModel>>(responseContent)[0];
                    Debug.WriteLine("Ingredient inserted successfully: " + ingredient.Id);
                    JArray jsonArray = JArray.Parse(responseContent);
                    _ingredients[^1].Id = jsonArray[0]["id"].ToObject<int>();
                    return ingredient.Id;
                }
                else
                {
                    Debug.WriteLine("Failed insertion");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    _ingredients.RemoveAt(_ingredients.Count - 1);
                }
                return 0;
            }

            public int Insert(IngredientModel info)
            {
                return Insert("", info);
            }

            public int UpdateById(string id, IngredientModel info)
            {
                JObject jsonObject = JObject.FromObject(info);
                //Remove all object's properties that are not match with the columns
                jsonObject.Remove("Id");
                JObject newJsonObject = new JObject();
                //keys must match with the columns in the database (lowercase)
                foreach (var pair in jsonObject.Properties())
                {
                    newJsonObject.Add(pair.Name.ToLower(), pair.Value);
                }
                StringContent content = new StringContent(newJsonObject.ToString(), Encoding.UTF8, "application/json");
                var reponse = client.PatchAsync($"{baseURL}/ingredients?id=eq.{id}", content).Result;

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

        public class APISupplierRepository : IRepository<SupplierModel>
        {
            private List<SupplierModel> _suppliers;
            public APISupplierRepository() { }
            public APISupplierRepository(List<SupplierModel> list) { _suppliers = list; }

            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var reponse = client.DeleteAsync($"{baseURL}/suppliers?id=eq.{id}").Result;

                if (reponse.IsSuccessStatusCode)
                {
                    var item = _suppliers.FirstOrDefault(r => r.Id == intId);
                    _suppliers.Remove(item);
                    return 1;
                }
                else
                {
                    Debug.WriteLine("Failed deletion");
                    Debug.WriteLine(reponse.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }

            public List<SupplierModel> GetAll(int pageNumber = 1, int pageSize = 10, string sortBy = null, bool sortDescending = false, string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                if (_suppliers != null)
                {
                    return _suppliers;
                }
                HttpResponseMessage response = client.GetAsync($"{baseURL}/suppliers?limit={pageSize}&offset={(pageNumber - 1) * pageSize}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;

                    List<SupplierModel> suppliers = JsonConvert.DeserializeObject<List<SupplierModel>>(jsonResponse);

                    _suppliers = suppliers;

                    return suppliers;
                }
                else
                {
                    Debug.WriteLine("Error: " + response.StatusCode);
                    return new List<SupplierModel>();
                }
            }

            public SupplierModel GetById(string id)
            {
                if (!int.TryParse(id, out int intId)) return null;
                var data = _suppliers ?? GetAll();
                return data.FirstOrDefault(p => p.Id == intId);

                HttpResponseMessage response = client.GetAsync($"{baseURL}/suppliers?id=eq.{id}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;
                    List<SupplierModel> suppliers = JsonConvert.DeserializeObject<List<SupplierModel>>(jsonResponse);
                    if (suppliers?.Count > 0)
                    {
                        return suppliers[0];
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    Debug.WriteLine("Failed to get supplier by ID");
                    Debug.WriteLine("Error: " + response.StatusCode);
                    return null;
                }
            }

            public int GetCount(string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                client.DefaultRequestHeaders.Add("Prefer", "count=exact");
                var response = client.GetAsync($"{baseURL}/suppliers").Result;
                if (response.IsSuccessStatusCode)
                {
                    int total = Int32.Parse(response.Content.Headers.GetValues("Content-Range").FirstOrDefault().Split('/')[^1]);
                    return total;
                }
                else
                {
                    Debug.WriteLine("Failed count total");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }

            public int Insert(string id, SupplierModel info)
            {
                if (info == null) return 0;

                info.Id = _suppliers.Count + 1;
                _suppliers.Add(info);

                JObject jsonObject = JObject.FromObject(info, serializer);
                //Remove all object's properties that are not match with the columns
                jsonObject.Remove("Id");
                jsonObject.Remove("Inbounds");
                JObject newJsonObject = new JObject();
                //keys must match with the columns in the database (lowercase)
                foreach (var pair in jsonObject.Properties())
                {
                    newJsonObject.Add(pair.Name.ToLower(), pair.Value);
                }

                string jsonContent = newJsonObject.ToString();
                StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{baseURL}/suppliers")
                {
                    Content = content
                };
                LogRequestDetails(request);
                HttpResponseMessage response = client.Send(request);

                if (response.IsSuccessStatusCode)
                {
                    // Handle the successful response (e.g., read the response content)
                    string responseContent = response.Content.ReadAsStringAsync().Result;
                    Debug.WriteLine("Supplier inserted successfully: " + responseContent);
                    JArray jsonArray = JArray.Parse(responseContent);
                    _suppliers[^1].Id = jsonArray[0]["id"].ToObject<int>();
                    return 1;
                }
                else
                {
                    Debug.WriteLine("Failed insertion");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    _suppliers.RemoveAt(_suppliers.Count - 1);
                    return 0;
                }
            }

            public int Insert(SupplierModel info)
            {
                return Insert("", info);
            }

            public int UpdateById(string id, SupplierModel info)
            {
                JObject jsonObject = JObject.FromObject(info);
                //Remove all object's properties that are not match with the columns
                jsonObject.Remove("Id");
                jsonObject.Remove("Inbounds");
                JObject newJsonObject = new JObject();
                //keys must match with the columns in the database (lowercase)
                foreach (var pair in jsonObject.Properties())
                {
                    newJsonObject.Add(pair.Name.ToLower(), pair.Value);
                }
                StringContent content = new StringContent(newJsonObject.ToString(), Encoding.UTF8, "application/json");
                var reponse = client.PatchAsync($"{baseURL}/suppliers?id=eq.{id}", content).Result;

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

        public class APIInboundRepository : IRepository<InboundModel>
        {
            private List<InboundModel> _inbounds;
            public APIInboundRepository() { }
            public APIInboundRepository(List<InboundModel> list) { _inbounds = list; }

            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var reponse = client.DeleteAsync($"{baseURL}/inbounds?id=eq.{id}").Result;

                if (reponse.IsSuccessStatusCode)
                {
                    var item = _inbounds.FirstOrDefault(r => r.Id == intId);
                    _inbounds.Remove(item);
                    return 1;
                }
                else
                {
                    Debug.WriteLine("Failed deletion");
                    Debug.WriteLine(reponse.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }

            public List<InboundModel> GetAll(int pageNumber = 1, int pageSize = 10, string sortBy = null, bool sortDescending = false, string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                if (_inbounds != null)
                {
                    return _inbounds;
                }
                HttpResponseMessage response = client.GetAsync($"{baseURL}/inbounds?limit={pageSize}&offset={(pageNumber - 1) * pageSize}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;

                    List<InboundModel> inbounds = JsonConvert.DeserializeObject<List<InboundModel>>(jsonResponse);

                    _inbounds = inbounds;

                    return inbounds;
                }
                else
                {
                    Debug.WriteLine("Error: " + response.StatusCode);
                    return new List<InboundModel>();
                }
            }

            public InboundModel GetById(string id)
            {
                if (!int.TryParse(id, out int intId)) return null;
                var data = _inbounds ?? GetAll();
                return data.FirstOrDefault(p => p.Id == intId);

                HttpResponseMessage response = client.GetAsync($"{baseURL}/inbounds?id=eq.{id}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;
                    List<InboundModel> inbounds = JsonConvert.DeserializeObject<List<InboundModel>>(jsonResponse);
                    if (inbounds?.Count > 0)
                    {
                        return inbounds[0];
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    Debug.WriteLine("Failed to get inbound by ID");
                    Debug.WriteLine("Error: " + response.StatusCode);
                    return null;
                }
            }

            public int GetCount(string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                client.DefaultRequestHeaders.Add("Prefer", "count=exact");
                var response = client.GetAsync($"{baseURL}/inbounds").Result;
                if (response.IsSuccessStatusCode)
                {
                    int total = Int32.Parse(response.Content.Headers.GetValues("Content-Range").FirstOrDefault().Split('/')[^1]);
                    return total;
                }
                else
                {
                    Debug.WriteLine("Failed count total");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }
            public int Insert(string id, InboundModel info)
            {
                if (info == null) return 0;

                info.Id = _inbounds.Count + 1;
                _inbounds.Add(info);

                JObject jsonObject = JObject.FromObject(info, serializer);
                //Remove all object's properties that are not match with the columns
                jsonObject.Remove("Id");
                jsonObject.Remove("Ingredient");
                jsonObject.Remove("Supplier");
                JObject newJsonObject = new JObject();
                //keys must match with the columns in the database (lowercase)
                foreach (var pair in jsonObject.Properties())
                {
                    newJsonObject.Add(pair.Name.ToLower(), pair.Value);
                }

                string jsonContent = newJsonObject.ToString();
                StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{baseURL}/inbounds")
                {
                    Content = content
                };
                LogRequestDetails(request);
                HttpResponseMessage response = client.Send(request);

                if (response.IsSuccessStatusCode)
                {
                    // Handle the successful response (e.g., read the response content)
                    string responseContent = response.Content.ReadAsStringAsync().Result;
                    Debug.WriteLine("Inbound inserted successfully: " + responseContent);
                    JArray jsonArray = JArray.Parse(responseContent);
                    _inbounds[^1].Id = jsonArray[0]["id"].ToObject<int>();
                    return 1;
                }
                else
                {
                    Debug.WriteLine("Failed insertion");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    _inbounds.RemoveAt(_inbounds.Count - 1);
                    return 0;
                }
            }

            public int Insert(InboundModel info)
            {
                throw new NotImplementedException();
            }

            public int UpdateById(string id, InboundModel info)
            {
                JObject jsonObject = JObject.FromObject(info);
                //Remove all object's properties that are not match with the columns
                jsonObject.Remove("Id");
                jsonObject.Remove("Inventories");
                JObject newJsonObject = new JObject();
                //keys must match with the columns in the database (lowercase)
                foreach (var pair in jsonObject.Properties())
                {
                    newJsonObject.Add(pair.Name.ToLower(), pair.Value);
                }
                StringContent content = new StringContent(newJsonObject.ToString(), Encoding.UTF8, "application/json");
                var reponse = client.PatchAsync($"{baseURL}/inbounds?id=eq.{id}", content).Result;

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

        public class APIOutboundRepository : IRepository<OutboundModel>
        {
            private List<OutboundModel> _outbounds;
            public APIOutboundRepository() { }
            public APIOutboundRepository(List<OutboundModel> list) { _outbounds = list; }

            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var reponse = client.DeleteAsync($"{baseURL}/outbounds?id=eq.{id}").Result;

                if (reponse.IsSuccessStatusCode)
                {
                    var item = _outbounds.FirstOrDefault(r => r.Id == intId);
                    _outbounds.Remove(item);
                    return 1;
                }
                else
                {
                    Debug.WriteLine("Failed deletion");
                    Debug.WriteLine(reponse.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }

            public List<OutboundModel> GetAll(int pageNumber = 1, int pageSize = 10, string sortBy = null, bool sortDescending = false, string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                if (_outbounds != null)
                {
                    return _outbounds;
                }
                HttpResponseMessage response = client.GetAsync($"{baseURL}/outbounds?limit={pageSize}&offset={(pageNumber - 1) * pageSize}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;

                    List<OutboundModel> outbounds = JsonConvert.DeserializeObject<List<OutboundModel>>(jsonResponse);

                    _outbounds = outbounds;

                    return outbounds;
                }
                else
                {
                    Debug.WriteLine("Error: " + response.StatusCode);
                    return new List<OutboundModel>();
                }
            }

            public OutboundModel GetById(string id)
            {
                if (!int.TryParse(id, out int intId)) return null;
                var data = _outbounds ?? GetAll();
                return data.FirstOrDefault(p => p.Id == intId);

                HttpResponseMessage response = client.GetAsync($"{baseURL}/outbounds?id=eq.{id}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;
                    List<OutboundModel> outbounds = JsonConvert.DeserializeObject<List<OutboundModel>>(jsonResponse);
                    if (outbounds?.Count > 0)
                    {
                        return outbounds[0];
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    Debug.WriteLine("Failed to get outbound by ID");
                    Debug.WriteLine("Error: " + response.StatusCode);
                    return null;
                }
            }

            public int GetCount(string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                client.DefaultRequestHeaders.Add("Prefer", "count=exact");
                var response = client.GetAsync($"{baseURL}/outbounds").Result;
                if (response.IsSuccessStatusCode)
                {
                    int total = Int32.Parse(response.Content.Headers.GetValues("Content-Range").FirstOrDefault().Split('/')[^1]);
                    return total;
                }
                else
                {
                    Debug.WriteLine("Failed count total");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }
            public int Insert(string id, OutboundModel info)
            {
                if (info == null)
                {
                    return 0;
                }

                info.Id = _outbounds.Count + 1;
                _outbounds.Add(info);

                JObject jsonObject = JObject.FromObject(info, serializer);
                //Remove all object's properties that are not match with the columns
                jsonObject.Remove("Id");
                jsonObject.Remove("Inventory");
                JObject newJsonObject = new JObject();
                //keys must match with the columns in the database (lowercase)
                foreach (var pair in jsonObject.Properties())
                {
                    newJsonObject.Add(pair.Name.ToLower(), pair.Value);
                }

                string jsonContent = newJsonObject.ToString();
                StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{baseURL}/outbounds")
                {
                    Content = content
                };
                LogRequestDetails(request);
                HttpResponseMessage response = client.Send(request);

                if (response.IsSuccessStatusCode)
                {
                    // Handle the successful response (e.g., read the response content)
                    string responseContent = response.Content.ReadAsStringAsync().Result;
                    Debug.WriteLine("Outbound inserted successfully: " + responseContent);
                    JArray jsonArray = JArray.Parse(responseContent);
                    _outbounds[^1].Id = jsonArray[0]["id"].ToObject<int>();
                    return 1;
                }
                else
                {
                    Debug.WriteLine("Failed insertion");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    _outbounds.RemoveAt(_outbounds.Count - 1);
                    return 0;
                }
            }

            public int Insert(OutboundModel info)
            {
                return Insert("", info);
            }

            public int UpdateById(string id, OutboundModel info)
            {
                JObject jsonObject = JObject.FromObject(info);
                //Remove all object's properties that are not match with the columns
                jsonObject.Remove("Id");
                jsonObject.Remove("Inventories");
                JObject newJsonObject = new JObject();
                //keys must match with the columns in the database (lowercase)
                foreach (var pair in jsonObject.Properties())
                {
                    newJsonObject.Add(pair.Name.ToLower(), pair.Value);
                }
                StringContent content = new StringContent(newJsonObject.ToString(), Encoding.UTF8, "application/json");
                var reponse = client.PatchAsync($"{baseURL}/outbounds?id=eq.{id}", content).Result;

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

        public class APICustomerRepository : IRepository<CustomerModel>
        {
            private List<CustomerModel> _customers;
            public APICustomerRepository() { }
            public APICustomerRepository(List<CustomerModel> list) { _customers = list; }

            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var reponse = client.DeleteAsync($"{baseURL}/customers?id=eq.{id}").Result;

                if (reponse.IsSuccessStatusCode)
                {
                    var item = _customers.FirstOrDefault(r => r.Id == intId);
                    _customers.Remove(item);
                    return 1;
                }
                else
                {
                    Debug.WriteLine("Failed deletion");
                    Debug.WriteLine(reponse.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }

            public List<CustomerModel> GetAll(int pageNumber = 1, int pageSize = 10, string sortBy = null, bool sortDescending = false, string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                if (_customers != null)
                {
                    return _customers;
                }
                HttpResponseMessage response = client.GetAsync($"{baseURL}/customers?limit={pageSize}&offset={(pageNumber - 1) * pageSize}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;

                    List<CustomerModel> customers = JsonConvert.DeserializeObject<List<CustomerModel>>(jsonResponse);

                    _customers = customers;

                    return customers;
                }
                else
                {
                    Debug.WriteLine("Error: " + response.StatusCode);
                    return new List<CustomerModel>();
                }
            }

            public CustomerModel GetById(string id)
            {
                if (!int.TryParse(id, out int intId)) return null;
                var data = _customers ?? GetAll();
                return data.FirstOrDefault(p => p.Id == intId);

                HttpResponseMessage response = client.GetAsync($"{baseURL}/customers?id=eq.{id}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;
                    List<CustomerModel> customers = JsonConvert.DeserializeObject<List<CustomerModel>>(jsonResponse);
                    if (customers?.Count > 0)
                    {
                        return customers[0];
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    Debug.WriteLine("Failed to get customer by ID");
                    Debug.WriteLine("Error: " + response.StatusCode);
                    return null;
                }
            }

            public int GetCount(string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                client.DefaultRequestHeaders.Add("Prefer", "count=exact");
                var response = client.GetAsync($"{baseURL}/customers").Result;
                if (response.IsSuccessStatusCode)
                {
                    int
                    total = Int32.Parse(response.Content.Headers.GetValues("Content-Range").FirstOrDefault().Split('/')[^1]);
                    return total;
                }
                else
                {
                    Debug.WriteLine("Failed count total");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }
            public int Insert(string id, CustomerModel info)
            {
                if (info == null)
                {
                    return 0;
                }

                info.Id = _customers.Count + 1;
                _customers.Add(info);

                JObject jsonObject = JObject.FromObject(info, serializer);
                //Remove all object's properties that are not match with the columns
                jsonObject.Remove("Id");
                jsonObject.Remove("Invoices");
                JObject newJsonObject = new JObject();
                //keys must match with the columns in the database (lowercase)
                foreach (var pair in jsonObject.Properties())
                {
                    newJsonObject.Add(pair.Name.ToLower(), pair.Value);
                }

                string jsonContent = newJsonObject.ToString();
                StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{baseURL}/customers")
                {
                    Content = content
                };
                LogRequestDetails(request);
                HttpResponseMessage response = client.Send(request);

                if (response.IsSuccessStatusCode)
                {
                    // Handle the successful response (e.g., read the response content)
                    string responseContent = response.Content.ReadAsStringAsync().Result;
                    Debug.WriteLine("Customer inserted successfully: " + responseContent);
                    JArray jsonArray = JArray.Parse(responseContent);
                    _customers[^1].Id = jsonArray[0]["id"].ToObject<int>();
                    return 1;
                }
                else
                {
                    Debug.WriteLine("Failed insertion");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    _customers.RemoveAt(_customers.Count - 1);
                    return 0;
                }
            }

            public int Insert(CustomerModel info)
            {
                return Insert("", info);
            }

            public int UpdateById(string id, CustomerModel info)
            {
                JObject jsonObject = JObject.FromObject(info);
                //Remove all object's properties that are not match with the columns
                jsonObject.Remove("Id");
                jsonObject.Remove("Invoices");
                JObject newJsonObject = new JObject();
                //keys must match with the columns in the database (lowercase)
                foreach (var pair in jsonObject.Properties())
                {
                    newJsonObject.Add(pair.Name.ToLower(), pair.Value);
                }
                StringContent content = new StringContent(newJsonObject.ToString(), Encoding.UTF8, "application/json");
                var reponse = client.PatchAsync($"{baseURL}/customers?id=eq.{id}", content).Result;

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

        public class APIExpenseCategoryRepository : IRepository<ExpenseCategoryModel>
        {
            private List<ExpenseCategoryModel> _expenseCategories;
            public APIExpenseCategoryRepository() { }
            public APIExpenseCategoryRepository(List<ExpenseCategoryModel> list) { _expenseCategories = list; }

            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var reponse = client.DeleteAsync($"{baseURL}/expensecategories?id=eq.{id}").Result;

                if (reponse.IsSuccessStatusCode)
                {
                    var item = _expenseCategories.FirstOrDefault(r => r.Id == intId);
                    _expenseCategories.Remove(item);
                    return 1;
                }
                else
                {
                    Debug.WriteLine("Failed deletion");
                    Debug.WriteLine(reponse.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }

            public List<ExpenseCategoryModel> GetAll(int pageNumber = 1, int pageSize = 10, string sortBy = null, bool sortDescending = false, string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                if (_expenseCategories != null)
                {
                    return _expenseCategories;
                }
                HttpResponseMessage response = client.GetAsync($"{baseURL}/expensecategories?limit={pageSize}&offset={(pageNumber - 1) * pageSize}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;

                    List<ExpenseCategoryModel> expenseCategories = JsonConvert.DeserializeObject<List<ExpenseCategoryModel>>(jsonResponse);

                    _expenseCategories = expenseCategories;

                    return expenseCategories;
                }
                else
                {
                    Debug.WriteLine("Error: " + response.StatusCode);
                    return new List<ExpenseCategoryModel>();
                }
            }

            public ExpenseCategoryModel GetById(string id)
            {
                if (!int.TryParse(id, out int intId)) return null;
                var data = _expenseCategories ?? GetAll();
                return data.FirstOrDefault(p => p.Id == intId);

                HttpResponseMessage response = client.GetAsync($"{baseURL}/expensecategories?id=eq.{id}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;
                    List<ExpenseCategoryModel> expenseCategories = JsonConvert.DeserializeObject<List<ExpenseCategoryModel>>(jsonResponse);
                    if (expenseCategories?.Count > 0)
                    {
                        return expenseCategories[0];
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    Debug.WriteLine("Failed to get expense category by ID");
                    Debug.WriteLine("Error: " + response.StatusCode);
                    return null;
                }
            }

            public int GetCount(string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                client.DefaultRequestHeaders.Add("Prefer", "count=exact");
                var response = client.GetAsync($"{baseURL}/expensecategories").Result;
                if (response.IsSuccessStatusCode)
                {
                    int total = Int32.Parse(response.Content.Headers.GetValues("Content-Range").FirstOrDefault().Split('/')[^1]);
                    return total;
                }
                else
                {
                    Debug.WriteLine("Failed count total");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }
            public int Insert(string id, ExpenseCategoryModel info)
            {
                if (info == null)
                {
                    return 0;
                }

                info.Id = _expenseCategories.Count + 1;
                _expenseCategories.Add(info);

                JObject jsonObject = JObject.FromObject(info, serializer);
                //Remove all object's properties that are not match with the columns
                jsonObject.Remove("Id");
                jsonObject.Remove("Expenses");
                JObject newJsonObject = new JObject();
                //keys must match with the columns in the database (lowercase)
                foreach (var pair in jsonObject.Properties())
                {
                    newJsonObject.Add(pair.Name.ToLower(), pair.Value);
                }

                string jsonContent = newJsonObject.ToString();
                StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{baseURL}/expensecategories")
                {
                    Content = content
                };
                LogRequestDetails(request);
                HttpResponseMessage response = client.Send(request);

                if (response.IsSuccessStatusCode)
                {
                    // Handle the successful response (e.g., read the response content)
                    string responseContent = response.Content.ReadAsStringAsync().Result;
                    Debug.WriteLine("Expense category inserted successfully: " + responseContent);
                    JArray jsonArray = JArray.Parse(responseContent);
                    _expenseCategories[^1].Id = jsonArray[0]["id"].ToObject<int>();
                    return 1;
                }
                else
                {
                    Debug.WriteLine("Failed insertion");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    _expenseCategories.RemoveAt(_expenseCategories.Count - 1);
                    return 0;
                }
            }

            public int Insert(ExpenseCategoryModel info)
            {
                return Insert("", info);
            }

            public int UpdateById(string id, ExpenseCategoryModel info)
            {
                JObject jsonObject = JObject.FromObject(info);
                //Remove all object's properties that are not match with the columns
                jsonObject.Remove("Id");
                jsonObject.Remove("Expenses");
                JObject newJsonObject = new JObject();
                //keys must match with the columns in the database (lowercase)
                foreach (var pair in jsonObject.Properties())
                {
                    newJsonObject.Add(pair.Name.ToLower(), pair.Value);
                }
                StringContent content = new StringContent(newJsonObject.ToString(), Encoding.UTF8, "application/json");
                var reponse = client.PatchAsync($"{baseURL}/expensecategories?id=eq.{id}", content).Result;

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

        public class APICheckInventoryRepository : IRepository<CheckInventoryModel>
        {
            private List<CheckInventoryModel> _checkInventories;
            public APICheckInventoryRepository() { }
            public APICheckInventoryRepository(List<CheckInventoryModel> list) { _checkInventories = list; }

            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var reponse = client.DeleteAsync($"{baseURL}/checkinventories?id=eq.{id}").Result;

                if (reponse.IsSuccessStatusCode)
                {
                    var item = _checkInventories.FirstOrDefault(r => r.Id == intId);
                    _checkInventories.Remove(item);
                    return 1;
                }
                else
                {
                    Debug.WriteLine("Failed deletion");
                    Debug.WriteLine(reponse.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }

            public List<CheckInventoryModel> GetAll(int pageNumber = 1, int pageSize = 10, string sortBy = null, bool sortDescending = false, string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                if (_checkInventories != null)
                {
                    return _checkInventories;
                }
                HttpResponseMessage response = client.GetAsync($"{baseURL}/checkinventories?limit={pageSize}&offset={(pageNumber - 1) * pageSize}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;

                    List<CheckInventoryModel> checkInventories = JsonConvert.DeserializeObject<List<CheckInventoryModel>>(jsonResponse);

                    _checkInventories = checkInventories;

                    return checkInventories;
                }
                else
                {
                    Debug.WriteLine("Error: " + response.StatusCode);
                    return new List<CheckInventoryModel>();
                }
            }

            public CheckInventoryModel GetById(string id)
            {
                if (!int.TryParse(id, out int intId)) return null;
                var data = _checkInventories ?? GetAll();
                return data.FirstOrDefault(p => p.Id == intId);


                HttpResponseMessage response = client.GetAsync($"{baseURL}/checkinventories?id=eq.{id}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;
                    List<CheckInventoryModel> checkInventories = JsonConvert.DeserializeObject<List<CheckInventoryModel>>(jsonResponse);
                    if (checkInventories?.Count > 0)
                    {
                        return checkInventories[0];
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    Debug.WriteLine("Failed to get check inventory by ID");
                    Debug.WriteLine("Error: " + response.StatusCode);
                    return null;
                }
            }

            public int GetCount(string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                client.DefaultRequestHeaders.Add("Prefer", "count=exact");
                var response = client.GetAsync($"{baseURL}/checkinventories").Result;
                if (response.IsSuccessStatusCode)
                {
                    int total = Int32.Parse(response.Content.Headers.GetValues("Content-Range").FirstOrDefault().Split('/')[^1]);
                    return total;
                }
                else
                {
                    Debug.WriteLine("Failed count total");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }
            public int Insert(string id, CheckInventoryModel info)
            {
                if (info == null)
                {
                    return 0;
                }

                info.Id = _checkInventories.Count + 1;
                _checkInventories.Add(info);

                JObject jsonObject = JObject.FromObject(info, serializer);
                //Remove all object's properties that are not match with the columns
                jsonObject.Remove("Id");
                jsonObject.Remove("Inventory");
                jsonObject.Remove("Discrepancy");
                JObject newJsonObject = new JObject();
                //keys must match with the columns in the database (lowercase)
                foreach (var pair in jsonObject.Properties())
                {
                    newJsonObject.Add(pair.Name.ToLower(), pair.Value);
                }

                string jsonContent = newJsonObject.ToString();
                StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{baseURL}/checkinventories")
                {
                    Content = content
                };
                LogRequestDetails(request);
                HttpResponseMessage response = client.Send(request);

                if (response.IsSuccessStatusCode)
                {
                    // Handle the successful response (e.g., read the response content)
                    string responseContent = response.Content.ReadAsStringAsync().Result;
                    Debug.WriteLine("Check inventory inserted successfully: " + responseContent);
                    JArray jsonArray = JArray.Parse(responseContent);
                    _checkInventories[^1].Id = jsonArray[0]["id"].ToObject<int>();
                    return 1;
                }
                else
                {
                    Debug.WriteLine("Failed insertion");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    _checkInventories.RemoveAt(_checkInventories.Count - 1);
                    return 0;
                }
            }

            public int Insert(CheckInventoryModel info)
            {
                return Insert("", info);
            }

            public int UpdateById(string id, CheckInventoryModel info)
            {
                JObject jsonObject = JObject.FromObject(info);
                //Remove all object's properties that are not match with the columns
                jsonObject.Remove("Id");
                jsonObject.Remove("Inventory");
                jsonObject.Remove("Discrepancy");
                JObject newJsonObject = new JObject();
                //keys must match with the columns in the database (lowercase)
                foreach (var pair in jsonObject.Properties())
                {
                    newJsonObject.Add(pair.Name.ToLower(), pair.Value);
                }
                StringContent content = new StringContent(newJsonObject.ToString(), Encoding.UTF8, "application/json");
                var reponse = client.PatchAsync($"{baseURL}/checkinventories?id=eq.{id}", content).Result;

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


        public class APIProductVariantRepository : IRepository<ProductVariantModel>
        {
            private List<ProductVariantModel> _productVariants;
            public APIProductVariantRepository() { }
            public APIProductVariantRepository(List<ProductVariantModel> list) { _productVariants = list; }

            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var reponse = client.DeleteAsync($"{baseURL}/productvariants?id=eq.{id}").Result;

                if (reponse.IsSuccessStatusCode)
                {
                    var item = _productVariants.FirstOrDefault(r => r.Id == intId);
                    _productVariants.Remove(item);
                    return 1;
                }
                else
                {
                    Debug.WriteLine("Failed deletion");
                    Debug.WriteLine(reponse.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }

            public List<ProductVariantModel> GetAll(int pageNumber = 1, int pageSize = 10, string sortBy = null, bool sortDescending = false, string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                if (_productVariants != null)
                {
                    return _productVariants;
                }
                HttpResponseMessage response = client.GetAsync($"{baseURL}/productvariants?limit={pageSize}&offset={(pageNumber - 1) * pageSize}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;

                    List<ProductVariantModel> productVariants = JsonConvert.DeserializeObject<List<ProductVariantModel>>(jsonResponse);

                    _productVariants = productVariants;

                    return productVariants;
                }
                else
                {
                    Debug.WriteLine("Error: " + response.StatusCode);
                    return new List<ProductVariantModel>();
                }
            }

            public ProductVariantModel GetById(string id)
            {
                if (!int.TryParse(id, out int intId)) return null;
                var data = _productVariants ?? GetAll();
                return data.FirstOrDefault(p => p.Id == intId);

                HttpResponseMessage response = client.GetAsync($"{baseURL}/productvariants?id=eq.{id}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;
                    List<ProductVariantModel> productVariants = JsonConvert.DeserializeObject<List<ProductVariantModel>>(jsonResponse);
                    if (productVariants?.Count > 0)
                    {
                        return productVariants[0];
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    Debug.WriteLine("Failed to get product variant by ID");
                    Debug.WriteLine("Error: " + response.StatusCode);
                    return null;
                }
            }

            public int GetCount(string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                client.DefaultRequestHeaders.Add("Prefer", "count=exact");
                var response = client.GetAsync($"{baseURL}/productvariants").Result;
                if (response.IsSuccessStatusCode)
                {
                    int total = Int32.Parse(response.Content.Headers.GetValues("Content-Range").FirstOrDefault().Split('/')[^1]);
                    return total;
                }
                else
                {
                    Debug.WriteLine("Failed count total");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }
            public int Insert(string id, ProductVariantModel info)
            {
                if (info == null)
                {
                    return 0;
                }

                info.Id = _productVariants.Count + 1;
                _productVariants.Add(info);

                JObject jsonObject = JObject.FromObject(info, serializer);
                //Remove all object's properties that are not match with the columns
                jsonObject.Remove("Id");
                jsonObject.Remove("Product");
                jsonObject.Remove("InvoiceDetails");
                jsonObject.Remove("Toppings");
                jsonObject.Remove("RecipeDetails");

                JObject newJsonObject = new JObject();
                //keys must match with the columns in the database (lowercase)
                foreach (var pair in jsonObject.Properties())
                {
                    newJsonObject.Add(pair.Name.ToLower(), pair.Value);
                }

                string jsonContent = newJsonObject.ToString();
                StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{baseURL}/productvariants")
                {
                    Content = content
                };
                LogRequestDetails(request);
                HttpResponseMessage response = client.Send(request);

                if (response.IsSuccessStatusCode)
                {
                    // Handle the successful response (e.g., read the response content)
                    string responseContent = response.Content.ReadAsStringAsync().Result;
                    Debug.WriteLine("Product variant inserted successfully: " + responseContent);
                    JArray jsonArray = JArray.Parse(responseContent);
                    _productVariants[^1].Id = jsonArray[0]["id"].ToObject<int>();
                    return 1;
                }
                else
                {
                    Debug.WriteLine("Failed insertion");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    _productVariants.RemoveAt(_productVariants.Count - 1);
                    return 0;
                }
            }

            public int Insert(ProductVariantModel info)
            {
                return Insert("", info);
            }

            public int UpdateById(string id, ProductVariantModel info)
            {
                JObject jsonObject = JObject.FromObject(info);
                //Remove all object's properties that are not match with the columns
                jsonObject.Remove("Id");
                jsonObject.Remove("Product");
                jsonObject.Remove("InvoiceDetails");
                jsonObject.Remove("Toppings");
                jsonObject.Remove("RecipeDetails");

                JObject newJsonObject = new JObject();
                //keys must match with the columns in the database (lowercase)
                foreach (var pair in jsonObject.Properties())
                {
                    newJsonObject.Add(pair.Name.ToLower(), pair.Value);
                }
                StringContent content = new StringContent(newJsonObject.ToString(), Encoding.UTF8,
                    "application/json");
                var reponse = client.PatchAsync($"{baseURL}/productvariants?id=eq.{id}", content).Result;

                if (reponse.IsSuccessStatusCode)
                {
                    return 1;
                }
                else
                {
                    Debug.WriteLine("Failed to update product variant by ID");
                    Debug.WriteLine(reponse.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }
        }

        public class APIExpenseRepository : IRepository<ExpenseModel>
        {
            private List<ExpenseModel> _expenses;
            public APIExpenseRepository() { }
            public APIExpenseRepository(List<ExpenseModel> list) { _expenses = list; }

            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var reponse = client.DeleteAsync($"{baseURL}/expenses?id=eq.{id}").Result;

                if (reponse.IsSuccessStatusCode)
                {
                    var item = _expenses.FirstOrDefault(r => r.Id == intId);
                    _expenses.Remove(item);
                    return 1;
                }
                else
                {
                    Debug.WriteLine("Failed deletion");
                    Debug.WriteLine(reponse.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }

            public List<ExpenseModel> GetAll(int pageNumber = 1, int pageSize = 10, string sortBy = null, bool sortDescending = false, string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                if (_expenses != null)
                {
                    return _expenses;
                }
                HttpResponseMessage response = client.GetAsync($"{baseURL}/expenses?limit={pageSize}&offset={(pageNumber - 1) * pageSize}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;

                    List<ExpenseModel> expenses = JsonConvert.DeserializeObject<List<ExpenseModel>>(jsonResponse);

                    _expenses = expenses;

                    return expenses;
                }
                else
                {
                    Debug.WriteLine("Error: " + response.StatusCode);
                    return new List<ExpenseModel>();
                }
            }

            public ExpenseModel GetById(string id)
            {
                if (!int.TryParse(id, out int intId)) return null;
                var data = _expenses ?? GetAll();
                return data.FirstOrDefault(p => p.Id == intId);

                HttpResponseMessage response = client.GetAsync($"{baseURL}/expenses?id=eq.{id}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;
                    List<ExpenseModel> expenses = JsonConvert.DeserializeObject<List<ExpenseModel>>(jsonResponse);
                    if (expenses?.Count > 0)
                    {
                        return expenses[0];
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    Debug.WriteLine("Failed to get expense by ID");
                    Debug.WriteLine("Error: " + response.StatusCode);
                    return null;
                }
            }

            public int GetCount(string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                client.DefaultRequestHeaders.Add("Prefer", "count=exact");
                var response = client.GetAsync($"{baseURL}/expenses").Result;
                if (response.IsSuccessStatusCode)
                {
                    int total = Int32.Parse(response.Content.Headers.GetValues("Content-Range").FirstOrDefault().Split('/')[^1]);
                    return total;
                }
                else
                {
                    Debug.WriteLine("Failed count total");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }
            public int Insert(string id, ExpenseModel info)
            {
                if (info == null)
                {
                    return 0;
                }

                info.Id = _expenses.Count + 1;
                _expenses.Add(info);

                JObject jsonObject = JObject.FromObject(info, serializer);
                //Remove all object's properties that are not match with the columns
                jsonObject.Remove("Id");
                jsonObject.Remove("ExpenseCategory");

                JObject newJsonObject = new JObject();
                //keys must match with the columns in the database (lowercase)
                foreach (var pair in jsonObject.Properties())
                {
                    newJsonObject.Add(pair.Name.ToLower(), pair.Value);
                }

                string jsonContent = newJsonObject.ToString();
                StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{baseURL}/expenses")
                {
                    Content = content
                };
                LogRequestDetails(request);
                HttpResponseMessage response = client.Send(request);

                if (response.IsSuccessStatusCode)
                {
                    // Handle the successful response (e.g., read the response content)
                    string responseContent = response.Content.ReadAsStringAsync().Result;
                    Debug.WriteLine("Expense inserted successfully: " + responseContent);
                    JArray jsonArray = JArray.Parse(responseContent);
                    _expenses[^1].Id = jsonArray[0]["id"].ToObject<int>();
                    return 1;
                }
                else
                {
                    Debug.WriteLine("Failed insertion");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    _expenses.RemoveAt(_expenses.Count - 1);
                    return 0;
                }
            }
            public int Insert(ExpenseModel info)
            {
                return Insert("", info);
            }
            public int UpdateById(string id, ExpenseModel info)
            {
                JObject jsonObject = JObject.FromObject(info);
                //Remove all object's properties that are not match with the columns
                jsonObject.Remove("Id");
                jsonObject.Remove("ExpenseCategory");

                JObject newJsonObject = new JObject();
                //keys must match with the columns in the database (lowercase)
                foreach (var pair in jsonObject.Properties())
                {
                    newJsonObject.Add(pair.Name.ToLower(), pair.Value);
                }
                StringContent content = new StringContent(newJsonObject.ToString(), Encoding.UTF8, "application/json");
                var reponse = client.PatchAsync($"{baseURL}/expenses?id=eq.{id}", content).Result;

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


        public class APIInvoiceRepository : IRepository<InvoiceModel>
        {
            private List<InvoiceModel> _invoices;
            public APIInvoiceRepository() { }
            public APIInvoiceRepository(List<InvoiceModel> list) { _invoices = list; }

            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var reponse = client.DeleteAsync($"{baseURL}/invoices?id=eq.{id}").Result;

                if (reponse.IsSuccessStatusCode)
                {
                    var item = _invoices.FirstOrDefault(r => r.Id == intId);
                    _invoices.Remove(item);
                    return 1;
                }
                else
                {
                    Debug.WriteLine("Failed deletion");
                    Debug.WriteLine(reponse.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }

            public List<InvoiceModel> GetAll(int pageNumber = 1, int pageSize = 10, string sortBy = null, bool sortDescending = false, string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                if (_invoices != null)
                {
                    return _invoices;
                }
                HttpResponseMessage response = client.GetAsync($"{baseURL}/invoices?limit={pageSize}&offset={(pageNumber - 1) * pageSize}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;

                    List<InvoiceModel> invoices = JsonConvert.DeserializeObject<List<InvoiceModel>>(jsonResponse);

                    _invoices = invoices;
                    return invoices;
                }
                else
                {
                    Debug.WriteLine("Error: " + response.StatusCode);
                    return new List<InvoiceModel>();
                }
            }

            public InvoiceModel GetById(string id)
            {
                if (!int.TryParse(id, out int intId)) return null;
                var data = _invoices ?? GetAll();
                return data.FirstOrDefault(p => p.Id == intId);

                HttpResponseMessage response = client.GetAsync($"{baseURL}/invoices?id=eq.{id}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;
                    List<InvoiceModel> invoices = JsonConvert.DeserializeObject<List<InvoiceModel>>(jsonResponse);
                    if (invoices?.Count > 0)
                    {
                        return invoices[0];
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    Debug.WriteLine("Failed to get invoice by ID");
                    Debug.WriteLine("Error: " + response.StatusCode);
                    return null;
                }
            }

            public int GetCount(string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                client.DefaultRequestHeaders.Add("Prefer", "count=exact");
                var response = client.GetAsync($"{baseURL}/invoices").Result;
                if (response.IsSuccessStatusCode)
                {
                    int total = Int32.Parse(response.Content.Headers.GetValues("Content-Range").FirstOrDefault().Split('/')[^1]);
                    return total;
                }
                else
                {
                    Debug.WriteLine("Failed count total");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }
            public int Insert(string id, InvoiceModel info)
            {
                if (info == null)
                {
                    return 0;
                }

                info.Id = _invoices.Count + 1;
                _invoices.Add(info);

                JObject jsonObject = JObject.FromObject(info, serializer);
                //Remove all object's properties that are not match with the columns
                jsonObject.Remove("Id");
                jsonObject.Remove("Customer");
                jsonObject.Remove("InvoiceDetails");

                JObject newJsonObject = new JObject();
                //keys must match with the columns in the database (lowercase)
                foreach (var pair in jsonObject.Properties())
                {
                    newJsonObject.Add(pair.Name.ToLower(), pair.Value);
                }

                string jsonContent = newJsonObject.ToString();
                StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{baseURL}/invoices")
                {
                    Content = content
                };
                LogRequestDetails(request);
                HttpResponseMessage response = client.Send(request);

                if (response.IsSuccessStatusCode)
                {
                    // Handle the successful response (e.g., read the response content)
                    string responseContent = response.Content.ReadAsStringAsync().Result;
                    Debug.WriteLine("Invoice inserted successfully: " + responseContent);
                    JArray jsonArray = JArray.Parse(responseContent);
                    _invoices[^1].Id = jsonArray[0]["id"].ToObject<int>();
                    return 1;
                }
                else
                {
                    Debug.WriteLine("Failed insertion");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    _invoices.RemoveAt(_invoices.Count - 1);
                    return 0;
                }
            }
            public int Insert(InvoiceModel info)
            {
                return Insert("", info);
            }
            public int UpdateById(string id, InvoiceModel info)
            {
                JObject jsonObject = JObject.FromObject(info);
                //Remove all object's properties that are not match with the columns
                jsonObject.Remove("Id");
                jsonObject.Remove("Customer");

                JObject newJsonObject = new JObject();
                //keys must match with the columns in the database (lowercase)
                foreach (var pair in jsonObject.Properties())
                {
                    newJsonObject.Add(pair.Name.ToLower(), pair.Value);
                }
                StringContent content = new StringContent(newJsonObject.ToString(), Encoding.UTF8, "application/json");
                var reponse = client.PatchAsync($"{baseURL}/invoices?id=eq.{id}", content).Result;

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

        public class APIInvoiceDetailRepository : IRepository<InvoiceDetailModel>
        {
            private List<InvoiceDetailModel> _invoiceDetails;
            public APIInvoiceDetailRepository() { }
            public APIInvoiceDetailRepository(List<InvoiceDetailModel> list) { _invoiceDetails = list; }

            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var reponse = client.DeleteAsync($"{baseURL}/invoicedetails?id=eq.{id}").Result;

                if (reponse.IsSuccessStatusCode)
                {
                    var item = _invoiceDetails.FirstOrDefault(r => r.Id == intId);
                    _invoiceDetails.Remove(item);
                    return 1;
                }
                else
                {
                    Debug.WriteLine("Failed deletion");
                    Debug.WriteLine(reponse.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }

            public List<InvoiceDetailModel> GetAll(int pageNumber = 1, int pageSize = 10, string sortBy = null, bool sortDescending = false, string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                if (_invoiceDetails != null)
                {
                    return _invoiceDetails;
                }
                HttpResponseMessage response = client.GetAsync($"{baseURL}/invoicedetails?limit={pageSize}&offset={(pageNumber - 1) * pageSize}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;

                    List<InvoiceDetailModel> invoiceDetails = JsonConvert.DeserializeObject<List<InvoiceDetailModel>>(jsonResponse);

                    _invoiceDetails = invoiceDetails;

                    return invoiceDetails;
                }
                else
                {
                    Debug.WriteLine("Error: " + response.StatusCode);
                    return new List<InvoiceDetailModel>();
                }
            }

            public InvoiceDetailModel GetById(string id)
            {
                if (!int.TryParse(id, out int intId)) return null;
                var data = _invoiceDetails ?? GetAll();
                return data.FirstOrDefault(p => p.Id == intId);

                HttpResponseMessage response = client.GetAsync($"{baseURL}/invoicedetails?id=eq.{id}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;
                    List<InvoiceDetailModel> invoiceDetails = JsonConvert.DeserializeObject<List<InvoiceDetailModel>>(jsonResponse);
                    if (invoiceDetails?.Count > 0)
                    {
                        return invoiceDetails[0];
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    Debug.WriteLine("Failed to get invoice detail by ID");
                    Debug.WriteLine("Error: " + response.StatusCode);
                    return null;
                }
            }

            public int GetCount(string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                client.DefaultRequestHeaders.Add("Prefer", "count=exact");
                var response = client.GetAsync($"{baseURL}/invoicedetails").Result;
                if (response.IsSuccessStatusCode)
                {
                    int total = Int32.Parse(response.Content.Headers.GetValues("Content-Range").FirstOrDefault().Split('/')[^1]);
                    return total;
                }
                else
                {
                    Debug.WriteLine("Failed count total");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }
            public int Insert(string id, InvoiceDetailModel info)
            {
                if (info == null)
                {
                    return 0;
                }

                info.Id = _invoiceDetails.Count + 1;
                _invoiceDetails.Add(info);
                
                JObject jsonObject = JObject.FromObject(info, serializer);
                //Remove all object's properties that are not match with the columns
                jsonObject.Remove("Id");
                jsonObject.Remove("Invoice");
                jsonObject.Remove("ProductVariant");
                jsonObject.Remove("Toppings");

                JObject newJsonObject = new JObject();
                //keys must match with the columns in the database (lowercase)
                foreach (var pair in jsonObject.Properties())
                {
                    newJsonObject.Add(pair.Name.ToLower(), pair.Value);
                }

                string jsonContent = newJsonObject.ToString();
                StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{baseURL}/invoicedetails")
                {
                    Content = content
                };
                LogRequestDetails(request);
                HttpResponseMessage response = client.Send(request);

                if (response.IsSuccessStatusCode)
                {
                    // Handle the successful response (e.g., read the response content)
                    string responseContent = response.Content.ReadAsStringAsync().Result;
                    Debug.WriteLine("Invoice detail inserted successfully: " + responseContent);
                    JArray jsonArray = JArray.Parse(responseContent);
                    _invoiceDetails[^1].Id = jsonArray[0]["id"].ToObject<int>();
                    return 1;
                }
                else
                {
                    Debug.WriteLine("Failed insertion");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    _invoiceDetails.RemoveAt(_invoiceDetails.Count - 1);
                    return 0;
                }
            }

            public int Insert(InvoiceDetailModel info)
            {
                return Insert("", info);
            }

            public int UpdateById(string id, InvoiceDetailModel info)
            {
                JObject jsonObject = JObject.FromObject(info);
                //Remove all object's properties that are not match with the columns
                jsonObject.Remove("Id");
                jsonObject.Remove("Invoice");
                jsonObject.Remove("ProductVariant");
                jsonObject.Remove("Toppings");

                JObject newJsonObject = new JObject();
                //keys must match with the columns in the database (lowercase)
                foreach (var pair in jsonObject.Properties())
                {
                    newJsonObject.Add(pair.Name.ToLower(), pair.Value);
                }
                StringContent content = new StringContent(newJsonObject.ToString(), Encoding.UTF8, "application/json");
                var reponse = client.PatchAsync($"{baseURL}/invoicedetails?id=eq.{id}", content).Result;

                if (reponse.IsSuccessStatusCode)
                {
                    return 1;
                }
                else
                {
                    Debug.WriteLine("Failed to update invoice detail by ID");
                    Debug.WriteLine(reponse.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }
        }

        public class APIRecipeDetailRepository : IRepository<RecipeDetailModel>
        {
            private List<RecipeDetailModel> _recipeDetails;
            public APIRecipeDetailRepository() { }
            public APIRecipeDetailRepository(List<RecipeDetailModel> list) { _recipeDetails = list; }

            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var reponse = client.DeleteAsync($"{baseURL}/recipedetails?id=eq.{id}").Result;

                if (reponse.IsSuccessStatusCode)
                {
                    var item = _recipeDetails.FirstOrDefault(r => r.Id == intId);
                    _recipeDetails.Remove(item);
                    return 1;
                }
                else
                {
                    Debug.WriteLine("Failed deletion");
                    Debug.WriteLine(reponse.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }

            public List<RecipeDetailModel> GetAll(int pageNumber = 1, int pageSize = 10, string sortBy = null, bool sortDescending = false, string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                if (_recipeDetails != null)
                {
                    return _recipeDetails;
                }
                HttpResponseMessage response = client.GetAsync($"{baseURL}/recipedetails?limit={pageSize}&offset={(pageNumber - 1) * pageSize}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;

                    List<RecipeDetailModel> recipeDetails = JsonConvert.DeserializeObject<List<RecipeDetailModel>>(jsonResponse);

                    _recipeDetails = recipeDetails;

                    return recipeDetails;
                }
                else
                {
                    Debug.WriteLine("Error: " + response.StatusCode);
                    return new List<RecipeDetailModel>();
                }
            }

            public RecipeDetailModel GetById(string id)
            {
                if (!int.TryParse(id, out int intId)) return null;
                var data = _recipeDetails ?? GetAll();
                return data.FirstOrDefault(p => p.Id == intId);

                HttpResponseMessage response = client.GetAsync($"{baseURL}/recipedetails?id=eq.{id}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;
                    List<RecipeDetailModel> recipeDetails = JsonConvert.DeserializeObject<List<RecipeDetailModel>>(jsonResponse);
                    if (recipeDetails?.Count > 0)
                    {
                        return recipeDetails[0];
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    Debug.WriteLine("Failed to get recipe detail by ID");
                    Debug.WriteLine("Error: " + response.StatusCode);
                    return null;
                }
            }

            public int GetCount(string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                client.DefaultRequestHeaders.Add("Prefer", "count=exact");
                var response = client.GetAsync($"{baseURL}/recipedetails").Result;
                if (response.IsSuccessStatusCode)
                {
                    int total = Int32.Parse(response.Content.Headers.GetValues("Content-Range").FirstOrDefault().Split('/')[^1]);
                    return total;
                }
                else
                {
                    Debug.WriteLine("Failed count total");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }
            public int Insert(string id, RecipeDetailModel info)
            {
                if (info == null)
                {
                    return 0;
                }

                info.Id = _recipeDetails.Count + 1;
                _recipeDetails.Add(info);

                JObject jsonObject = JObject.FromObject(info, serializer);
                //Remove all object's properties that are not match with the columns
                jsonObject.Remove("Id");
                jsonObject.Remove("ProductVariant");
                jsonObject.Remove("Ingredient");

                JObject newJsonObject = new JObject();
                //keys must match with the columns in the database (lowercase)
                foreach (var pair in jsonObject.Properties())
                {
                    newJsonObject.Add(pair.Name.ToLower(), pair.Value);
                }

                string jsonContent = newJsonObject.ToString();
                StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{baseURL}/recipedetails")
                {
                    Content = content
                };
                LogRequestDetails(request);
                HttpResponseMessage response = client.Send(request);

                if (response.IsSuccessStatusCode)
                {
                    // Handle the successful response (e.g., read the response content)
                    string responseContent = response.Content.ReadAsStringAsync().Result;
                    Debug.WriteLine("Recipe detail inserted successfully: " + responseContent);
                    JArray jsonArray = JArray.Parse(responseContent);
                    _recipeDetails[^1].Id = jsonArray[0]["id"].ToObject<int>();
                    return 1;
                }
                else
                {
                    Debug.WriteLine("Failed insertion");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    _recipeDetails.RemoveAt(_recipeDetails.Count - 1);
                    return 0;
                }
            }
            public int Insert(RecipeDetailModel info)
            {
                return Insert("", info);
            }
            public int UpdateById(string id, RecipeDetailModel info)
            {
                JObject jsonObject = JObject.FromObject(info);
                //Remove all object's properties that are not match with the columns
                jsonObject.Remove("Id");
                jsonObject.Remove("ProductVariant");
                jsonObject.Remove("Ingredient");

                JObject newJsonObject = new JObject();
                //keys must match with the columns in the database (lowercase)
                foreach (var pair in jsonObject.Properties())
                {
                    newJsonObject.Add(pair.Name.ToLower(), pair.Value);
                }
                StringContent content = new StringContent(newJsonObject.ToString(), Encoding.UTF8, "application/json");
                var reponse = client.PatchAsync($"{baseURL}/recipedetails?id=eq.{id}", content).Result;

                if (reponse.IsSuccessStatusCode)
                {
                    return 1;
                }
                else
                {
                    Debug.WriteLine("Failed to update recipe detail by ID");
                    Debug.WriteLine(reponse.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }
        }


        public class APIPaymentRepository : IRepository<PaymentModel>
        {
            private List<PaymentModel> _payments;
            public APIPaymentRepository() { }
            public APIPaymentRepository(List<PaymentModel> list) { _payments = list; }

            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var reponse = client.DeleteAsync($"{baseURL}/payments?id=eq.{id}").Result;

                if (reponse.IsSuccessStatusCode)
                {
                    var item = _payments.FirstOrDefault(r => r.Id == intId);
                    _payments.Remove(item);
                    return 1;
                }
                else
                {
                    Debug.WriteLine("Failed deletion");
                    Debug.WriteLine(reponse.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }

            public List<PaymentModel> GetAll(int pageNumber = 1, int pageSize = 10, string sortBy = null, bool sortDescending = false, string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                if (_payments != null)
                {
                    return _payments;
                }
                HttpResponseMessage response = client.GetAsync($"{baseURL}/payments?limit={pageSize}&offset={(pageNumber - 1) * pageSize}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;

                    List<PaymentModel> payments = JsonConvert.DeserializeObject<List<PaymentModel>>(jsonResponse);

                    _payments = payments;

                    return payments;
                }
                else
                {
                    Debug.WriteLine("Error: " + response.StatusCode);
                    return new List<PaymentModel>();
                }
            }

            public PaymentModel GetById(string id)
            {
                if (!int.TryParse(id, out int intId)) return null;
                var data = _payments ?? GetAll();
                return data.FirstOrDefault(p => p.Id == intId);

                HttpResponseMessage response = client.GetAsync($"{baseURL}/payments?id=eq.{id}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;
                    List<PaymentModel> payments = JsonConvert.DeserializeObject<List<PaymentModel>>(jsonResponse);
                    if (payments?.Count > 0)
                    {
                        return payments[0];
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    Debug.WriteLine("Failed to get payment by ID");
                    Debug.WriteLine("Error: " + response.StatusCode);
                    return null;
                }
            }

            public int GetCount(string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                client.DefaultRequestHeaders.Add("Prefer", "count=exact");
                var response = client.GetAsync($"{baseURL}/payments").Result;
                if (response.IsSuccessStatusCode)
                {
                    int total = Int32.Parse(response.Content.Headers.GetValues("Content-Range").FirstOrDefault().Split('/')[^1]);
                    return total;
                }
                else
                {
                    Debug.WriteLine("Failed count total");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }
            public int Insert(string id, PaymentModel info)
            {
                if (info == null)
                {
                    return 0;
                }

                info.Id = _payments.Count + 1;
                _payments.Add(info);
                
                JObject jsonObject = JObject.FromObject(info, serializer);
                //Remove all object's properties that are not match with the columns
                jsonObject.Remove("Id");
                jsonObject.Remove("Invoice");

                JObject newJsonObject = new JObject();
                //keys must match with the columns in the database (lowercase)
                foreach (var pair in jsonObject.Properties())
                {
                    newJsonObject.Add(pair.Name.ToLower(), pair.Value);
                }

                string jsonContent = newJsonObject.ToString();
                StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{baseURL}/payments")
                {
                    Content = content
                };
                LogRequestDetails(request);
                HttpResponseMessage response = client.Send(request);

                if (response.IsSuccessStatusCode)
                {
                    // Handle the successful response (e.g., read the response content)
                    string responseContent = response.Content.ReadAsStringAsync().Result;
                    Debug.WriteLine("Payment inserted successfully: " + responseContent);
                    JArray jsonArray = JArray.Parse(responseContent);
                    _payments[^1].Id = jsonArray[0]["id"].ToObject<int>();
                    return 1;
                }
                else
                {
                    Debug.WriteLine("Failed insertion");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    _payments.RemoveAt(_payments.Count - 1);
                    return 0;
                }
            }
            public int Insert(PaymentModel info)
            {
                return Insert("", info);
            }
            public int UpdateById(string id, PaymentModel info)
            {
                JObject jsonObject = JObject.FromObject(info);
                //Remove all object's properties that are not match with the columns
                jsonObject.Remove("Id");
                jsonObject.Remove("Invoice");

                JObject newJsonObject = new JObject();
                //keys must match with the columns in the database (lowercase)
                foreach (var pair in jsonObject.Properties())
                {
                    newJsonObject.Add(pair.Name.ToLower(), pair.Value);
                }
                StringContent content = new StringContent(newJsonObject.ToString(), Encoding.UTF8, "application/json");
                var reponse = client.PatchAsync($"{baseURL}/payments?id=eq.{id}", content).Result;

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


        public class APIOrderToppingRepository : IRepository<OrderToppingModel>
        {
            private List<OrderToppingModel> _orderToppings;
            public APIOrderToppingRepository() { }
            public APIOrderToppingRepository(List<OrderToppingModel> list) { _orderToppings = list; }

            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var reponse = client.DeleteAsync($"{baseURL}/ordertoppings?id=eq.{id}").Result;

                if (reponse.IsSuccessStatusCode)
                {
                    var item = _orderToppings.FirstOrDefault(r => r.Id == intId);
                    _orderToppings.Remove(item);
                    return 1;
                }
                else
                {
                    Debug.WriteLine("Failed deletion");
                    Debug.WriteLine(reponse.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }

            public List<OrderToppingModel> GetAll(int pageNumber = 1, int pageSize = 10, string sortBy = null, bool sortDescending = false, string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                if (_orderToppings != null)
                {
                    return _orderToppings;
                }
                HttpResponseMessage response = client.GetAsync($"{baseURL}/ordertoppings?limit={pageSize}&offset={(pageNumber - 1) * pageSize}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;

                    List<OrderToppingModel> orderToppings = JsonConvert.DeserializeObject<List<OrderToppingModel>>(jsonResponse);

                    _orderToppings = orderToppings;

                    return orderToppings;
                }
                else
                {
                    Debug.WriteLine("Error: " + response.StatusCode);
                    return new List<OrderToppingModel>();
                }
            }

            public OrderToppingModel GetById(string id)
            {
                if (!int.TryParse(id, out int intId)) return null;
                var data = _orderToppings ?? GetAll();
                return data.FirstOrDefault(p => p.Id == intId);

                HttpResponseMessage response = client.GetAsync($"{baseURL}/ordertoppings?id=eq.{id}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;
                    List<OrderToppingModel> orderToppings = JsonConvert.DeserializeObject<List<OrderToppingModel>>(jsonResponse);
                    if (orderToppings?.Count > 0)
                    {
                        return orderToppings[0];
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    Debug.WriteLine("Failed to get order topping by ID");
                    Debug.WriteLine("Error: " + response.StatusCode);
                    return null;
                }
            }

            public int GetCount(string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                client.DefaultRequestHeaders.Add("Prefer", "count=exact");
                var response = client.GetAsync($"{baseURL}/ordertoppings").Result;
                if (response.IsSuccessStatusCode)
                {
                    int total = Int32.Parse(response.Content.Headers.GetValues("Content-Range").FirstOrDefault().Split('/')[^1]);
                    return total;
                }
                else
                {
                    Debug.WriteLine("Failed count total");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }
            public int Insert(string id, OrderToppingModel info)
            {
                if (info == null)
                {
                    return 0;
                }
                
                info.Id = _orderToppings.Count + 1;
                _orderToppings.Add(info);
                
                JObject jsonObject = JObject.FromObject(info, serializer);
                //Remove all object's properties that are not match with the columns
                jsonObject.Remove("Id");
                jsonObject.Remove("InvoiceDetail");
                jsonObject.Remove("ProductVariant");

                JObject newJsonObject = new JObject();
                //keys must match with the columns in the database (lowercase)
                foreach (var pair in jsonObject.Properties())
                {
                    newJsonObject.Add(pair.Name.ToLower(), pair.Value);
                }

                string jsonContent = newJsonObject.ToString();
                StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{baseURL}/ordertoppings")
                {
                    Content = content
                };
                LogRequestDetails(request);
                HttpResponseMessage response = client.Send(request);

                if (response.IsSuccessStatusCode)
                {
                    // Handle the successful response (e.g., read the response content)
                    string responseContent = response.Content.ReadAsStringAsync().Result;
                    Debug.WriteLine("Order topping inserted successfully: " + responseContent);
                    JArray jsonArray = JArray.Parse(responseContent);
                    _orderToppings[^1].Id = jsonArray[0]["id"].ToObject<int>();
                    return 1;
                }
                else
                {
                    Debug.WriteLine("Failed insertion");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    _orderToppings.RemoveAt(_orderToppings.Count - 1);
                    return 0;
                }
            }
            public int Insert(OrderToppingModel info)
            {
                return Insert("", info);
            }
            public int UpdateById(string id, OrderToppingModel info)
            {
                JObject jsonObject = JObject.FromObject(info);
                //Remove all object's properties that are not match with the columns
                jsonObject.Remove("Id");
                jsonObject.Remove("InvoiceDetail");
                jsonObject.Remove("ProductVariant");

                JObject newJsonObject = new JObject();
                //keys must match with the columns in the database (lowercase)
                foreach (var pair in jsonObject.Properties())
                {
                    newJsonObject.Add(pair.Name.ToLower(), pair.Value);
                }
                StringContent content = new StringContent(newJsonObject.ToString(), Encoding.UTF8, "application/json");
                var reponse = client.PatchAsync($"{baseURL}/ordertoppings?id=eq.{id}", content).Result;

                if (reponse.IsSuccessStatusCode)
                {
                    return 1;
                }
                else
                {
                    Debug.WriteLine("Failed to update order topping by ID");
                    Debug.WriteLine(reponse.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }
        }

        public class APITaxRepository : IRepository<TaxModel>
        {
            private List<TaxModel> _taxes;
            public APITaxRepository() { }
            public APITaxRepository(List<TaxModel> list) { _taxes = list; }

            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var reponse = client.DeleteAsync($"{baseURL}/taxes?id=eq.{id}").Result;

                if (reponse.IsSuccessStatusCode)
                {
                    var item = _taxes.FirstOrDefault(r => r.Id == intId);
                    _taxes.Remove(item);
                    return 1;
                }
                else
                {
                    Debug.WriteLine("Failed deletion");
                    Debug.WriteLine(reponse.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }

            public List<TaxModel> GetAll(int pageNumber = 1, int pageSize = 10, string sortBy = null, bool sortDescending = false, string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                if (_taxes != null)
                {
                    return _taxes;
                }
                HttpResponseMessage response = client.GetAsync($"{baseURL}/taxes?limit={pageSize}&offset={(pageNumber - 1) * pageSize}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;

                    List<TaxModel> taxes = JsonConvert.DeserializeObject<List<TaxModel>>(jsonResponse);

                    _taxes = taxes;

                    return taxes;
                }
                else
                {
                    Debug.WriteLine("Error: " + response.StatusCode);
                    return new List<TaxModel>();
                }
            }

            public TaxModel GetById(string id)
            {
                if (!int.TryParse(id, out int intId)) return null;
                var data = _taxes ?? GetAll();
                return data.FirstOrDefault(p => p.Id == intId);

                HttpResponseMessage response = client.GetAsync($"{baseURL}/taxes?id=eq.{id}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;
                    List<TaxModel> taxes = JsonConvert.DeserializeObject<List<TaxModel>>(jsonResponse);
                    if (taxes?.Count > 0)
                    {
                        return taxes[0];
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    Debug.WriteLine("Failed to get tax by ID");
                    Debug.WriteLine("Error: " + response.StatusCode);
                    return null;
                }
            }

            public int GetCount(string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                client.DefaultRequestHeaders.Add("Prefer", "count=exact");
                var response = client.GetAsync($"{baseURL}/taxes").Result;
                if (response.IsSuccessStatusCode)
                {
                    int total = Int32.Parse(response.Content.Headers.GetValues("Content-Range").FirstOrDefault().Split('/')[^1]);
                    return total;
                }
                else
                {
                    Debug.WriteLine("Failed count total");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }
            public int Insert(string id, TaxModel info)
            {
                if (info == null)
                {
                    return 0;
                }

                info.Id = _taxes.Count + 1;
                _taxes.Add(info);
                
                JObject jsonObject = JObject.FromObject(info, serializer);
                //Remove all object's properties that are not match with the columns
                jsonObject.Remove("Id");

                JObject newJsonObject = new JObject();
                //keys must match with the columns in the database (lowercase)
                foreach (var pair in jsonObject.Properties())
                {
                    newJsonObject.Add(pair.Name.ToLower(), pair.Value);
                }

                string jsonContent = newJsonObject.ToString();
                StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{baseURL}/taxes")
                {
                    Content = content
                };
                LogRequestDetails(request);
                HttpResponseMessage response = client.Send(request);

                if (response.IsSuccessStatusCode)
                {
                    // Handle the successful response (e.g., read the response content)
                    string responseContent = response.Content.ReadAsStringAsync().Result;
                    Debug.WriteLine("Tax inserted successfully: " + responseContent);
                    JArray jsonArray = JArray.Parse(responseContent);
                    _taxes[^1].Id = jsonArray[0]["id"].ToObject<int>();
                    return 1;
                }
                else
                {
                    Debug.WriteLine("Failed insertion");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    _taxes.RemoveAt(_taxes.Count - 1);
                    return 0;
                }
            }
            public int Insert(TaxModel info)
            {
                return Insert("", info);
            }
            public int UpdateById(string id, TaxModel info)
            {
                JObject jsonObject = JObject.FromObject(info);
                //Remove all object's properties that are not match with the columns
                jsonObject.Remove("Id");

                JObject newJsonObject = new JObject();
                //keys must match with the columns in the database (lowercase)
                foreach (var pair in jsonObject.Properties())
                {
                    newJsonObject.Add(pair.Name.ToLower(), pair.Value);
                }
                StringContent content = new StringContent(newJsonObject.ToString(), Encoding.UTF8, "application/json");
                var reponse = client.PatchAsync($"{baseURL}/taxes?id=eq.{id}", content).Result;

                if (reponse.IsSuccessStatusCode)
                {
                    return 1;
                }
                else
                {
                    Debug.WriteLine("Failed to update tax by ID");
                    Debug.WriteLine(reponse.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }
        }

        public class APIInvoiceTaxRepository : IRepository<InvoiceTaxModel>
        {
            private List<InvoiceTaxModel> _invoiceTaxes;
            public APIInvoiceTaxRepository() { }
            public APIInvoiceTaxRepository(List<InvoiceTaxModel> list) { _invoiceTaxes = list; }

            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var reponse = client.DeleteAsync($"{baseURL}/invoicetaxes?id=eq.{id}").Result;

                if (reponse.IsSuccessStatusCode)
                {
                    var item = _invoiceTaxes.FirstOrDefault(r => r.Id == intId);
                    _invoiceTaxes.Remove(item);
                    return 1;
                }
                else
                {
                    Debug.WriteLine("Failed deletion");
                    Debug.WriteLine(reponse.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }

            public List<InvoiceTaxModel> GetAll(int pageNumber = 1, int pageSize = 10, string sortBy = null, bool sortDescending = false, string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                if (_invoiceTaxes != null)
                {
                    return _invoiceTaxes;
                }
                HttpResponseMessage response = client.GetAsync($"{baseURL}/invoicetaxes?limit={pageSize}&offset={(pageNumber - 1) * pageSize}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;

                    List<InvoiceTaxModel> invoiceTaxes = JsonConvert.DeserializeObject<List<InvoiceTaxModel>>(jsonResponse);

                    _invoiceTaxes = invoiceTaxes;

                    return invoiceTaxes;
                }
                else
                {
                    Debug.WriteLine("Error: " + response.StatusCode);
                    return new List<InvoiceTaxModel>();
                }
            }

            public InvoiceTaxModel GetById(string id)
            {
                if (!int.TryParse(id, out int intId)) return null;
                var data = _invoiceTaxes ?? GetAll();
                return data.FirstOrDefault(p => p.Id == intId);

                HttpResponseMessage response = client.GetAsync($"{baseURL}/invoicetaxes?id=eq.{id}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;
                    List<InvoiceTaxModel> invoiceTaxes = JsonConvert.DeserializeObject<List<InvoiceTaxModel>>(jsonResponse);
                    if (invoiceTaxes?.Count > 0)
                    {
                        return invoiceTaxes[0];
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    Debug.WriteLine("Failed to get invoice tax by ID");
                    Debug.WriteLine("Error: " + response.StatusCode);
                    return null;
                }
            }

            public int GetCount(string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                client.DefaultRequestHeaders.Add("Prefer", "count=exact");
                var response = client.GetAsync($"{baseURL}/invoicetaxes").Result;
                if (response.IsSuccessStatusCode)
                {
                    int total = Int32.Parse(response.Content.Headers.GetValues("Content-Range").FirstOrDefault().Split('/')[^1]);
                    return total;
                }
                else
                {
                    Debug.WriteLine("Failed count total");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }
            public int Insert(string id, InvoiceTaxModel info)
            {
                if (info == null)
                {
                    return 0;
                }

                info.Id = _invoiceTaxes.Count + 1;
                _invoiceTaxes.Add(info);
                
                JObject jsonObject = JObject.FromObject(info, serializer);
                //Remove all object's properties that are not match with the columns
                jsonObject.Remove("Id");
                jsonObject.Remove("Invoice");
                jsonObject.Remove("Tax");

                JObject newJsonObject = new JObject();
                //keys must match with the columns in the database (lowercase)
                foreach (var pair in jsonObject.Properties())
                {
                    newJsonObject.Add(pair.Name.ToLower(), pair.Value);
                }

                string jsonContent = newJsonObject.ToString();
                StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{baseURL}/invoicetaxes")
                {
                    Content = content
                };
                LogRequestDetails(request);
                HttpResponseMessage response = client.Send(request);

                if (response.IsSuccessStatusCode)
                {
                    // Handle the successful response (e.g., read the response content)
                    string responseContent = response.Content.ReadAsStringAsync().Result;
                    Debug.WriteLine("Invoice tax inserted successfully: " + responseContent);
                    JArray jsonArray = JArray.Parse(responseContent);
                    _invoiceTaxes[^1].Id = jsonArray[0]["id"].ToObject<int>();
                    return 1;
                }
                else
                {
                    Debug.WriteLine("Failed insertion");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    _invoiceTaxes.RemoveAt(_invoiceTaxes.Count - 1);
                    return 0;
                }
            }
            public int Insert(InvoiceTaxModel info)
            {
                return Insert("", info);
            }
            public int UpdateById(string id, InvoiceTaxModel info)
            {
                JObject jsonObject = JObject.FromObject(info);
                //Remove all object's properties that are not match with the columns
                jsonObject.Remove("Id");
                jsonObject.Remove("Invoice");
                jsonObject.Remove("Tax");

                JObject newJsonObject = new JObject();
                //keys must match with the columns in the database (lowercase)
                foreach (var pair in jsonObject.Properties())
                {
                    newJsonObject.Add(pair.Name.ToLower(), pair.Value);
                }
                StringContent content = new StringContent(newJsonObject.ToString(), Encoding.UTF8, "application/json");
                var reponse = client.PatchAsync($"{baseURL}/invoicetaxes?id=eq.{id}", content).Result;

                if (reponse.IsSuccessStatusCode)
                {
                    return 1;
                }
                else
                {
                    Debug.WriteLine("Failed to update invoice tax by ID");
                    Debug.WriteLine(reponse.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }
        }

        public APIDao()
        {
            client.DefaultRequestHeaders.Add("Prefer", "return=representation");

            var categories = new APICategoryRepository().GetAll();
            var customers = new APICustomerRepository().GetAll();
            var expenseCategories = new APIExpenseCategoryRepository().GetAll();
            var expenses = new APIExpenseRepository().GetAll();
            var inbounds = new APIInboundRepository().GetAll();
            var invoices = new APIInvoiceRepository().GetAll();
            var inventories = new APIInventoryRepository().GetAll();
            var ingredients = new APIIngredientRepository().GetAll();
            var invoiceDetails = new APIInvoiceDetailRepository().GetAll();
            var recipeDetails = new APIRecipeDetailRepository().GetAll();
            var payments = new APIPaymentRepository().GetAll();
            var orderToppings = new APIOrderToppingRepository().GetAll();
            var suppliers = new APISupplierRepository().GetAll();
            var products = new APIProductRepository().GetAll();
            var outbounds = new APIOutboundRepository().GetAll();
            var productVariants = new APIProductVariantRepository().GetAll();
            var taxes = new APITaxRepository().GetAll();
            var invoiceTaxes = new APIInvoiceTaxRepository().GetAll();
            var checkInventories = new APICheckInventoryRepository().GetAll();

            foreach (var category in categories)
            {
                category.Products = products.Where(p => p.CategoryId == category.Id).ToList();
                foreach (var product in category.Products)
                {
                    product.Category = category;
                }
            }

            foreach (var customer in customers)
            {
                customer.Invoices = invoices.Where(i => i.CustomerId == customer.Id).ToList();
                foreach (var invoice in customer.Invoices)
                {
                    invoice.Customer = customer;
                }
            }

            foreach (var expenseCategory in expenseCategories)
            {
                expenseCategory.Expenses = expenses.Where(e => e.ExpenseCategoryId == expenseCategory.Id).ToList();
                foreach (var expense in expenseCategory.Expenses)
                {
                    expense.ExpenseCategory = expenseCategory;
                }
            }

            foreach (var inbound in inbounds)
            {
                inbound.Ingredient = ingredients.FirstOrDefault(i => i.Id == inbound.IngredientId);
                inbound.Supplier = suppliers.FirstOrDefault(s => s.Id == inbound.SupplierId);
            }

            foreach (var supplier in suppliers)
            {
                supplier.Inbounds = inbounds.Where(i => i.SupplierId == supplier.Id).ToList();
            }

            foreach (var invoice in invoices)
            {
                invoice.InvoiceDetails = invoiceDetails.Where(id => id.InvoiceId == invoice.Id).ToList();
                foreach (var invoiceDetail in invoice.InvoiceDetails)
                {
                    invoiceDetail.Invoice = invoice;
                }
            }

            foreach (var inventory in inventories)
            {
                inventory.Inbound = inbounds.FirstOrDefault(ib => ib.Id == inventory.InboundId);
            }

            foreach (var product in products)
            {
                product.ProductVariants = productVariants.Where(pv => pv.ProductId == product.Id).ToList();
                foreach (var variant in product.ProductVariants)
                {
                    variant.Product = product;
                }
            }

            foreach (var productVariant in productVariants)
            {
                productVariant.InvoiceDetails = invoiceDetails.Where(id => id.ProductId == productVariant.Id).ToList();
                productVariant.Toppings = orderToppings.Where(ot => ot.ProductId == productVariant.Id).ToList();
                productVariant.RecipeDetails = recipeDetails.Where(rd => rd.ProductVariantId == productVariant.Id).ToList();
            }

            foreach (var invoiceDetail in invoiceDetails)
            {
                invoiceDetail.ProductVariant = productVariants.FirstOrDefault(pv => pv.Id == invoiceDetail.ProductId);
                invoiceDetail.Toppings = orderToppings.Where(ot => ot.InvoiceDetailId == invoiceDetail.Id).ToList();
            }

            foreach (var recipeDetail in recipeDetails)
            {
                recipeDetail.ProductVariant = productVariants.FirstOrDefault(pv => pv.Id == recipeDetail.ProductVariantId);
                recipeDetail.Ingredient = ingredients.FirstOrDefault(i => i.Id == recipeDetail.IngredientId);
            }

            foreach (var payment in payments)
            {
                payment.Invoice = invoices.FirstOrDefault(i => i.Id == payment.InvoiceId);
            }

            foreach (var orderTopping in orderToppings)
            {
                orderTopping.ProductVariant = productVariants.FirstOrDefault(pv => pv.Id == orderTopping.ProductId);
                orderTopping.InvoiceDetail = invoiceDetails.FirstOrDefault(id => id.Id == orderTopping.InvoiceDetailId);
            }

            foreach (var outbound in outbounds)
            {
                outbound.Inventory = inventories.FirstOrDefault(iv => iv.Id == outbound.InventoryId);
            }

            foreach (var tax in taxes)
            {
                tax.InvoiceTaxes = invoiceTaxes.Where(it => it.TaxId == tax.Id).ToList();
                foreach (var invoiceTax in tax.InvoiceTaxes)
                {
                    invoiceTax.Tax = tax;
                }
            }

            foreach (var invoiceTax in invoiceTaxes)
            {
                invoiceTax.Invoice = invoices.FirstOrDefault(i => i.Id == invoiceTax.InvoiceId);
            }

            foreach (var checkInventory in checkInventories)
            {
                checkInventory.Inventory = inventories.FirstOrDefault(i => i.Id == checkInventory.InventoryId);
            }

            Categories = new APICategoryRepository(categories);
            Customers = new APICustomerRepository(customers);
            ExpenseCategories = new APIExpenseCategoryRepository(expenseCategories);
            Expenses = new APIExpenseRepository(expenses);
            Inbounds = new APIInboundRepository(inbounds);
            Invoices = new APIInvoiceRepository(invoices);
            Inventories = new APIInventoryRepository(inventories);
            Ingredients = new APIIngredientRepository(ingredients);
            InvoiceDetails = new APIInvoiceDetailRepository(invoiceDetails);
            RecipeDetails = new APIRecipeDetailRepository(recipeDetails);
            Payments = new APIPaymentRepository(payments);
            OrderToppings = new APIOrderToppingRepository(orderToppings);
            Suppliers = new APISupplierRepository(suppliers);
            Products = new APIProductRepository(products);
            Outbounds = new APIOutboundRepository(outbounds);
            ProductVariants = new APIProductVariantRepository(productVariants);
            Taxes = new APITaxRepository(taxes);
            InvoiceTaxes = new APIInvoiceTaxRepository(invoiceTaxes);
            CheckInventories = new APICheckInventoryRepository(checkInventories);
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
