using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Kohi.Utils
{
    public static class ExcelDataReaderUtil
    {
        private const string ExpectedSheetName = "InboundImport";

        public static List<RawInboundData> ReadInboundDataFromExcel(string filePath)
        {
            // Kiểm tra file tồn tại
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            var dataList = new List<RawInboundData>();

            // Mở file Excel
            using (SpreadsheetDocument document = SpreadsheetDocument.Open(filePath, false))
            {
                WorkbookPart wbPart = document.WorkbookPart;
                SharedStringTable stringTable = wbPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault()?.SharedStringTable;
                Sheet sheet = wbPart.Workbook.Descendants<Sheet>()
                    .FirstOrDefault(s => s.Name != null && s.Name.Value.Equals(ExpectedSheetName, StringComparison.OrdinalIgnoreCase))
                    ?? wbPart.Workbook.Descendants<Sheet>().FirstOrDefault();

                if (sheet == null) return dataList;

                WorksheetPart wsPart = (WorksheetPart)wbPart.GetPartById(sheet.Id);
                SheetData sheetData = wsPart.Worksheet.GetFirstChild<SheetData>();
                if (sheetData == null) return dataList;

                // Đọc các hàng từ dòng thứ 2 (bỏ tiêu đề)
                var rows = sheetData.Elements<Row>().Where(r => r.RowIndex != null && r.RowIndex.Value > 1);
                foreach (Row row in rows)
                {
                    var rowData = new RawInboundData { RowNumber = (int)row.RowIndex.Value };
                    var cells = row.Elements<Cell>().ToDictionary(c => c.CellReference?.Value ?? "", c => c);

                    // Lấy giá trị từ các cột
                    rowData.IngredientName = GetCellValue(cells, $"A{row.RowIndex}", stringTable);
                    rowData.SupplierName = GetCellValue(cells, $"B{row.RowIndex}", stringTable);
                    rowData.QuantityString = GetCellValue(cells, $"C{row.RowIndex}", stringTable);
                    rowData.TotalCostString = GetCellValue(cells, $"D{row.RowIndex}", stringTable);
                    rowData.InboundDateString = GetCellValue(cells, $"E{row.RowIndex}", stringTable);
                    rowData.ExpiryDateString = GetCellValue(cells, $"F{row.RowIndex}", stringTable);

                    // Phân tích dữ liệu
                    ParseRowData(rowData);
                    dataList.Add(rowData);
                }
            }

            return dataList;
        }

        private static string GetCellValue(Dictionary<string, Cell> cells, string cellRef, SharedStringTable stringTable)
        {
            if (!cells.TryGetValue(cellRef, out Cell cell) || cell?.CellValue == null)
            {
                return null;
            }

            string value = cell.CellValue.InnerText;
            if (cell.DataType?.Value == CellValues.SharedString && stringTable != null && int.TryParse(value, out int ssid))
            {
                return stringTable.ChildElements[ssid]?.InnerText ?? "";
            }
            return value;
        }

        private static void ParseRowData(RawInboundData data)
        {
            // Phân tích Quantity
            if (int.TryParse(data.QuantityString, out int quantity))
            {
                data.ParsedQuantity = quantity;
            }

            // Phân tích TotalCost
            if (int.TryParse(data.TotalCostString, out int cost))
            {
                data.ParsedTotalCost = cost;
            }

            // Phân tích InboundDate
            if (!string.IsNullOrEmpty(data.InboundDateString))
            {
                // Thử parse như định dạng ngày
                if (DateTime.TryParse(data.InboundDateString, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime inboundDate))
                {
                    data.ParsedInboundDate = inboundDate;
                }
                // Thử parse như serial number của Excel
                else if (double.TryParse(data.InboundDateString, NumberStyles.Any, CultureInfo.InvariantCulture, out double serialDate))
                {
                    try
                    {
                        data.ParsedInboundDate = DateTime.FromOADate(serialDate);
                    }
                    catch
                    {
                        // Nếu serialDate không hợp lệ, bỏ qua
                    }
                }
            }

            // Phân tích ExpiryDate
            if (!string.IsNullOrEmpty(data.ExpiryDateString))
            {
                // Thử parse như định dạng ngày
                if (DateTime.TryParse(data.ExpiryDateString, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime expiryDate))
                {
                    data.ParsedExpiryDate = expiryDate;
                }
                // Thử parse như serial number của Excel
                else if (double.TryParse(data.ExpiryDateString, NumberStyles.Any, CultureInfo.InvariantCulture, out double serialDate))
                {
                    try
                    {
                        data.ParsedExpiryDate = DateTime.FromOADate(serialDate);
                    }
                    catch
                    {
                        // Nếu serialDate không hợp lệ, bỏ qua
                    }
                }
            }
        }
    }
}