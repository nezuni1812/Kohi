using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.Models
{
    internal class RecipeDetailModel
    {
        public int RecipeId { get; set; }
        public int IngredientId { get; set; }
        public int Quantity { get; set; }
        public string? Unit { get; set; }
        public RecipeModel Recipe { get; set; }
        public IngredientModel Ingredient { get; set; }
    }
}
