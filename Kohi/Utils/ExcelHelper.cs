using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Kohi.Utils
{
    public static class ExcelHelper
    {
        private const string SheetName = "InboundImport";

        /// <summary>
        /// Creates an Excel template file for importing inbound inventory data.
        /// </summary>
        /// <param name="filePath">The full path where the template file will be saved.</param>
        /// <exception cref="IOException">Thrown if the file cannot be created.</exception>
        public static void CreateInboundExcelTemplate(string filePath)
        {
            try
            {
                using (SpreadsheetDocument document = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook))
                {
                    WorkbookPart wbPart = document.AddWorkbookPart();
                    wbPart.Workbook = new Workbook();

                    WorksheetPart wsPart = wbPart.AddNewPart<WorksheetPart>();
                    wsPart.Worksheet = new Worksheet(new SheetData());

                    Sheets sheets = wbPart.Workbook.AppendChild(new Sheets());
                    Sheet sheet = new Sheet
                    {
                        Id = wbPart.GetIdOfPart(wsPart),
                        SheetId = 1,
                        Name = SheetName
                    };
                    sheets.Append(sheet);

                    SheetData sheetData = wsPart.Worksheet.GetFirstChild<SheetData>();
                    SharedStringTablePart stringTablePart = wbPart.AddNewPart<SharedStringTablePart>();
                    stringTablePart.SharedStringTable = new SharedStringTable();

                    uint AddSharedString(string text, SharedStringTable stringTable)
                    {
                        int index = stringTable.Count();
                        stringTable.AppendChild(new SharedStringItem(new Text(text)));
                        return (uint)index;
                    }

                    Row headerRow = new Row { RowIndex = 1 };
                    string[] headers = { "IngredientName", "SupplierName", "Quantity", "TotalCost", "InboundDate", "ExpiryDate" };
                    char column = 'A';
                    foreach (var header in headers)
                    {
                        Cell cell = new Cell
                        {
                            CellReference = $"{column}1",
                            DataType = CellValues.SharedString,
                            CellValue = new CellValue(AddSharedString(header, stringTablePart.SharedStringTable).ToString())
                        };
                        headerRow.Append(cell);
                        column++;
                    }
                    sheetData.Append(headerRow);

                    stringTablePart.SharedStringTable.Save();
                    wbPart.Workbook.Save();
                }
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to create Excel template at '{filePath}': {ex.Message}", ex);
            }
        }
    }
}