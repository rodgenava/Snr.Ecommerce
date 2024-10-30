using Data.Common.Contracts;
using Data.Definitions.Pricebook4;
using OfficeOpenXml;

namespace Application.Writing.Pricebook4
{
    public class PromotionReviewTemplateItemsStreamlinedCollectionWriter : IStreamlinedCollectionWriter<PromotionReviewTemplateItem>
    {
        public async Task<Stream> WriteAsync(IAsyncDataSourceIterator<PromotionReviewTemplateItem> dataSourceIterator, CancellationToken token)
        {
            const string YES_VALUE = "Yes";
            const string NO_VALUE = "No";
            const string NUMBER_FORMAT = "#,##0.00";
            const string DATE_FORMAT = "yyyy-MM-dd";

            Dictionary<int, int> storeRowNumberCache = new Dictionary<int, int>();

            MemoryStream templateSream = new();
            ExcelPackage excelPackage = new(templateSream);
            ExcelWorkbook workbook = excelPackage.Workbook;
            ExcelWorksheets worksheets = workbook.Worksheets;

            void WriteItem(ExcelWorksheet worksheet, int row, PromotionReviewTemplateItem item)
            {
                worksheet.Cells[row, 1].Value = item.Sku;
                worksheet.Cells[row, 2].Value = item.Description;
                worksheet.Cells[row, 3].Value = item.HighestCost;
                worksheet.Cells[row, 4].Value = item.HighestRegularPrice;
                worksheet.Cells[row, 5].Value = item.PromoNumber;
                worksheet.Cells[row, 6].Value = item.PromoPrice;
                worksheet.Cells[row, 7].Value = item.MarginPercent;
                worksheet.Cells[row, 8].Value = item.SaleOffPercent;
                worksheet.Cells[row, 9].Value = item.PromoBegin;
                worksheet.Cells[row, 10].Value = item.PromoEnd;
                worksheet.Cells[row, 11].Value = item.Apply ? YES_VALUE : NO_VALUE;
            }

            try
            {
                await dataSourceIterator.IterateAsync(item =>
                {
                    int store = item.Store;

                    if (storeRowNumberCache.TryGetValue(store, out int currentRow))
                    {
                        ExcelWorksheet worksheet = worksheets[store.ToString()];

                        WriteItem(worksheet, currentRow, item);

                        storeRowNumberCache[store] = ++currentRow;
                    }
                    else
                    {
                        currentRow = 1;

                        ExcelWorksheet worksheet = worksheets.Add(store.ToString());

                        WriteHeader(worksheet, currentRow);

                        ++currentRow;

                        WriteItem(worksheet, currentRow, item);

                        storeRowNumberCache.Add(store, ++currentRow);
                    }
                },
                token);

                foreach (ExcelWorksheet ws in worksheets)
                {
                    ws.Column(3).Style.Numberformat.Format = NUMBER_FORMAT;
                    ws.Column(4).Style.Numberformat.Format = NUMBER_FORMAT;
                    ws.Column(6).Style.Numberformat.Format = NUMBER_FORMAT;
                    ws.Column(7).Style.Numberformat.Format = NUMBER_FORMAT;
                    ws.Column(8).Style.Numberformat.Format = NUMBER_FORMAT;
                    ws.Column(9).Style.Numberformat.Format = DATE_FORMAT;
                    ws.Column(10).Style.Numberformat.Format = DATE_FORMAT;

                    ws.Cells[ws.Dimension.Address].AutoFitColumns();

                    ws.Protection.IsProtected = true; //--------Protect whole sheet
                    ws.Protection.SetPassword("pw@1234");
                    ws.Column(11).Style.Locked = false; //---
                }



                excelPackage.Save();

                templateSream.Position = 0;

                return templateSream;
            }
            finally
            {
                worksheets.Dispose();
                workbook.Dispose();
                excelPackage.Dispose();
            }
        }

        private static void WriteHeader(ExcelWorksheet worksheet, int row)
        {
            worksheet.Cells[row, 1].Value = "Sku";
            worksheet.Cells[row, 2].Value = "Description";
            worksheet.Cells[row, 3].Value = "Highest Cost";
            worksheet.Cells[row, 4].Value = "Highest Regular Price";
            worksheet.Cells[row, 5].Value = "Promo Number";
            worksheet.Cells[row, 6].Value = "Promo Price";
            worksheet.Cells[row, 7].Value = "Margin %";
            worksheet.Cells[row, 8].Value = "Sale Off %";
            worksheet.Cells[row, 9].Value = "Promo Begin";
            worksheet.Cells[row, 10].Value = "Promo End";
            worksheet.Cells[row, 11].Value = "Apply";

            worksheet.View.FreezePanes(row + 1, 1);
        }

    }
}
