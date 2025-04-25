using System;

namespace Kohi.Utils
{
    public class RawInboundData
    {
        public string IngredientName { get; set; }
        public string SupplierName { get; set; }
        public string QuantityString { get; set; }
        public string TotalCostString { get; set; }
        public string InboundDateString { get; set; }
        public string ExpiryDateString { get; set; }

        public string Notes { get; set; }

        public int RowNumber { get; set; }

        public int? ParsedQuantity { get; set; }
        public int? ParsedTotalCost { get; set; }
        public DateTime? ParsedInboundDate { get; set; }
        public DateTime? ParsedExpiryDate { get; set; }
    }
}