using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.Models
{
    public class PaymentModel : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public int InvoiceId { get; set; }
        public DateTime PaymentDate { get; set; } // Not null, dùng DateTime thay cho date
        [Required]
        public float Amount { get; set; } // Not null
        [Required]
        public string? PaymentMethod { get; set; } // Not null

        // Navigation Property: Liên kết tới Invoice
        public InvoiceModel? Invoice { get; set; }
        public event PropertyChangedEventHandler? PropertyChanged;

    }
}
