using Data.Definitions.Pricebook4;
using OfficeOpenXml;

namespace Application.Writing.Pricebook4
{
    public class WeightedSkuUpdateTemplateItemCollectionReader : ICollectionReader<WeightedSkuUpdateTemplateItem, Stream>
    {
        public IEnumerable<WeightedSkuUpdateTemplateItem> ReadFrom(Stream collection)
        {
            List<WeightedSkuUpdateTemplateItem> items = new();

            using (ExcelPackage package = new(collection))
            {
                IEnumerable<ExcelWorksheet> worksheets = package.Workbook.Worksheets;

                foreach (ExcelWorksheet worksheet in worksheets)
                {
                    int rowsCount = worksheet.Dimension.End.Row;

                    for (int currentRow = 2; currentRow <= rowsCount; ++currentRow)
                    {
                        items.Add(new WeightedSkuUpdateTemplateItem(
                            Sku: worksheet.Cells[currentRow, 1].GetValue<int>(),
                            AverageWeight: worksheet.Cells[currentRow, 2].GetValue<decimal>()));
                    }
                }
            }
            return items;
        }
    }
}
