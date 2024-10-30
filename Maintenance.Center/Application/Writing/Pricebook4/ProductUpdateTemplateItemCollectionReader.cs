using Data.Definitions.Pricebook4;
using OfficeOpenXml;

namespace Application.Writing.Pricebook4
{
    public class ProductUpdateTemplateItemCollectionReader : ICollectionReader<ProductUpdateTemplateItem, Stream>
    {
        public IEnumerable<ProductUpdateTemplateItem> ReadFrom(Stream collection)
        {
            const string YES_VALUE = "YES";

            List<ProductUpdateTemplateItem> items = new();

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
                        bool excludeMargin = worksheet.Cells[currentRow, 2].Value.ToString()!.ToUpper() == YES_VALUE;

                        decimal? overridePrice = null;

                        ExcelRange overridePriceCell = worksheet.Cells[currentRow, 3];

                        if (overridePriceCell.Value != null && decimal.TryParse(overridePriceCell.Value.ToString(), out decimal op))
                        {
                            overridePrice = op;
                        }

                        items.Add(new ProductUpdateTemplateItem(
                            Store: store,
                            Sku: sku,
                            ExcludeMargin: excludeMargin,
                            OverridePrice: overridePrice));
                    }
                }
            }

            return from item in items
                   group item by new
                   {
                       item.Store,
                       item.Sku
                   }
                   into grouped
                   select grouped.First();
        }
    }
}
