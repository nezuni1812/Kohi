using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.Models
{
    internal class OutboundModel
    {
        public int Id { get; set; }              // Mã phiếu xuất
        public int InventoryId { get; set; }     // Mã lô hàng (liên kết với InventoryModel)
        public int Quantity { get; set; }        // Số lượng xuất từ lô này
        public DateTime OutboundDate { get; set; } // Ngày xuất kho
        public string Purpose { get; set; }      // Mục đích xuất (ví dụ: sản xuất, hư hỏng)
        public string Notes { get; set; }        // Ghi chú (tùy chọn)

        public InventoryModel Inventory { get; set; }

    }
}
