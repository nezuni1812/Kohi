using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.Models
{
    public class RecipeDetailModel : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public int ProductVariantId { get; set; }
        public int IngredientId { get; set; }
        public float Quantity { get; set; }
        public string Unit { get; set; }
        public ProductVariantModel ProductVariant { get; set; }
        public IngredientModel Ingredient { get; set; }
        public event PropertyChangedEventHandler? PropertyChanged;

    }
}
