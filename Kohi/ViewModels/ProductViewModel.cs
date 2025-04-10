using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Kohi.BusinessLogic;
using Kohi.Models;
using Kohi.Services;
using Kohi.Utils;
using Microsoft.UI.Xaml.Controls.Primitives;
using Windows.Storage;

//namespace Kohi.ViewModels
//{
//    public class ProductViewModel
//    {
//        private ProductService _service;
//        public ObservableCollection<ProductModel> Products { get; set; }
//        public string NewProductName { get; set; } = "";
//        public string NewCategoryId { get; set; } = "";

//        public ProductViewModel()
//        {
//            _service = new ProductService();
//            Products = new ObservableCollection<ProductModel>();

//            LoadProducts();
//        }

//        private async void LoadProducts()
//        {
//            var products = await _service.GetProductAsync();
//            Products.Clear();

//            foreach (var product in products)
//            {
//                Products.Add(product);
//            }
//        }

//        public async void AddProduct()
//        {
//            await _service.AddProductAsync(new ProductModel { Name = NewProductName });
//            LoadProducts();
//            NewProductName = "";
//            NewCategoryId = null;
//        }
//    }
//}


//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Kohi.Models;
//using Kohi.Services;
//using Kohi.Utils;

namespace Kohi.ViewModels
{
    public class ProductViewModel
    {
        private IDao _dao;

        //private ProductService _service;
        public string NewProductName { get; set; } = "";
        public string NewCategoryId { get; set; } = "";
        public FullObservableCollection<ProductModel> Products { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize); // Tổng số trang
        public ProductViewModel()
        {
            _dao = Service.GetKeyedSingleton<IDao>();
            Products = new FullObservableCollection<ProductModel>();
            LoadData();
        }

        public async Task LoadData(int page = 1)
        {
            CurrentPage = page;
            TotalItems = _dao.Products.GetCount(); // Lấy tổng số khách hàng từ DAO
            var result = await Task.Run(() => _dao.Products.GetAll(
                pageNumber: CurrentPage,
                pageSize: PageSize
            )); // Lấy danh sách khách hàng phân trang
            Products.Clear();
            foreach (var item in result)
            {
                // Kiểm tra xem danh mục đã tồn tại trong danh sách chưa
                if (!Products.Any(c => c.Id == item.Id))
                {
                    // Đường dẫn đầy đủ tới hình ảnh trong LocalFolder
                    if (!string.IsNullOrEmpty(item.ImageUrl))
                    {
                        // Tạo đường dẫn đầy đủ
                        try
                        {
                            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                            item.ImageUrl = System.IO.Path.Combine(localFolder.Path, item.ImageUrl);
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine("Safely skip: " + e.StackTrace);
                        }
                    }
                    item.Category = _dao.Categories.GetById(item.CategoryId.ToString());
                }
                Products.Add(item);
            }
        }
        public async void AddProduct()
        {
            Debug.WriteLine(NewProductName);
            ProductModel newProduct = new ProductModel { Name = NewProductName };

            Add(newProduct);

            LoadData();
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

        public async Task Add(ProductModel product)
        {
            try
            {
                int result = _dao.Products.Insert(product);
                product.Category = _dao.Categories.GetAll().FirstOrDefault(c => c.Id == product.CategoryId);
                await LoadData(CurrentPage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
        public async Task Delete(string id)
        {
            try
            {
                _dao.Products.GetAll();

                ProductModel product = _dao.Products.GetById(id);
                if (product == null)
                {
                    Debug.WriteLine("Không tìm thấy sản phẩm cần xóa");
                    await LoadData(CurrentPage);
                    return;
                }
                var variantList = product.ProductVariants;
                var variantIdList = variantList.Select(v => v.Id).ToList();
                Debug.WriteLine("Variant count: " + variantIdList.Count);

                if (product.IsTopping == false)
                {
                    var invoiceDetailList = _dao.InvoiceDetails.GetAll();
                    foreach (var invoiceDetail in invoiceDetailList)
                    {
                        if (variantIdList.Contains(invoiceDetail.ProductId))
                        {
                            Debug.WriteLine("Không thể xóa sản phẩm này vì đã có hóa đơn sử dụng");
                            await LoadData(CurrentPage);
                            return;
                        }
                    }
                }
                else
                {
                    var orderToppingList = _dao.OrderToppings.GetAll();
                    foreach (var orderTopping in orderToppingList)
                    {
                        if (variantIdList.Contains(orderTopping.ProductVariant.Id))
                        {
                            Debug.WriteLine("Không thể xóa sản phẩm này vì đã sử dụng thành topping");
                            await LoadData(CurrentPage);
                            return;
                        }
                    }
                }

                var recipeDetailList = _dao.RecipeDetails.GetAll();
                var recipeDetailIdList = new List<int>();
                foreach (var recipeDetail in recipeDetailList)
                {
                    if (variantIdList.Contains(recipeDetail.ProductVariantId))
                    {
                        recipeDetailIdList.Add(recipeDetail.Id);
                    }
                }

                recipeDetailIdList.ForEach(id => _dao.RecipeDetails.DeleteById(id + ""));
                Debug.WriteLine("Đã xóa recipeDetails");
                variantIdList.ForEach(id => _dao.ProductVariants.DeleteById(id + ""));
                Debug.WriteLine("Đã xóa product variants");

                int result = _dao.Products.DeleteById(id);

                await LoadData(CurrentPage);
            }
            catch (Exception ex)
            {

            }
        }

        public async Task Update(string id, ProductModel product)
        {
            try
            {
                int result = _dao.Products.UpdateById(id, product);
            }
            catch (Exception ex)
            {

            }
        }
        public async Task<ProductModel> GetById(string id)
        {
            try
            {
                var product = _dao.Products.GetById(id); // Đồng bộ
                return product;
            }
            catch (Exception ex)
            {
                return null; // Trả về null khi có lỗi
                // Xử lý lỗi (tùy chọn)
            }
        }
    }
}