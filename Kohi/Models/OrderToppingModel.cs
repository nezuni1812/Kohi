using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.Models
{
    public class OrderToppingModel : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public int InvoiceDetailId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; } // Số lượng topping, mặc định là 1

        // Navigation Properties
        public InvoiceDetailModel InvoiceDetail { get; set; } // Liên kết ngược lại InvoiceDetail
        public ProductVariantModel ProductVariant { get; set; } // Liên kết tới Product (topping cũng là một Product)
        public event PropertyChangedEventHandler? PropertyChanged;

    }
}
