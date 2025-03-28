﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.Models
{ 
    public class InvoiceTaxModel : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public int InvoiceId { get; set; }
        public int TaxId { get; set; }

        // Navigation Properties
        public InvoiceModel Invoice { get; set; }
        public TaxModel Tax { get; set; }
        public event PropertyChangedEventHandler? PropertyChanged;

    }
}
