using Kohi.Models;
using Kohi.Services;
using Kohi.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.ViewModels
{
    public class ProductVariantViewModel
    {
        private IDao _dao;
        public FullObservableCollection<ProductVariantModel> Variants { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize); // Tổng số trang

        public ProductVariantViewModel()
        {
            _dao = Service.GetKeyedSingleton<IDao>();
            Variants = new FullObservableCollection<ProductVariantModel>();

            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            await LoadData();
        }

        public async Task LoadData(int page = 1)
        {
            try
            {
                CurrentPage = page;
                TotalItems = _dao.ProductVariants.GetCount();

                // Lấy danh sách ProductVariantModel
                var result = await Task.Run(() => _dao.ProductVariants.GetAll(
                    pageNumber: CurrentPage,
                    pageSize: PageSize
                ));

                // Lấy tất cả ProductModel
                var allProducts = await Task.Run(() => _dao.Products.GetAll(
                    pageNumber: 1,
                    pageSize: 1000
                ));

                var allInvoiceDetails = await Task.Run(() => _dao.InvoiceDetails.GetAll(
                    pageNumber: 1,
                    pageSize: 1000
                ));
                var allToppings = await Task.Run(() => _dao.OrderToppings.GetAll(
                    pageNumber: 1,
                    pageSize: 1000
                ));
                var allRecipeDetails = await Task.Run(() => _dao.RecipeDetails.GetAll(
                    pageNumber: 1,
                    pageSize: 1000
                ));

                Variants.Clear();
                foreach (var item in result)
                {
                    item.Product = allProducts.FirstOrDefault(p => p.Id == item.ProductId);
                    Debug.WriteLine($"Variant {item.Id} mapped to Product {item.Product?.Id}");

                    var invoiceDetailsForVariant = allInvoiceDetails.Where(d => d.ProductId == item.Id).ToList();
                    item.InvoiceDetails.Clear();
                    foreach (var detail in invoiceDetailsForVariant)
                    {
                        item.InvoiceDetails.Add(detail);
                    }
                    Debug.WriteLine($"Variant {item.Id} has {item.InvoiceDetails.Count} invoice details");

                    // Nối Toppings
                    var toppingsForVariant = allToppings.Where(t => t.ProductId == item.Id).ToList();
                    item.Toppings.Clear();
                    foreach (var topping in toppingsForVariant)
                    {
                        item.Toppings.Add(topping);
                    }
                    Debug.WriteLine($"Variant {item.Id} has {item.Toppings.Count} toppings");

                    // Nối RecipeDetails
                    var recipeDetailsForVariant = allRecipeDetails.Where(r => r.ProductVariantId == item.Id).ToList();
                    item.RecipeDetails.Clear();
                    foreach (var recipeDetail in recipeDetailsForVariant)
                    {
                        item.RecipeDetails.Add(recipeDetail);
                    }
                    Debug.WriteLine($"Variant {item.Id} has {item.RecipeDetails.Count} recipe details");

                    Variants.Add(item);
                }
            } 
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in LoadData: {ex.Message}");
            }
        }

        // Phương thức để chuyển đến trang tiếp theo
        public async Task NextPage()
        {
            if (CurrentPage < TotalPages)
            {
                await LoadData(CurrentPage + 1);
            }
        }

        // Phương thức để quay lại trang trước
        public async Task PreviousPage()
        {
            if (CurrentPage > 1)
            {
                await LoadData(CurrentPage - 1);
            }
        }

        // Phương thức để chuyển đến trang cụ thể
        public async Task GoToPage(int page)
        {
            if (page >= 1 && page <= TotalPages)
            {
                await LoadData(page);
            }
        }

        public async Task Add(ProductVariantModel productVariant)
        {
            try
            {
                int result = _dao.ProductVariants.Insert(productVariant);
                await LoadData(CurrentPage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding product variant: {ex.Message}");
                throw;
            }
        }

        public async Task Delete(string id)
        {
            try
            {
                int result = _dao.ProductVariants.DeleteById(id);
                await LoadData(CurrentPage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting product variant: {ex.Message}");
            }
        }

        public async Task Update(string id, ProductVariantModel productVariantModel)
        {
            try
            {
                int result = _dao.ProductVariants.UpdateById(id, productVariantModel);
                await LoadData(CurrentPage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating product variant: {ex.Message}");
            }
        }
        public async Task<List<ProductVariantModel>> GetByProductId(int productId)
        {
            try
            {
                var variants = await Task.Run(() => _dao.ProductVariants.GetAll(1, int.MaxValue)
                    .Where(v => v.ProductId == productId)
                    .ToList());
                Debug.WriteLine($"Loaded {variants.Count} ProductVariants for ProductId = {productId}");
                return variants;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading ProductVariants for ProductId = {productId}: {ex.Message}");
                throw;
            }
        }
        public async Task<List<ProductVariantModel>> GetAll()
        {
            try
            {
                var ProductVariants = _dao.ProductVariants.GetAll(1, 1000); // Đồng bộ
                return ProductVariants;
            }
            catch (Exception ex)
            {
                return null; // Trả về null khi có lỗi
                // Xử lý lỗi (tùy chọn)
            }
        }
    }
}