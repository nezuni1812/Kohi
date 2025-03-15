using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.Models
{
    public class InboundModel
    {
        public int Id { get; set; }              // Mã phiếu nhập
        public int IngredientId { get; set; }    // Mã nguyên liệu (liên kết với InventoryModel)
        public int Quantity { get; set; }        // Số lượng nhập
        public DateTime InboundDate { get; set; } // Ngày nhập kho
        public DateTime ExpiryDate { get; set; }  // Ngày hết hạn của lô hàng nhập
        public int SupplierId { get; set; }     // Nhà cung cấp (tùy chọn)
        public string Notes { get; set; }        // Ghi chú (tùy chọn)

        public IngredientModel Ingredient { get; set; }
        public SupplierModel Supplier { get; set; }
    }
}
