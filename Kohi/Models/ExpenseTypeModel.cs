using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.Models
{
    public class ExpenseTypeModel : INotifyPropertyChanged
    {
        public int Id { get; set; }
        [Required]
        public string TypeName { get; set; } // Not null
        public string? Description { get; set; }

        // Navigation Property
        public List<ExpenseCategoryModel> ExpenseCategories { get; set; } = new List<ExpenseCategoryModel>();
        public event PropertyChangedEventHandler? PropertyChanged;

    }
}
