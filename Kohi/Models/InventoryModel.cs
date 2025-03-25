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
        public int Quantity { get; set; }
        public DateTime InboundDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public InboundModel? Inbound { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

    }
}
