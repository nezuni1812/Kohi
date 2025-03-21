using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Services.Maps;

namespace Kohi.Models
{
    public class RecipeModel : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public ProductModel Product { get; set; }
        public List<RecipeDetailModel> Ingredients { get; set; } = new List<RecipeDetailModel>();
        public event PropertyChangedEventHandler? PropertyChanged;

    }
}
