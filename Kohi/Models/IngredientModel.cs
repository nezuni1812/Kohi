using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.Models
{
    public class IngredientModel
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Unit { get; set; }
        public decimal CostPerUnit { get; set; }
        public int SupplierId { get; set; }
        public string Description { get; set; }

        // Navigation Property
        public SupplierModel Supplier { get; set; }
     
    }
}
