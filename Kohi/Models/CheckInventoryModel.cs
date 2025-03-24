using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.Models
{
    public class CheckInventoryModel : INotifyPropertyChanged
    {
        public int Id { get; set; }              // Mã phiếu nhập
        public int InventoryId { get; set; }     // Mã lô hàng (liên kết với InventoryModel)
        public int ActualQuantity { get; set; } // Số lượng thực tế sau khi kiểm kê (không liên quan DB)
        public int Discrepancy => ActualQuantity - Inventory.Quantity; // Số lượng chênh lệch sau khi kiểm kê (không liên quan DB)
        public DateTime CheckDate { get; set; } // Ngày kiểm kê
        public string? Notes { get; set; }        // Ghi chú (tùy chọn)

        public InventoryModel Inventory { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
