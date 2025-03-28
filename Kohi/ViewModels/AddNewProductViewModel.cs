using Kohi.Models;
using Kohi.Services;
using Kohi.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.ViewModels
{
    public class AddNewProductViewModel
    {
        private IDao _dao;
        public string ProductName { get; set; }
        public FullObservableCollection<ProductVariantViewModel> Variants { get; set; }
        public FullObservableCollection<CategoryModel> Categories { get; set; }
        public FullObservableCollection<IngredientModel> Ingredients { get; set; }

        public AddNewProductViewModel()
        {
            _dao = Service.GetKeyedSingleton<IDao>();
            Variants = new FullObservableCollection<ProductVariantViewModel>();
            Categories = new FullObservableCollection<CategoryModel>(_dao.Categories.GetAll()); // Giả sử có phương thức này
            Ingredients = new FullObservableCollection<IngredientModel>(_dao.Ingredients.GetAll()); // Giả sử có phương thức này
        }

        public void SaveProduct()
        {
            var product = new ProductModel
            {
                Name = ProductName,
                ProductVariants = Variants.Select(v => new ProductVariantModel
                {
                    Size = v.Size,
                    Price = v.Price,
                    Cost = v.Cost,
                    RecipeDetails = v.RecipeDetails.Select(r => new RecipeDetailModel
                    {
                        IngredientId = r.Ingredient?.Id ?? 0,
                        Quantity = r.Quantity,
                        Unit = r.Unit
                    }).ToList()
                }).ToList()
            };
            _dao.Products.Insert(product);
        }

        public async Task AddProduct(ProductModel product)
        {
            try
            {
                int result = _dao.Products.Insert(product);
                product.Category = _dao.Categories.GetAll().FirstOrDefault(c => c.Id == product.CategoryId);
            }
            catch (Exception ex)
            {

            }
        }

        public async Task AddProductVariant(ProductModel product)
        {
            try
            {
                 
            }
            catch (Exception ex)
            {

            }
        }

        public async Task AddRecipeDetails(ProductModel product)
        {
            try
            {
               
            }
            catch (Exception ex)
            {

            }
        }



        public class ProductVariantViewModel : INotifyPropertyChanged
        {
            public int Id { get; set; }
            public int ProductId { get; set; }
            public string? Size { get; set; }
            public float Price { get; set; }
            public float Cost { get; set; }
            public FullObservableCollection<RecipeDetailViewModel> RecipeDetails { get; set; }

            public ProductVariantViewModel()
            {
                RecipeDetails = new FullObservableCollection<RecipeDetailViewModel>();
            }

            public event PropertyChangedEventHandler? PropertyChanged;
        }

        public class RecipeDetailViewModel : INotifyPropertyChanged
        {
            public int Id { get; set; }
            public int ProductVariantId { get; set; }
            public int IngredientId { get; set; }
            public float Quantity { get; set; }
            public string? Unit { get; set; }
            public IngredientModel? Ingredient { get; set; }

            public event PropertyChangedEventHandler? PropertyChanged; // Fody sẽ tự xử lý
        }
    }
}
