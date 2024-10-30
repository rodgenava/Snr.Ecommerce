using Data.Definitions.Pricebook5;
using OfficeOpenXml;

namespace Application.Writing.Pricebook5
{
    public class PromotionUpdateItemCollectionReader : ICollectionReader<PromotionUpdateItem, Stream>
    {
        public IEnumerable<PromotionUpdateItem> ReadFrom(Stream collection)
        {
            const string YES_VALUE = "YES";

            string[] YES_NO_VALUES = new string[] { YES_VALUE, "NO" };

            List<PromotionUpdateItem> items = new();

            using (ExcelPackage package = new(collection))
            {
                IEnumerable<ExcelWorksheet> worksheets = package.Workbook.Worksheets;

                foreach (ExcelWorksheet worksheet in worksheets)
                {
                    int rowsCount = worksheet.Dimension.End.Row;

                    for (int currentRow = 2; currentRow <= rowsCount; ++currentRow)
                    {
                        //int store = Convert.ToInt32(worksheet.Cells[currentRow, 1].Value);

                        ExcelRange storeRange = worksheet.Cells[currentRow, 2];

                        if (storeRange.Value == null || !int.TryParse(storeRange.Value.ToString(), out int store))
                        {
                            throw new ApplicationLogicException($"Bad data was caught at Sheet \"{worksheet.Name}\"; Column \"{worksheet.Cells[1, 2].Value}\"; CellAddess \"{storeRange.Address}\" ({storeRange.Value})");
                        }

                        //int sku = Convert.ToInt32(worksheet.Cells[currentRow, 2].Value);

                        ExcelRange skuRange = worksheet.Cells[currentRow, 2];

                        if (skuRange.Value == null || !int.TryParse(skuRange.Value.ToString(), out int sku))
                        {
                            throw new ApplicationLogicException($"Bad data was caught at Sheet \"{worksheet.Name}\"; Column \"{worksheet.Cells[1, 2].Value}\"; CellAddess \"{skuRange.Address}\" ({skuRange.Value})");
                        }

                        //int eventNumber = Convert.ToInt32(worksheet.Cells[currentRow, 6].Value);

                        ExcelRange eventNumberRange = worksheet.Cells[currentRow, 2];

                        if (eventNumberRange.Value == null || !int.TryParse(eventNumberRange.Value.ToString(), out int eventNumber))
                        {
                            throw new ApplicationLogicException($"Bad data was caught at Sheet \"{worksheet.Name}\"; Column \"{worksheet.Cells[1, 2].Value}\"; CellAddess \"{eventNumberRange.Address}\" ({eventNumberRange.Value})");
                        }

                        //bool apply = worksheet.Cells[currentRow, 12].Value.ToString()!.ToUpper() == YES_VALUE;

                        ExcelRange applyRange = worksheet.Cells[currentRow, 4];

                        string applyRangeValue;

                        if (applyRange.Value == null || !YES_NO_VALUES.Contains(applyRangeValue = (applyRange.Value.ToString() ?? "").Trim().ToUpper()))
                        {
                            throw new ApplicationLogicException($"Bad data was caught at Sheet \"{worksheet.Name}\"; Column \"{worksheet.Cells[1, 4].Value}\"; CellAddess \"{applyRange.Address}\" ({applyRange.Value})");
                        }

                        items.Add(new PromotionUpdateItem(
                            EventNumber: eventNumber,
                            Store: store,
                            Sku: sku, 
                            Apply: applyRangeValue == YES_VALUE));
                    }
                }
            }

            return items;
        }
    }
}
