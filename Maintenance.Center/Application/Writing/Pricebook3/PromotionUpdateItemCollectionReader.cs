using Application.Writing;
using Data.Definitions.Pricebook3;
using OfficeOpenXml;

namespace Application.Writing.Pricebook3
{
    public class PromotionUpdateItemCollectionReader : ICollectionReader<PromotionUpdateItem, Stream>
    {
        public IEnumerable<PromotionUpdateItem> ReadFrom(Stream collection)
        {
            const string YES_VALUE = "YES";

            List<PromotionUpdateItem> items = new();

            using (ExcelPackage package = new(collection))
            {
                IEnumerable<ExcelWorksheet> worksheets = package.Workbook.Worksheets;

                foreach (ExcelWorksheet worksheet in worksheets)
                {

                    int rowsCount = worksheet.Dimension.End.Row;

                    for (int currentRow = 2; currentRow <= rowsCount; ++currentRow)
                    {
                        int store = Convert.ToInt32(worksheet.Cells[currentRow, 1].Value);

                        int sku = Convert.ToInt32(worksheet.Cells[currentRow, 2].Value);

                        int eventNumber = Convert.ToInt32(worksheet.Cells[currentRow, 6].Value);

                        bool apply = worksheet.Cells[currentRow, 12].Value.ToString()!.ToUpper() == YES_VALUE;

                        items.Add(new PromotionUpdateItem(eventNumber, store, sku, apply));
                    }
                }
            }

            return items;
        }
    }
}
