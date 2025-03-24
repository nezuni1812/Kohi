using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.Models
{
    public class ExpenseModel : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public int ExpenseCategoryId { get; set; }
        public string? Note { get; set; }
        public string? ReceiptUrl { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public float Amount { get; set; } // Not null
        public DateTime ExpenseDate { get; set; } // Not null, dùng DateTime thay date

        // Navigation Property
        public ExpenseCategoryModel ExpenseCategory { get; set; }
        public event PropertyChangedEventHandler? PropertyChanged;

    }
}
