using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.Models
{
    public class CategoryModel : INotifyPropertyChanged
    {
        [Key]
        public int Id { get; set; }  // Primary Key

        [Required]
        public string Name { get; set; } = "";

        public string? ImagePath { get; set; } = "";

        public List<ProductModel> Products { get; set; } = new List<ProductModel>();

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
