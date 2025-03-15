using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Services.Maps;

namespace Kohi.Models
{
    internal class RecipeModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public ProductModel Product { get; set; }
        public List<RecipeDetailModel> Ingredients { get; set; } = new List<RecipeDetailModel>();

    }
}
