using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.Models
{
    public class InventoryModel : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public int InboundId { get; set; }
        public int IngredientId { get; set; }
        public int Quantity { get; set; }
        public int InitialQuantity { get; set; }
        public DateTime InboundDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int SupplierId { get; set; }

        public int ActualQuantity { get; set; } // Số lượng thực tế sau khi kiểm kê (không liên quan DB)

        public int Discrepancy => ActualQuantity - Quantity ; // Số lượng chênh lệch sau khi kiểm kê (không liên quan DB)
        public InboundModel Inbound { get; set; }
        public IngredientModel Ingredient { get; set; }
        public SupplierModel Supplier { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

    }
}
