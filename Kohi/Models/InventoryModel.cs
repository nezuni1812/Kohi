using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.Models
{
    public class InventoryModel
    {
        public int Id { get; set; }
        public int InboundId { get; set; }
        public int IngredientId { get; set; }
        public int Quantity { get; set; }
        public int InitialQuantity { get; set; }
        public DateTime InboundDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int SupplierId { get; set; }

        public InboundModel Inbound { get; set; }
        public IngredientModel Ingredient { get; set; }
        public SupplierModel Supplier { get; set; }


    }
}
