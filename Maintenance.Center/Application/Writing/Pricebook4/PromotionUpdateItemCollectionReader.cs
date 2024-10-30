using Data.Definitions.Pricebook4;
using OfficeOpenXml;

namespace Application.Writing.Pricebook4
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
                    int store = int.Parse(worksheet.Name);

                    int rowsCount = worksheet.Dimension.End.Row;

                    for (int currentRow = 2; currentRow <= rowsCount; ++currentRow)
                    {
                        int sku = Convert.ToInt32(worksheet.Cells[currentRow, 1].Value);
                        int promoNumber = Convert.ToInt32(worksheet.Cells[currentRow, 5].Value);
                        bool apply = worksheet.Cells[currentRow, 11].Value.ToString()!.ToUpper() == YES_VALUE;

                        items.Add(
                            new PromotionUpdateItem(
                                store,
                                sku,
                                promoNumber,
                                apply));
                    }
                }
            }

            return items;
        }
    }
}
