using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.Models
{
    internal class PaymentModel
    {
        public int Id { get; set; }
        public int InvoiceId { get; set; }
        public DateTime PaymentDate { get; set; } // Not null, dùng DateTime thay cho date
        [Required]
        public decimal Amount { get; set; } // Not null
        [Required]
        public string PaymentMethod { get; set; } // Not null

        // Navigation Property: Liên kết tới Invoice
        public InvoiceModel Invoice { get; set; }
    }
}
