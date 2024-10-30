using Data.Common.Contracts;
using Data.Definitions.Pricebook4;
using OfficeOpenXml;

namespace Application.Writing.Pricebook4
{
    public class WeightedSkuUpdateTemplateItemsStreamlinedCollectionWriter : IStreamlinedCollectionWriter<WeightedSkuUpdateTemplateItem>
    {
        public async Task<Stream> WriteAsync(IAsyncDataSourceIterator<WeightedSkuUpdateTemplateItem> dataSourceIterator, CancellationToken token)
        {
            MemoryStream templateSream = new();

            ExcelPackage package = new(templateSream);

            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("S&R Online Store Products");

            int currentRow = 1;

            worksheet.View.FreezePanes(currentRow + 1, 1);

            worksheet.Cells[currentRow, 1].Value = "Sku";
            worksheet.Cells[currentRow, 2].Value = "Avg. Weight";

            try
            {
                await dataSourceIterator.IterateAsync(item =>
                {
                    ++currentRow;

                    worksheet.Cells[currentRow, 1].Value = item.Sku;
                    worksheet.Cells[currentRow, 2].Value = item.AverageWeight;

                },
                token);

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                package.Save();

                templateSream.Position = 0;

                return templateSream;
            }
            finally
            {
                worksheet.Dispose();
                package.Dispose();
            }
        }
    }
}
