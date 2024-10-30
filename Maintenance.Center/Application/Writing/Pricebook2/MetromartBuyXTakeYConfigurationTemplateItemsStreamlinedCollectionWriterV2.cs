using Data.Common.Contracts;
using Data.Definitions.Pricebook2;
using OfficeOpenXml;

namespace Application.Writing.Pricebook2
{
    public class MetromartBuyXTakeYConfigurationTemplateItemsStreamlinedCollectionWriterV2 : IStreamlinedCollectionWriterV2<MetromartBuyXTakeYConfigurationTemplateItem>
    {
        const string NUMBER_FORMAT = "#,##0.00";

        public async Task WriteAsync(Stream stream, IAsyncEnumerableQuery<MetromartBuyXTakeYConfigurationTemplateItem> query, CancellationToken cancellationToken = default)
        {
            Dictionary<int, int> storeRowNumberCache = new Dictionary<int, int>();

            static void WriteItemAction(ExcelWorksheet worksheet, int row, MetromartBuyXTakeYConfigurationTemplateItem item)
            {
                item.Deconstruct(
                    out _,
                    out int sku,
                    out string description,
                    out DateOnly begin,
                    out DateOnly end,
                    out decimal buyQuantity,
                    out decimal takeQuantity);

                worksheet.Cells[row, 1].Value = sku;
                worksheet.Cells[row, 2].Value = description;
                worksheet.Cells[row, 3].Value = begin.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 4].Value = end.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 5].Value = buyQuantity;
                worksheet.Cells[row, 6].Value = takeQuantity;
            }

            using ExcelPackage package = new(stream);

            using ExcelWorkbook workbook = package.Workbook;

            using ExcelWorksheets worksheets = workbook.Worksheets;

            await foreach(MetromartBuyXTakeYConfigurationTemplateItem item in query.ExecuteAsync(cancellationToken))
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
            }

            foreach (ExcelWorksheet ws in worksheets)
            {
                ws.Column(4).Style.Numberformat.Format = NUMBER_FORMAT;
                ws.Column(5).Style.Numberformat.Format = NUMBER_FORMAT;

                ws.Cells[ws.Dimension.Address].AutoFitColumns();
            }

            await package.SaveAsync(cancellationToken);
        }

        private static void WriteHeaderAction(ExcelWorksheet worksheet, int row)
        {
            worksheet.Cells[row, 1].Value = "Sku";
            worksheet.Cells[row, 2].Value = "Description";
            worksheet.Cells[row, 3].Value = "Buy X Take Y Begin";
            worksheet.Cells[row, 4].Value = "Buy X Take Y End";
            worksheet.Cells[row, 5].Value = "Buy Quantity";
            worksheet.Cells[row, 6].Value = "TakeQuantity";

            worksheet.View.FreezePanes(row + 1, 1);
        }
    }
}
