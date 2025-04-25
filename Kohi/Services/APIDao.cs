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
            private static int _nextId;
            public APICategoryRepository() { }
            public APICategoryRepository(List<CategoryModel> list) {  }

            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
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

            public List<CategoryModel> GetAll(int pageNumber = 1, int pageSize = 20, string sortBy = null, bool sortDescending = false, string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                string orderClause = "";
                if (!string.IsNullOrEmpty(sortBy))
                {
                    orderClause = $"&order={sortBy}.{(sortDescending ? "desc" : "asc")}";
                }

                string filterClause = "";
                if (!string.IsNullOrEmpty(filterField) && !string.IsNullOrEmpty(filterValue))
                {
                    filterClause = $"&{filterField}=eq.{filterValue}";
                }

                string url = $"{baseURL}/categories?limit={pageSize}&offset={(pageNumber - 1) * pageSize}{orderClause}{filterClause}";
                
                HttpResponseMessage response = client.GetAsync($"{url}").Result;
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
                if (!int.TryParse(id, out int intId)) return null;
                var data =  GetAll();
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
                    CategoryModel category = JsonConvert.DeserializeObject<List<CategoryModel>>(responseContent)[0];
                    info.Id = category.Id;
                    Debug.WriteLine("Category inserted successfully: " + responseContent);
                    return info.Id;
                }
                else
                {
                    Debug.WriteLine("Failed insertion");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }

            public int Insert(CategoryModel info)
            {
                return Insert("", info);
            }

            public int UpdateById(string id, CategoryModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data = GetAll();
                var item = data.FirstOrDefault(c => c.Id == intId);
                if (item == null) return 0;

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
                StringContent content = new StringContent(newJsonObject.ToString(), Encoding.UTF8, "application/json");
                var reponse = client.PatchAsync($"{baseURL}/categories?id=eq.{id}", content).Result;

                if (reponse.IsSuccessStatusCode)
                {
                    item.Name = info.Name;
                    item.ImageUrl = info.ImageUrl;
                    item.Products = info.Products; // Cập nhật navigation property nếu cần
                    Debug.WriteLine("Category updated successfully: " + reponse.Content.ReadAsStringAsync().Result);
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

            public APIProductRepository() { }
            public APIProductRepository(List<ProductModel> products)
            {
            }

            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var reponse = client.DeleteAsync($"{baseURL}/products?id=eq.{id}").Result;

                if (reponse.IsSuccessStatusCode)
                {
                    Debug.WriteLine("Success deletion product");
                    return 1;
                }
                else
                {
                    Debug.WriteLine("Failed deletion");
                    Debug.WriteLine(reponse.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }
            public List<ProductModel> GetAll(int pageNumber = 1, int pageSize = 20, string sortBy = null, bool sortDescending = false, string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                string orderClause = "";
                if (!string.IsNullOrEmpty(sortBy))
                {
                    orderClause = $"&order={sortBy}.{(sortDescending ? "desc" : "asc")}";
                }

                string filterClause = "";
                if (!string.IsNullOrEmpty(filterField) && !string.IsNullOrEmpty(filterValue))
                {
                    filterClause = $"&{filterField}=eq.{filterValue}";
                }

                string url = $"{baseURL}/products?limit={pageSize}&offset={(pageNumber - 1) * pageSize}{orderClause}{filterClause}";
                
                HttpResponseMessage response = client.GetAsync($"{url}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;

                    List<ProductModel> products = JsonConvert.DeserializeObject<List<ProductModel>>(jsonResponse);

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
                var data =  GetAll();
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
                    info.Id = product.Id;
                    return info.Id;
                }
                else
                {
                    Debug.WriteLine("Failed insertion");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                }
                return 0;
            }

            public int Insert(ProductModel info)
            {
                return Insert("", info);
            }

            public int UpdateById(string id, ProductModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data = GetAll();
                var item = data.FirstOrDefault(p => p.Id == intId);
                if (item == null) return 0;

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
                StringContent content = new StringContent(newJsonObject.ToString(), Encoding.UTF8, "application/json");
                var reponse = client.PatchAsync($"{baseURL}/products?id=eq.{id}", content).Result;

                if (reponse.IsSuccessStatusCode)
                {
                    item.Name = info.Name;
                    item.IsActive = info.IsActive;
                    item.CategoryId = info.CategoryId;
                    item.ImageUrl = info.ImageUrl;
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

            public APIInventoryRepository() { }
            public APIInventoryRepository(List<InventoryModel> list) {  }

            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var reponse = client.DeleteAsync($"{baseURL}/inventories?id=eq.{id}").Result;

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

            public List<InventoryModel> GetAll(int pageNumber = 1, int pageSize = 20, string sortBy = null, bool sortDescending = false, string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                string orderClause = "";
                if (!string.IsNullOrEmpty(sortBy))
                {
                    orderClause = $"&order={sortBy}.{(sortDescending ? "desc" : "asc")}";
                }

                string filterClause = "";
                if (!string.IsNullOrEmpty(filterField) && !string.IsNullOrEmpty(filterValue))
                {
                    filterClause = $"&{filterField}=eq.{filterValue}";
                }

                string url = $"{baseURL}/inventories?limit={pageSize}&offset={(pageNumber - 1) * pageSize}{orderClause}{filterClause}";
                
                HttpResponseMessage response = client.GetAsync($"{url}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;

                    List<InventoryModel> inventories = JsonConvert.DeserializeObject<List<InventoryModel>>(jsonResponse);

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
                var data = GetAll();
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
                    InventoryModel newInventory = JsonConvert.DeserializeObject<List<InventoryModel>>(responseContent)[0];
                    info.Id = newInventory.Id;
                    Debug.WriteLine("Inventory inserted successfully: " + responseContent);
                    return info.Id;
                }
                else
                {
                    Debug.WriteLine("Failed insertion");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }

            public int Insert(InventoryModel info)
            {
                return Insert("", info);
            }

            public int UpdateById(string id, InventoryModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data =  GetAll();
                var item = data.FirstOrDefault(i => i.Id == intId);
                if (item == null) return 0;
                Debug.WriteLine("Before serializer");
                JObject jsonObject = JObject.FromObject(info, serializer);
                Debug.WriteLine("After serializer");
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
                    item.InboundId = info.InboundId;
                    item.Quantity = info.Quantity;
                    item.InboundDate = info.InboundDate;
                    item.ExpiryDate = info.ExpiryDate;
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
            public APIIngredientRepository() { }
            public APIIngredientRepository(List<IngredientModel> list) { }
            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var reponse = client.DeleteAsync($"{baseURL}/ingredients?id=eq.{id}").Result;

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

            public List<IngredientModel> GetAll(int pageNumber = 1, int pageSize = 20, string sortBy = null, bool sortDescending = false, string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                string orderClause = "";
                if (!string.IsNullOrEmpty(sortBy))
                {
                    orderClause = $"&order={sortBy}.{(sortDescending ? "desc" : "asc")}";
                }

                string filterClause = "";
                if (!string.IsNullOrEmpty(filterField) && !string.IsNullOrEmpty(filterValue))
                {
                    filterClause = $"&{filterField}=eq.{filterValue}";
                }

                string url = $"{baseURL}/ingredients?limit={pageSize}&offset={(pageNumber - 1) * pageSize}{orderClause}{filterClause}";
                
                HttpResponseMessage response = client.GetAsync($"{url}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;

                    List<IngredientModel> ingredients = JsonConvert.DeserializeObject<List<IngredientModel>>(jsonResponse);

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
                var data = GetAll();
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
                    info.Id = ingredient.Id;
                    Debug.WriteLine("Ingredient inserted successfully: " + ingredient.Id);
                    return ingredient.Id;
                }
                else
                {
                    Debug.WriteLine("Failed insertion");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                }
                return 0;
            }

            public int Insert(IngredientModel info)
            {
                return Insert("", info);
            }

            public int UpdateById(string id, IngredientModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data = GetAll();
                var item = data.FirstOrDefault(i => i.Id == intId);
                if (item == null) return 0;

                JObject jsonObject = JObject.FromObject(info, serializer);
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
                    item.Name = info.Name;
                    item.Unit = info.Unit;
                    //item.CostPerUnit = info.CostPerUnit;
                    item.Description = info.Description;
                    Debug.WriteLine("Ingredient updated successfully: " + reponse.Content.ReadAsStringAsync().Result);
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
            public APISupplierRepository() { }
            public APISupplierRepository(List<SupplierModel> list) { }

            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var reponse = client.DeleteAsync($"{baseURL}/suppliers?id=eq.{id}").Result;

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

            public List<SupplierModel> GetAll(int pageNumber = 1, int pageSize = 20, string sortBy = null, bool sortDescending = false, string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                string orderClause = "";
                if (!string.IsNullOrEmpty(sortBy))
                {
                    orderClause = $"&order={sortBy}.{(sortDescending ? "desc" : "asc")}";
                }

                string filterClause = "";
                if (!string.IsNullOrEmpty(filterField) && !string.IsNullOrEmpty(filterValue))
                {
                    filterClause = $"&{filterField}=eq.{filterValue}";
                }

                string url = $"{baseURL}/suppliers?limit={pageSize}&offset={(pageNumber - 1) * pageSize}{orderClause}{filterClause}";
                
                HttpResponseMessage response = client.GetAsync($"{url}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;

                    List<SupplierModel> suppliers = JsonConvert.DeserializeObject<List<SupplierModel>>(jsonResponse);

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
                var data = GetAll();
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
                    SupplierModel supplier = JsonConvert.DeserializeObject<List<SupplierModel>>(responseContent)[0];
                    info.Id = supplier.Id;
                    Debug.WriteLine("Supplier inserted successfully: " + responseContent);
                    return info.Id;
                }
                else
                {
                    Debug.WriteLine("Failed insertion");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }

            public int Insert(SupplierModel info)
            {
                return Insert("", info);
            }

            public int UpdateById(string id, SupplierModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data =  GetAll();
                var item = data.FirstOrDefault(i => i.Id == intId);
                if (item == null) return 0;

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
                StringContent content = new StringContent(newJsonObject.ToString(), Encoding.UTF8, "application/json");
                var reponse = client.PatchAsync($"{baseURL}/suppliers?id=eq.{id}", content).Result;

                if (reponse.IsSuccessStatusCode)
                {
                    item.Name = info.Name;
                    item.Email = info.Email;
                    item.Phone = info.Phone;
                    item.Address = info.Address;
                    Debug.WriteLine("Supplier updated successfully: " + reponse.Content.ReadAsStringAsync().Result);
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
            public APIInboundRepository() { }
            public APIInboundRepository(List<InboundModel> list) { }

            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var reponse = client.DeleteAsync($"{baseURL}/inbounds?id=eq.{id}").Result;

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

            public List<InboundModel> GetAll(int pageNumber = 1, int pageSize = 20, string sortBy = null, bool sortDescending = false, string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                string orderClause = "";
                if (!string.IsNullOrEmpty(sortBy))
                {
                    orderClause = $"&order={sortBy}.{(sortDescending ? "desc" : "asc")}";
                }

                string filterClause = "";
                if (!string.IsNullOrEmpty(filterField) && !string.IsNullOrEmpty(filterValue))
                {
                    filterClause = $"&{filterField}=eq.{filterValue}";
                }

                string url = $"{baseURL}/inbounds?limit={pageSize}&offset={(pageNumber - 1) * pageSize}{orderClause}{filterClause}";
                
                HttpResponseMessage response = client.GetAsync($"{url}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;

                    List<InboundModel> inbounds = JsonConvert.DeserializeObject<List<InboundModel>>(jsonResponse);

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
                var data =  GetAll();
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

                JObject jsonObject = JObject.FromObject(info, serializer);
                //Remove all object's properties that are not match with the columns
                jsonObject.Remove("Id");
                jsonObject.Remove("Ingredient");
                jsonObject.Remove("Supplier");
                jsonObject.Remove("CostPerUnit");
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
                    InboundModel inbound = JsonConvert.DeserializeObject<List<InboundModel>>(responseContent)[0];
                    info.Id = inbound.Id;
                    Debug.WriteLine("Inbound inserted successfully: " + responseContent);
                    return info.Id;
                }
                else
                {
                    Debug.WriteLine("Failed insertion");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }

            public int Insert(InboundModel info)
            {
                return Insert("", info);
            }

            public int UpdateById(string id, InboundModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data = GetAll();
                var item = data.FirstOrDefault(i => i.Id == intId);
                if (item == null) return 0;

                JObject jsonObject = JObject.FromObject(info, serializer);
                //Remove all object's properties that are not match with the columns
                jsonObject.Remove("Id");
                jsonObject.Remove("Inventories");
                jsonObject.Remove("Supplier");
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
                    item.IngredientId = info.IngredientId;
                    item.Quantity = info.Quantity;
                    item.InboundDate = info.InboundDate;
                    item.ExpiryDate = info.ExpiryDate;
                    item.SupplierId = info.SupplierId;
                    item.Notes = info.Notes;
                    Debug.WriteLine("Inbound updated successfully: " + reponse.Content.ReadAsStringAsync().Result);
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
            public APIOutboundRepository() { }
            public APIOutboundRepository(List<OutboundModel> list) {  }

            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var reponse = client.DeleteAsync($"{baseURL}/outbounds?id=eq.{id}").Result;

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

            public List<OutboundModel> GetAll(int pageNumber = 1, int pageSize = 20, string sortBy = null, bool sortDescending = false, string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                string orderClause = "";
                if (!string.IsNullOrEmpty(sortBy))
                {
                    orderClause = $"&order={sortBy}.{(sortDescending ? "desc" : "asc")}";
                }

                string filterClause = "";
                if (!string.IsNullOrEmpty(filterField) && !string.IsNullOrEmpty(filterValue))
                {
                    filterClause = $"&{filterField}=eq.{filterValue}";
                }

                string url = $"{baseURL}/outbounds?limit={pageSize}&offset={(pageNumber - 1) * pageSize}{orderClause}{filterClause}";
                
                HttpResponseMessage response = client.GetAsync($"{url}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;

                    List<OutboundModel> outbounds = JsonConvert.DeserializeObject<List<OutboundModel>>(jsonResponse);

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
                var data =  GetAll();
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
                    OutboundModel outbound = JsonConvert.DeserializeObject<List<OutboundModel>>(responseContent)[0];
                    info.Id = outbound.Id;
                    Debug.WriteLine("Outbound inserted successfully: " + responseContent);
                    return info.Id;
                }
                else
                {
                    Debug.WriteLine("Failed insertion");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }

            public int Insert(OutboundModel info)
            {
                return Insert("", info);
            }

            public int UpdateById(string id, OutboundModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data = GetAll();
                var item = data.FirstOrDefault(o => o.Id == intId);
                if (item == null) return 0;

                JObject jsonObject = JObject.FromObject(info, serializer);
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
                    item.InventoryId = info.InventoryId;
                    item.Quantity = info.Quantity;
                    item.OutboundDate = info.OutboundDate;
                    item.Purpose = info.Purpose;
                    item.Notes = info.Notes;
                    Debug.WriteLine("Outbound updated successfully: " + reponse.Content.ReadAsStringAsync().Result);
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
            public APICustomerRepository() { }
            public APICustomerRepository(List<CustomerModel> list) {  }

            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var reponse = client.DeleteAsync($"{baseURL}/customers?id=eq.{id}").Result;

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

            public List<CustomerModel> GetAll(int pageNumber = 1, int pageSize = 20, string sortBy = null, bool sortDescending = false, string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                string orderClause = "";
                if (!string.IsNullOrEmpty(sortBy))
                {
                    orderClause = $"&order={sortBy}.{(sortDescending ? "desc" : "asc")}";
                }

                string filterClause = "";
                if (!string.IsNullOrEmpty(filterField) && !string.IsNullOrEmpty(filterValue))
                {
                    filterClause = $"&{filterField}=eq.{filterValue}";
                }

                string url = $"{baseURL}/customers?limit={pageSize}&offset={(pageNumber - 1) * pageSize}{orderClause}{filterClause}";
                
                HttpResponseMessage response = client.GetAsync($"{url}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;

                    List<CustomerModel> customers = JsonConvert.DeserializeObject<List<CustomerModel>>(jsonResponse);

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
                var data =  GetAll();
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
                    CustomerModel newCustomer = JsonConvert.DeserializeObject<List<CustomerModel>>(responseContent)[0];
                    Debug.WriteLine("Customer inserted successfully: " + responseContent);
                    info.Id = newCustomer.Id;
                    return 1;
                }
                else

                {
                    Debug.WriteLine("Failed insertion");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }

            public int Insert(CustomerModel info)
            {
                return Insert("", info);
            }

            public int UpdateById(string id, CustomerModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data = GetAll();
                var item = data.FirstOrDefault(c => c.Id == intId);
                if (item == null) return 0;

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
                StringContent content = new StringContent(newJsonObject.ToString(), Encoding.UTF8, "application/json");
                var reponse = client.PatchAsync($"{baseURL}/customers?id=eq.{id}", content).Result;

                if (reponse.IsSuccessStatusCode)
                {
                    item.Name = info.Name;
                    item.Email = info.Email;
                    item.Phone = info.Phone;
                    item.Address = info.Address;
                    item.Invoices = info.Invoices;
                    Debug.WriteLine("Customer updated successfully: " + reponse.Content.ReadAsStringAsync().Result);
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
            public APIExpenseCategoryRepository() { }
            public APIExpenseCategoryRepository(List<ExpenseCategoryModel> list) {  }

            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var reponse = client.DeleteAsync($"{baseURL}/expensecategories?id=eq.{id}").Result;

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

            public List<ExpenseCategoryModel> GetAll(int pageNumber = 1, int pageSize = 20, string sortBy = null, bool sortDescending = false, string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                string orderClause = "";
                if (!string.IsNullOrEmpty(sortBy))
                {
                    orderClause = $"&order={sortBy}.{(sortDescending ? "desc" : "asc")}";
                }

                string filterClause = "";
                if (!string.IsNullOrEmpty(filterField) && !string.IsNullOrEmpty(filterValue))
                {
                    filterClause = $"&{filterField}=eq.{filterValue}";
                }

                string url = $"{baseURL}/expensecategories?limit={pageSize}&offset={(pageNumber - 1) * pageSize}{orderClause}{filterClause}";
                
                HttpResponseMessage response = client.GetAsync($"{url}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;

                    List<ExpenseCategoryModel> expenseCategories = JsonConvert.DeserializeObject<List<ExpenseCategoryModel>>(jsonResponse);

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
                var data =  GetAll();
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
                    ExpenseCategoryModel expenseCategory = JsonConvert.DeserializeObject<List<ExpenseCategoryModel>>(responseContent)[0];
                    info.Id = expenseCategory.Id;
                    Debug.WriteLine("Expense category inserted successfully: " + responseContent);
                    return info.Id;
                }
                else
                {
                    Debug.WriteLine("Failed insertion");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }

            public int Insert(ExpenseCategoryModel info)
            {
                return Insert("", info);
            }

            public int UpdateById(string id, ExpenseCategoryModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data =  GetAll();
                var item = data.FirstOrDefault(c => c.Id == intId);
                if (item == null) return 0;

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
                StringContent content = new StringContent(newJsonObject.ToString(), Encoding.UTF8, "application/json");
                var reponse = client.PatchAsync($"{baseURL}/expensecategories?id=eq.{id}", content).Result;

                if (reponse.IsSuccessStatusCode)
                {
                    item.CategoryName = info.CategoryName;
                    item.Description = info.Description;
                    item.Expenses = info.Expenses;
                    Debug.WriteLine("Expense category updated successfully: " + reponse.Content.ReadAsStringAsync().Result);
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
            public APICheckInventoryRepository() { }
            public APICheckInventoryRepository(List<CheckInventoryModel> list) {  }

            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var reponse = client.DeleteAsync($"{baseURL}/checkinventories?id=eq.{id}").Result;

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

            public List<CheckInventoryModel> GetAll(int pageNumber = 1, int pageSize = 20, string sortBy = null, bool sortDescending = false, string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                string orderClause = "";
                if (!string.IsNullOrEmpty(sortBy))
                {
                    orderClause = $"&order={sortBy}.{(sortDescending ? "desc" : "asc")}";
                }

                string filterClause = "";
                if (!string.IsNullOrEmpty(filterField) && !string.IsNullOrEmpty(filterValue))
                {
                    filterClause = $"&{filterField}=eq.{filterValue}";
                }

                string url = $"{baseURL}/checkinventories?limit={pageSize}&offset={(pageNumber - 1) * pageSize}{orderClause}{filterClause}";
                
                HttpResponseMessage response = client.GetAsync($"{url}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;

                    List<CheckInventoryModel> checkInventories = JsonConvert.DeserializeObject<List<CheckInventoryModel>>(jsonResponse);

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
                var data =  GetAll();
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
                    CheckInventoryModel checkInventory = JsonConvert.DeserializeObject<List<CheckInventoryModel>>(responseContent)[0];
                    info.Id = checkInventory.Id;
                    Debug.WriteLine("Check inventory inserted successfully: " + responseContent);
                    return info.Id;
                }
                else
                {
                    Debug.WriteLine("Failed insertion");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }

            public int Insert(CheckInventoryModel info)
            {
                return Insert("", info);
            }

            public int UpdateById(string id, CheckInventoryModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data =  GetAll();
                var item = data.FirstOrDefault(i => i.Id == intId);
                if (item == null) return 0;

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
                StringContent content = new StringContent(newJsonObject.ToString(), Encoding.UTF8, "application/json");
                var reponse = client.PatchAsync($"{baseURL}/checkinventories?id=eq.{id}", content).Result;

                if (reponse.IsSuccessStatusCode)
                {
                    item.InventoryId = info.InventoryId;
                    item.ActualQuantity = info.ActualQuantity;
                    item.CheckDate = info.CheckDate;
                    item.Notes = info.Notes;
                    item.Inventory = info.Inventory; // Update navigation property if needed
                    Debug.WriteLine("Check inventory updated successfully: " + reponse.Content.ReadAsStringAsync().Result);
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
            public APIProductVariantRepository() { }
            public APIProductVariantRepository(List<ProductVariantModel> list) {  }

            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var reponse = client.DeleteAsync($"{baseURL}/productvariants?id=eq.{id}").Result;

                if (reponse.IsSuccessStatusCode)
                {
                    Debug.WriteLine("Success deletion productvariant");

                    return 1;
                }
                else
                {
                    Debug.WriteLine("Failed deletion");
                    Debug.WriteLine(reponse.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }

            public List<ProductVariantModel> GetAll(int pageNumber = 1, int pageSize = 20, string sortBy = null, bool sortDescending = false, string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                string orderClause = "";
                if (!string.IsNullOrEmpty(sortBy))
                {
                    orderClause = $"&order={sortBy}.{(sortDescending ? "desc" : "asc")}";
                }

                string filterClause = "";
                if (!string.IsNullOrEmpty(filterField) && !string.IsNullOrEmpty(filterValue))
                {
                    filterClause = $"&{filterField}=eq.{filterValue}";
                }

                string url = $"{baseURL}/productvariants?limit={pageSize}&offset={(pageNumber - 1) * pageSize}{orderClause}{filterClause}";
                
                HttpResponseMessage response = client.GetAsync($"{url}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;

                    List<ProductVariantModel> productVariants = JsonConvert.DeserializeObject<List<ProductVariantModel>>(jsonResponse);

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
                var data =  GetAll();
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
                    ProductVariantModel productVariant = JsonConvert.DeserializeObject<List<ProductVariantModel>>(responseContent)[0];
                    info.Id = productVariant.Id;
                    Debug.WriteLine("Product variant inserted successfully: " + responseContent);
                    return info.Id;
                }
                else
                {
                    Debug.WriteLine("Failed insertion");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }

            public int Insert(ProductVariantModel info)
            {
                return Insert("", info);
            }

            public int UpdateById(string id, ProductVariantModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null)
                {
                    Debug.WriteLine($"Thất bại: Id không hợp lệ hoặc info null: Id = {id}");
                    return 0;
                }
                var data = GetAll();
                var item = data.FirstOrDefault(p => p.Id == intId);
                if (item == null)
                {
                    Debug.WriteLine($"Thất bại: Không tìm thấy ProductVariant với Id = {id} trong danh sách _productVariants");
                    return 0;
                }

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
                StringContent content = new StringContent(newJsonObject.ToString(), Encoding.UTF8,
                    "application/json");
                var reponse = client.PatchAsync($"{baseURL}/productvariants?id=eq.{id}", content).Result;

                if (reponse.IsSuccessStatusCode)
                {
                    item.ProductId = info.ProductId;
                    item.Size = info.Size;
                    item.Price = info.Price;
                    item.Cost = info.Cost;
                    item.Product = info.Product;
                    item.InvoiceDetails = info.InvoiceDetails;
                    item.Toppings = info.Toppings;
                    item.RecipeDetails = info.RecipeDetails;
                    Debug.WriteLine("Product variant updated successfully: " + reponse.Content.ReadAsStringAsync().Result);
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
            public APIExpenseRepository() { }
            public APIExpenseRepository(List<ExpenseModel> list) {  }

            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var reponse = client.DeleteAsync($"{baseURL}/expenses?id=eq.{id}").Result;

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

            public List<ExpenseModel> GetAll(int pageNumber = 1, int pageSize = 20, string sortBy = null, bool sortDescending = false, string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                string orderClause = "";
                if (!string.IsNullOrEmpty(sortBy))
                {
                    Debug.WriteLine("We order");
                    orderClause = $"&order={sortBy}.{(sortDescending ? "desc" : "asc")}";
                }

                string filterClause = "";
                if (!string.IsNullOrEmpty(filterField) && !string.IsNullOrEmpty(filterValue))
                {
                    filterClause = $"&{filterField}=eq.{filterValue}";
                }

                string url = $"{baseURL}/expenses?limit={pageSize}&offset={(pageNumber - 1) * pageSize}{orderClause}{filterClause}";
                Debug.WriteLine("URL: " + url);
                HttpResponseMessage response = client.GetAsync($"{url}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;

                    List<ExpenseModel> expenses = JsonConvert.DeserializeObject<List<ExpenseModel>>(jsonResponse);

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
                var data = GetAll();
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
                    ExpenseModel expense = JsonConvert.DeserializeObject<List<ExpenseModel>>(responseContent)[0];
                    info.Id = expense.Id;
                    Debug.WriteLine("Expense inserted successfully: " + responseContent);
                    return info.Id;
                }
                else
                {
                    Debug.WriteLine("Failed insertion");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }
            public int Insert(ExpenseModel info)
            {
                return Insert("", info);
            }
            public int UpdateById(string id, ExpenseModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data = GetAll();
                var item = data.FirstOrDefault(p => p.Id == intId);
                if (item == null) return 0;

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
                StringContent content = new StringContent(newJsonObject.ToString(), Encoding.UTF8, "application/json");
                var reponse = client.PatchAsync($"{baseURL}/expenses?id=eq.{id}", content).Result;

                if (reponse.IsSuccessStatusCode)
                {
                    item.ExpenseCategoryId = info.ExpenseCategoryId;
                    item.Amount = info.Amount;
                    item.ExpenseDate = info.ExpenseDate;
                    Debug.WriteLine("Expense updated successfully: " + reponse.Content.ReadAsStringAsync().Result);
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
            public APIInvoiceRepository() { }
            public APIInvoiceRepository(List<InvoiceModel> list) {  }

            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var reponse = client.DeleteAsync($"{baseURL}/invoices?id=eq.{id}").Result;

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

            public List<InvoiceModel> GetAll(int pageNumber = 1, int pageSize = 20, string sortBy = null, bool sortDescending = false, string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                string orderClause = "";
                if (!string.IsNullOrEmpty(sortBy))
                {
                    orderClause = $"&order={sortBy}.{(sortDescending ? "desc" : "asc")}";
                }
                
                string filterClause = "";
                if (!string.IsNullOrEmpty(filterField) && !string.IsNullOrEmpty(filterValue))
                {
                    filterClause = $"&{filterField}=eq.{filterValue}";
                }
                
                string url = $"{baseURL}/invoices?limit={pageSize}&offset={(pageNumber - 1) * pageSize}{orderClause}{filterClause}";
                
                HttpResponseMessage response = client.GetAsync($"{url}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;

                    List<InvoiceModel> invoices = JsonConvert.DeserializeObject<List<InvoiceModel>>(jsonResponse);

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
                var data =  GetAll();
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
                    InvoiceModel invoice = JsonConvert.DeserializeObject<List<InvoiceModel>>(responseContent)[0];
                    info.Id = invoice.Id;
                    Debug.WriteLine("Invoice inserted successfully: " + responseContent);
                    return info.Id;
                }
                else
                {
                    Debug.WriteLine("Failed insertion");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }
            public int Insert(InvoiceModel info)
            {
                return Insert("", info);
            }
            public int UpdateById(string id, InvoiceModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data =  GetAll();
                var item = data.FirstOrDefault(i => i.Id == intId);
                if (item == null) return 0;

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
                StringContent content = new StringContent(newJsonObject.ToString(), Encoding.UTF8, "application/json");
                var reponse = client.PatchAsync($"{baseURL}/invoices?id=eq.{id}", content).Result;

                if (reponse.IsSuccessStatusCode)
                {
                    item.CustomerId = info.CustomerId;
                    item.InvoiceDate = info.InvoiceDate;
                    item.TotalAmount = info.TotalAmount;
                    item.InvoiceDetails = info.InvoiceDetails;
                    Debug.WriteLine("Invoice updated successfully: " + reponse.Content.ReadAsStringAsync().Result);
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
            public APIInvoiceDetailRepository() { }
            public APIInvoiceDetailRepository(List<InvoiceDetailModel> list) {  }

            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var reponse = client.DeleteAsync($"{baseURL}/invoicedetails?id=eq.{id}").Result;

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

            public List<InvoiceDetailModel> GetAll(int pageNumber = 1, int pageSize = 20, string sortBy = null, bool sortDescending = false, string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                string orderClause = "";
                if (!string.IsNullOrEmpty(sortBy))
                {
                    orderClause = $"&order={sortBy}.{(sortDescending ? "desc" : "asc")}";
                }

                string filterClause = "";
                if (!string.IsNullOrEmpty(filterField) && !string.IsNullOrEmpty(filterValue))
                {
                    filterClause = $"&{filterField}=eq.{filterValue}";
                }

                string url = $"{baseURL}/invoicedetails?limit={pageSize}&offset={(pageNumber - 1) * pageSize}{orderClause}{filterClause}";
                
                HttpResponseMessage response = client.GetAsync($"{url}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;

                    List<InvoiceDetailModel> invoiceDetails = JsonConvert.DeserializeObject<List<InvoiceDetailModel>>(jsonResponse);


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
                var data =  GetAll();
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
                    InvoiceDetailModel invoiceDetail = JsonConvert.DeserializeObject<List<InvoiceDetailModel>>(responseContent)[0];
                    info.Id = invoiceDetail.Id;
                    Debug.WriteLine("Invoice detail inserted successfully: " + responseContent);
                    return info.Id;
                }
                else
                {
                    Debug.WriteLine("Failed insertion");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }

            public int Insert(InvoiceDetailModel info)
            {
                return Insert("", info);
            }

            public int UpdateById(string id, InvoiceDetailModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data =  GetAll();
                var item = data.FirstOrDefault(i => i.Id == intId);
                if (item == null) return 0;

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
                StringContent content = new StringContent(newJsonObject.ToString(), Encoding.UTF8, "application/json");
                var reponse = client.PatchAsync($"{baseURL}/invoicedetails?id=eq.{id}", content).Result;

                if (reponse.IsSuccessStatusCode)
                {
                    item.InvoiceId = info.InvoiceId;
                    item.ProductId = info.ProductId;
                    item.SugarLevel = info.SugarLevel;
                    item.IceLevel = info.IceLevel;
                    item.Toppings = info.Toppings;
                    Debug.WriteLine("Invoice detail updated successfully: " + reponse.Content.ReadAsStringAsync().Result);
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
            public APIRecipeDetailRepository() { }
            public APIRecipeDetailRepository(List<RecipeDetailModel> list) {  }

            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var reponse = client.DeleteAsync($"{baseURL}/recipedetails?id=eq.{id}").Result;

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

            public List<RecipeDetailModel> GetAll(int pageNumber = 1, int pageSize = 20, string sortBy = null, bool sortDescending = false, string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                string orderClause = "";
                if (!string.IsNullOrEmpty(sortBy))
                {
                    orderClause = $"&order={sortBy}.{(sortDescending ? "desc" : "asc")}";
                }

                string filterClause = "";
                if (!string.IsNullOrEmpty(filterField) && !string.IsNullOrEmpty(filterValue))
                {
                    filterClause = $"&{filterField}=eq.{filterValue}";
                }

                string url = $"{baseURL}/recipedetails?limit={pageSize}&offset={(pageNumber - 1) * pageSize}{orderClause}{filterClause}";
                
                HttpResponseMessage response = client.GetAsync($"{url}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;

                    List<RecipeDetailModel> recipeDetails = JsonConvert.DeserializeObject<List<RecipeDetailModel>>(jsonResponse);


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
                var data =  GetAll();
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
                    RecipeDetailModel recipeDetail = JsonConvert.DeserializeObject<List<RecipeDetailModel>>(responseContent)[0];
                    info.Id = recipeDetail.Id;
                    Debug.WriteLine("Recipe detail inserted successfully: " + responseContent);
                    return info.Id;
                }
                else
                {
                    Debug.WriteLine("Failed insertion");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }
            public int Insert(RecipeDetailModel info)
            {
                return Insert("", info);
            }
            public int UpdateById(string id, RecipeDetailModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data =  GetAll();
                var item = data.FirstOrDefault(r => r.Id == intId);
                if (item == null) return 0;

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
                StringContent content = new StringContent(newJsonObject.ToString(), Encoding.UTF8, "application/json");
                var reponse = client.PatchAsync($"{baseURL}/recipedetails?id=eq.{id}", content).Result;

                if (reponse.IsSuccessStatusCode)
                {
                    item.ProductVariantId = info.ProductVariantId;
                    item.IngredientId = info.IngredientId;
                    item.Quantity = info.Quantity;
                    item.Unit = info.Unit;
                    Debug.WriteLine("Recipe detail updated successfully: " + reponse.Content.ReadAsStringAsync().Result);
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
            public APIPaymentRepository() { }
            public APIPaymentRepository(List<PaymentModel> list) { }

            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var reponse = client.DeleteAsync($"{baseURL}/payments?id=eq.{id}").Result;

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

            public List<PaymentModel> GetAll(int pageNumber = 1, int pageSize = 20, string sortBy = null, bool sortDescending = false, string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                string orderClause = "";
                if (!string.IsNullOrEmpty(sortBy))
                {
                    orderClause = $"&order={sortBy}.{(sortDescending ? "desc" : "asc")}";
                }

                string filterClause = "";
                if (!string.IsNullOrEmpty(filterField) && !string.IsNullOrEmpty(filterValue))
                {
                    filterClause = $"&{filterField}=eq.{filterValue}";
                }

                string url = $"{baseURL}/payments?limit={pageSize}&offset={(pageNumber - 1) * pageSize}{orderClause}{filterClause}";
                
                HttpResponseMessage response = client.GetAsync($"{url}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;

                    List<PaymentModel> payments = JsonConvert.DeserializeObject<List<PaymentModel>>(jsonResponse);

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
                var data =  GetAll();
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
                    PaymentModel payment = JsonConvert.DeserializeObject<List<PaymentModel>>(responseContent)[0];
                    info.Id = payment.Id;
                    Debug.WriteLine("Payment inserted successfully: " + responseContent);
                    return info.Id;
                }
                else
                {
                    Debug.WriteLine("Failed insertion");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }
            public int Insert(PaymentModel info)
            {
                return Insert("", info);
            }
            public int UpdateById(string id, PaymentModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data =  GetAll();
                var item = data.FirstOrDefault(p => p.Id == intId);
                if (item == null) return 0;

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
                StringContent content = new StringContent(newJsonObject.ToString(), Encoding.UTF8, "application/json");
                var reponse = client.PatchAsync($"{baseURL}/payments?id=eq.{id}", content).Result;

                if (reponse.IsSuccessStatusCode)
                {
                    item.InvoiceId = info.InvoiceId;
                    item.PaymentDate = info.PaymentDate;
                    item.Amount = info.Amount;
                    item.PaymentMethod = info.PaymentMethod;
                    Debug.WriteLine("Payment updated successfully: " + reponse.Content.ReadAsStringAsync().Result);
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
            public APIOrderToppingRepository() { }
            public APIOrderToppingRepository(List<OrderToppingModel> list) {  }

            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var reponse = client.DeleteAsync($"{baseURL}/ordertoppings?id=eq.{id}").Result;

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

            public List<OrderToppingModel> GetAll(int pageNumber = 1, int pageSize = 20, string sortBy = null, bool sortDescending = false, string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                string orderClause = "";
                if (!string.IsNullOrEmpty(sortBy))
                {
                    orderClause = $"&order={sortBy}.{(sortDescending ? "desc" : "asc")}";
                }

                string filterClause = "";
                if (!string.IsNullOrEmpty(filterField) && !string.IsNullOrEmpty(filterValue))
                {
                    filterClause = $"&{filterField}=eq.{filterValue}";
                }

                string url = $"{baseURL}/ordertoppings?limit={pageSize}&offset={(pageNumber - 1) * pageSize}{orderClause}{filterClause}";
                
                HttpResponseMessage response = client.GetAsync($"{url}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;

                    List<OrderToppingModel> orderToppings = JsonConvert.DeserializeObject<List<OrderToppingModel>>(jsonResponse);

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
                var data =   GetAll();
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
                    OrderToppingModel orderTopping = JsonConvert.DeserializeObject<List<OrderToppingModel>>(responseContent)[0];
                    info.Id = orderTopping.Id;
                    Debug.WriteLine("Order topping inserted successfully: " + responseContent);
                    return info.Id;
                }
                else
                {
                    Debug.WriteLine("Failed insertion");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }
            public int Insert(OrderToppingModel info)
            {
                return Insert("", info);
            }
            public int UpdateById(string id, OrderToppingModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data =  GetAll();
                var item = data.FirstOrDefault(o => o.Id == intId);
                if (item == null) return 0;

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
                StringContent content = new StringContent(newJsonObject.ToString(), Encoding.UTF8, "application/json");
                var reponse = client.PatchAsync($"{baseURL}/ordertoppings?id=eq.{id}", content).Result;

                if (reponse.IsSuccessStatusCode)
                {
                    item.InvoiceDetailId = info.InvoiceDetailId;
                    item.ProductId = info.ProductId;
                    Debug.WriteLine("Order topping updated successfully: " + reponse.Content.ReadAsStringAsync().Result);
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
            public APITaxRepository() { }
            public APITaxRepository(List<TaxModel> list) {  }

            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var reponse = client.DeleteAsync($"{baseURL}/taxes?id=eq.{id}").Result;

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

            public List<TaxModel> GetAll(int pageNumber = 1, int pageSize = 20, string sortBy = null, bool sortDescending = false, string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                string orderClause = "";
                if (!string.IsNullOrEmpty(sortBy))
                {
                    orderClause = $"&order={sortBy}.{(sortDescending ? "desc" : "asc")}";
                }

                string filterClause = "";
                if (!string.IsNullOrEmpty(filterField) && !string.IsNullOrEmpty(filterValue))
                {
                    filterClause = $"&{filterField}=eq.{filterValue}";
                }

                string url = $"{baseURL}/taxes?limit={pageSize}&offset={(pageNumber - 1) * pageSize}{orderClause}{filterClause}";
                
                HttpResponseMessage response = client.GetAsync($"{url}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;

                    List<TaxModel> taxes = JsonConvert.DeserializeObject<List<TaxModel>>(jsonResponse);


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
                var data =  GetAll();
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
                    TaxModel tax = JsonConvert.DeserializeObject<List<TaxModel>>(responseContent)[0];
                    info.Id = tax.Id;
                    Debug.WriteLine("Tax inserted successfully: " + responseContent);
                    return info.Id;
                }
                else
                {
                    Debug.WriteLine("Failed insertion");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }
            public int Insert(TaxModel info)
            {
                return Insert("", info);
            }
            public int UpdateById(string id, TaxModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data =  GetAll();
                var item = data.FirstOrDefault(t => t.Id == intId);
                if (item == null) return 0;

                JObject jsonObject = JObject.FromObject(info, serializer);
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
                    item.TaxName = info.TaxName;
                    item.TaxRate = info.TaxRate;
                    item.InvoiceTaxes = info.InvoiceTaxes;
                    Debug.WriteLine("Tax updated successfully: " + reponse.Content.ReadAsStringAsync().Result);
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
            public APIInvoiceTaxRepository() { }
            public APIInvoiceTaxRepository(List<InvoiceTaxModel> list) {  }

            public int DeleteById(string id)
            {
                if (!int.TryParse(id, out int intId)) return 0;
                var reponse = client.DeleteAsync($"{baseURL}/invoicetaxes?id=eq.{id}").Result;

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

            public List<InvoiceTaxModel> GetAll(int pageNumber = 1, int pageSize = 20, string sortBy = null, bool sortDescending = false, string filterField = null, string filterValue = null, string searchKeyword = null)
            {
                string orderClause = "";
                if (!string.IsNullOrEmpty(sortBy))
                {
                    orderClause = $"&order={sortBy}.{(sortDescending ? "desc" : "asc")}";
                }

                string filterClause = "";
                if (!string.IsNullOrEmpty(filterField) && !string.IsNullOrEmpty(filterValue))
                {
                    filterClause = $"&{filterField}=eq.{filterValue}";
                }

                string url = $"{baseURL}/invoicetaxes?limit={pageSize}&offset={(pageNumber - 1) * pageSize}{orderClause}{filterClause}";
                
                HttpResponseMessage response = client.GetAsync($"{url}").Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;

                    List<InvoiceTaxModel> invoiceTaxes = JsonConvert.DeserializeObject<List<InvoiceTaxModel>>(jsonResponse);


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
                var data =   GetAll();
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
                    InvoiceTaxModel invoiceTax = JsonConvert.DeserializeObject<List<InvoiceTaxModel>>(responseContent)[0];
                    info.Id = invoiceTax.Id;
                    Debug.WriteLine("Invoice tax inserted successfully: " + responseContent);
                    return info.Id;
                }
                else
                {
                    Debug.WriteLine("Failed insertion");
                    Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                    return 0;
                }
            }
            public int Insert(InvoiceTaxModel info)
            {
                return Insert("", info);
            }
            public int UpdateById(string id, InvoiceTaxModel info)
            {
                if (!int.TryParse(id, out int intId) || info == null) return 0;
                var data =  GetAll();
                var item = data.FirstOrDefault(i => i.Id == intId);
                if (item == null) return 0;

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
                StringContent content = new StringContent(newJsonObject.ToString(), Encoding.UTF8, "application/json");
                var reponse = client.PatchAsync($"{baseURL}/invoicetaxes?id=eq.{id}", content).Result;

                if (reponse.IsSuccessStatusCode)
                {
                    item.InvoiceId = info.InvoiceId;
                    item.TaxId = info.TaxId;
                    Debug.WriteLine("Invoice tax updated successfully: " + reponse.Content.ReadAsStringAsync().Result);
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
