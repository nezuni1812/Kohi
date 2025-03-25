using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.Models
{
    public class IngredientModel : INotifyPropertyChanged
    {
        public int Id { get; set; }
        [Required]
        public string? Name { get; set; }
        public string? Unit { get; set; }
        public float CostPerUnit { get; set; }
        public string? Description { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
