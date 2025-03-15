using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.Models
{
    internal class InvoiceTaxModel
    {
        public int Id { get; set; }
        public int InvoiceId { get; set; }
        public int TaxId { get; set; }

        // Navigation Properties
        public InvoiceModel Invoice { get; set; }
        public TaxModel Tax { get; set; }
    }
}
