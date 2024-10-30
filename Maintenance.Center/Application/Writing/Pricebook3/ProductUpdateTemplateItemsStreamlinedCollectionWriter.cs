using Data.Common.Contracts;
using Data.Definitions.Pricebook3;
using OfficeOpenXml;

namespace Application.Writing.Pricebook3
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
            ExcelWorksheet lazadaWorksheet = excelPackage.Workbook.Worksheets.Add("Lazada");

            int lazadaCurrentRow = 1;

            lazadaWorksheet.View.FreezePanes(lazadaCurrentRow + 1, 1);
            lazadaWorksheet.Cells[lazadaCurrentRow, 1].Value = "Sku";
            lazadaWorksheet.Cells[lazadaCurrentRow, 2].Value = "Threshold";
            lazadaWorksheet.Cells[lazadaCurrentRow, 3].Value = "Exclude Margin";
            lazadaWorksheet.Cells[lazadaCurrentRow, 4].Value = "Override Price";
            lazadaWorksheet.Cells[lazadaCurrentRow, 5].Value = "Override Quantity";
            lazadaWorksheet.Cells[lazadaCurrentRow, 6].Value = "Hide Price In Update";
            lazadaWorksheet.Cells[lazadaCurrentRow, 7].Value = "Hide Quantity In Update";
            lazadaWorksheet.Cells[lazadaCurrentRow, 8].Value = "Inventory Allocation Weight";

            try
            {
                await dataSourceIterator.IterateAsync(
                    item =>
                    {

                        ++lazadaCurrentRow;
                        lazadaWorksheet.Cells[lazadaCurrentRow, 1].Value = item.Sku;
                        lazadaWorksheet.Cells[lazadaCurrentRow, 2].Value = item.Threshold;
                        lazadaWorksheet.Cells[lazadaCurrentRow, 3].Value = item.ExcludeMargin ? YES_VALUE : NO_VALUE;
                        lazadaWorksheet.Cells[lazadaCurrentRow, 4].Value = item.OverridePrice;
                        lazadaWorksheet.Cells[lazadaCurrentRow, 5].Value = item.OverrideQuantity;
                        lazadaWorksheet.Cells[lazadaCurrentRow, 6].Value = item.HidePriceInUpdate ? YES_VALUE : NO_VALUE;
                        lazadaWorksheet.Cells[lazadaCurrentRow, 7].Value = item.HideQuantityInUpdate ? YES_VALUE : NO_VALUE;
                        lazadaWorksheet.Cells[lazadaCurrentRow, 8].Value = item.InventoryAllocationWeight.ToString(PERCENTILE_EXPRESSION);
                    },
                    token);

                lazadaWorksheet.Column(8).Style.Numberformat.Format = PERCENTILE_EXPRESSION;

                lazadaWorksheet.Cells[lazadaWorksheet.Dimension.Address].AutoFitColumns();

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
