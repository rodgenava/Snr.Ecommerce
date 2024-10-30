using Data.Common.Contracts;
using Data.Definitions.Pricebook4;
using OfficeOpenXml;

namespace Application.Writing.Pricebook4
{
    public class ProductUpdateTemplateItemsStreamlinedCollectionWriter : IStreamlinedCollectionWriter<ProductUpdateTemplateItem>
    {
        public async Task<Stream> WriteAsync(IAsyncDataSourceIterator<ProductUpdateTemplateItem> dataSourceIterator, CancellationToken token)
        {
            const string YES_VALUE = "Yes";
            const string NO_VALUE = "No";

            Dictionary<int, int> storeRowNumberCache = new Dictionary<int, int>();

            MemoryStream templateSream = new();
            ExcelPackage package = new(templateSream);
            ExcelWorkbook workbook = package.Workbook;
            ExcelWorksheets worksheets = workbook.Worksheets;

            void WriteItemAction(ExcelWorksheet worksheet, int row, ProductUpdateTemplateItem item)
            {
                worksheet.Cells[row, 1].Value = item.Sku;
                worksheet.Cells[row, 2].Value = item.ExcludeMargin ? YES_VALUE : NO_VALUE;
                worksheet.Cells[row, 3].Value = item.OverridePrice;
            }

            try
            {
                await dataSourceIterator.IterateAsync(item =>
                {
                    int store = item.Store;

                    if (storeRowNumberCache.TryGetValue(store, out int currentRow))
                    {
                        ExcelWorksheet worksheet = worksheets[store.ToString()];

                        WriteItemAction(worksheet, currentRow, item);

                        storeRowNumberCache[store] = ++currentRow;
                    }
                    else
                    {
                        currentRow = 1;

                        ExcelWorksheet worksheet = worksheets.Add(store.ToString());

                        WriteHeaderAction(worksheet, currentRow);

                        ++currentRow;

                        WriteItemAction(worksheet, currentRow, item);

                        storeRowNumberCache.Add(store, ++currentRow);
                    }
                },
                token);

                foreach (ExcelWorksheet ws in worksheets)
                {
                    ws.Cells[ws.Dimension.Address].AutoFitColumns();
                }

                await package.SaveAsync(token);

                templateSream.Position = 0;

                return templateSream;
            }
            finally
            {
                worksheets.Dispose();
                workbook.Dispose();
                package.Dispose();
            }
        }

        private static void WriteHeaderAction(ExcelWorksheet worksheet, int row)
        {
            worksheet.Cells[row, 1].Value = "Sku";
            worksheet.Cells[row, 2].Value = "Exclude Margin";
            worksheet.Cells[row, 3].Value = "OverridePrice";

            worksheet.View.FreezePanes(row + 1, 1);
        }
    }
}
