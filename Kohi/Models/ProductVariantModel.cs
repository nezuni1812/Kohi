using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Services.Maps;

namespace Kohi.Models
{
    public class ProductVariantModel : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? Size { get; set; }        // varchar(20), nullable
        public float? Price { get; set; }        // Nullable float
        public float? Cost { get; set; }         // Nullable float
        public ProductModel? Product { get; set; }

        public List<InvoiceDetailModel> InvoiceDetails { get; set; } = new List<InvoiceDetailModel>();
        public List<OrderToppingModel> Toppings { get; set; } = new List<OrderToppingModel>();
        public List<RecipeDetailModel> RecipeDetails { get; set; } = new List<RecipeDetailModel>(); // Ref: recipe_details.recipe_id > product_variants.id
        public event PropertyChangedEventHandler? PropertyChanged;

    }
}
