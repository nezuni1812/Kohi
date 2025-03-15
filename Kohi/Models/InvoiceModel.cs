using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.Models
{
    public class InvoiceModel
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public decimal TotalAmount { get; set; } = 0.00m;
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation Property: Một Invoice có nhiều InvoiceDetails
        public CustomerModel Customer { get; set; }
        public List<InvoiceDetailModel> InvoiceDetails { get; set; } = new List<InvoiceDetailModel>();
    }
}
