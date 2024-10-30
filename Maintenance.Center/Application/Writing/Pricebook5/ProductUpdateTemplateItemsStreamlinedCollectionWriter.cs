using Data.Common.Contracts;
using Data.Definitions.Pricebook5;
using OfficeOpenXml;

namespace Application.Writing.Pricebook5
{

    public class ProductUpdateTemplateItemsStreamlinedCollectionWriter : IStreamlinedCollectionWriter<ProductUpdateTemplateItem>
    {
        public async Task<Stream> WriteAsync(IAsyncDataSourceIterator<ProductUpdateTemplateItem> dataSourceIterator, CancellationToken token)
        {
            const string YES_VALUE = "Yes";
            const string NO_VALUE = "No";
            const string PERCENTILE_EXPRESSION = "0.00";

            MemoryStream templateSream = new();

            ExcelPackage excelPackage = new ExcelPackage(templateSream);
            ExcelWorksheet shopeeWorksheet = excelPackage.Workbook.Worksheets.Add("Shopee");

            int shopeeCurrentRow = 1;

            shopeeWorksheet.View.FreezePanes(shopeeCurrentRow + 1, 1);
            shopeeWorksheet.Cells[shopeeCurrentRow, 1].Value = "Shopee Id";
            shopeeWorksheet.Cells[shopeeCurrentRow, 2].Value = "Sku";
            shopeeWorksheet.Cells[shopeeCurrentRow, 3].Value = "Threshold";
            shopeeWorksheet.Cells[shopeeCurrentRow, 4].Value = "Exclude Margin";
            shopeeWorksheet.Cells[shopeeCurrentRow, 5].Value = "Override Price";
            shopeeWorksheet.Cells[shopeeCurrentRow, 6].Value = "Override Quantity";
            shopeeWorksheet.Cells[shopeeCurrentRow, 7].Value = "Hide Quantity In Update";
            shopeeWorksheet.Cells[shopeeCurrentRow, 8].Value = "Hide Price In Update";
            shopeeWorksheet.Cells[shopeeCurrentRow, 9].Value = "Inventory Allocation Weight";

            try
            {
                await dataSourceIterator.IterateAsync(
                    item =>
                    {
                        ++shopeeCurrentRow;
                        shopeeWorksheet.Cells[shopeeCurrentRow, 1].Value = item.ShopeeId;
                        shopeeWorksheet.Cells[shopeeCurrentRow, 2].Value = item.Sku;
                        shopeeWorksheet.Cells[shopeeCurrentRow, 3].Value = item.Threshold;
                        shopeeWorksheet.Cells[shopeeCurrentRow, 4].Value = item.ExcludeMargin ? YES_VALUE : NO_VALUE;
                        shopeeWorksheet.Cells[shopeeCurrentRow, 5].Value = item.OverridePrice;
                        shopeeWorksheet.Cells[shopeeCurrentRow, 6].Value = item.OverrideQuantity;
                        shopeeWorksheet.Cells[shopeeCurrentRow, 7].Value = item.ExcludeInStockUpdate ? YES_VALUE : NO_VALUE;
                        shopeeWorksheet.Cells[shopeeCurrentRow, 8].Value = item.ExcludeInPriceUpdate ? YES_VALUE : NO_VALUE;
                        shopeeWorksheet.Cells[shopeeCurrentRow, 9].Value = item.InventoryAllocationWeight.ToString(PERCENTILE_EXPRESSION);
                    },
                    token);

                shopeeWorksheet.Column(9).Style.Numberformat.Format = PERCENTILE_EXPRESSION;

                shopeeWorksheet.Cells[shopeeWorksheet.Dimension.Address].AutoFitColumns();

                await excelPackage.SaveAsync(token);

                templateSream.Position = 0;

                return templateSream;
            }
            finally
            {
                excelPackage.Workbook.Dispose();
                excelPackage.Dispose();
            }
        }
    }
}
