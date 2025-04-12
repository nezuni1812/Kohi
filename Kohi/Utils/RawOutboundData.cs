using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.Utils
{
    public class RawOutboundData
    {
        public string InventoryIdString { get; set; }
        public string QuantityString { get; set; }
        public string OutboundDateString { get; set; }
        public string Purpose { get; set; }
        public string Notes { get; set; }

        public int RowNumber { get; set; }

        public int? ParsedInventoryId { get; set; }
        public int? ParsedQuantity { get; set; }
        public DateTime? ParsedOutboundDate { get; set; }
    }
}
