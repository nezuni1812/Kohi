﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.Models
{
    internal class TaxModel
    {
        public int Id { get; set; }
        [Required]
        public string TaxName { get; set; } // Not null
        [Required]
        public decimal TaxRate { get; set; } // Not null

        // Navigation Property: Một Tax liên kết với nhiều InvoiceTax
        public List<InvoiceTaxModel> InvoiceTaxes { get; set; } = new List<InvoiceTaxModel>();
    }
}
