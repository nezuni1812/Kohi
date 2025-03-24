using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.Models
{
    public class InvoiceDetailModel : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public int InvoiceId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1;
        [Required]
        public decimal UnitPrice { get; set; }
        public int? SugarLevel { get; set; }
        public int? IceLevel { get; set; }

        // Navigation Properties
        public InvoiceModel Invoice { get; set; } // Liên kết ngược lại Invoice
        public ProductModel Product { get; set; } // Liên kết tới Product
        public List<OrderToppingModel> Toppings { get; set; } = new List<OrderToppingModel>(); // Một InvoiceDetail có nhiều Toppings
        public event PropertyChangedEventHandler? PropertyChanged;

    }
}
