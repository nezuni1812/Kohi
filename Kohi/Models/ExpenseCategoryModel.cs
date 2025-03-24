using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.Models
{
    public class ExpenseCategoryModel : INotifyPropertyChanged
    {
        public int Id { get; set; }
        [Required]
        public string CategoryName { get; set; } // Not null
        public int ExpenseTypeId { get; set; }
        public string? Description { get; set; }

        // Navigation Properties
        public List<ExpenseModel> Expenses { get; set; } = new List<ExpenseModel>();
        public event PropertyChangedEventHandler? PropertyChanged;

    }
}
