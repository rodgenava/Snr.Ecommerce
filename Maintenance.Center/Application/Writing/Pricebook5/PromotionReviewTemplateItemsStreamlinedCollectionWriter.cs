using Data.Common.Contracts;
using Data.Definitions.Pricebook5;
using OfficeOpenXml;

namespace Application.Writing.Pricebook5
{
    public class PromotionReviewTemplateItemsStreamlinedCollectionWriter : IStreamlinedCollectionWriter<PromotionReviewTemplateItem>
    {
        public async Task<Stream> WriteAsync(IAsyncDataSourceIterator<PromotionReviewTemplateItem> dataSourceIterator, CancellationToken token)
        {
            const string YES_VALUE = "Yes";
            const string NO_VALUE = "No";
            const string NUMBER_FORMAT = "#,##0.00";
            const string DATE_FORMAT = "yyyy-MM-dd";

            MemoryStream templateStream = new();

            ExcelPackage package = new(templateStream);

            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Pricebook 3 Warehouse Promotions");

            int currentRow = 1;

            worksheet.View.FreezePanes(currentRow + 1, 1);

            worksheet.Cells[currentRow, 1].Value = "Store";
            worksheet.Cells[currentRow, 2].Value = "Sku";
            worksheet.Cells[currentRow, 3].Value = "Description";
            worksheet.Cells[currentRow, 4].Value = "Highest Cost";
            worksheet.Cells[currentRow, 5].Value = "Highest Regular Price";
            worksheet.Cells[currentRow, 6].Value = "Promo Number";
            worksheet.Cells[currentRow, 7].Value = "Promo Price";
            worksheet.Cells[currentRow, 8].Value = "Margin %";
            worksheet.Cells[currentRow, 9].Value = "Sale Off %";
            worksheet.Cells[currentRow, 10].Value = "Promo Begin";
            worksheet.Cells[currentRow, 11].Value = "Promo End";
            worksheet.Cells[currentRow, 12].Value = "Apply";

            try
            {
                await dataSourceIterator.IterateAsync(item =>
                {
                    ++currentRow;

                    worksheet.Cells[currentRow, 1].Value = item.Store;
                    worksheet.Cells[currentRow, 2].Value = item.Sku;
                    worksheet.Cells[currentRow, 3].Value = item.Description;
                    worksheet.Cells[currentRow, 4].Value = item.HighestCost;
                    worksheet.Cells[currentRow, 5].Value = item.HighestRegularPrice;
                    worksheet.Cells[currentRow, 6].Value = item.EventNumber;
                    worksheet.Cells[currentRow, 7].Value = item.PromoPrice;
                    worksheet.Cells[currentRow, 8].Value = item.Margin;
                    worksheet.Cells[currentRow, 9].Value = item.PriceDifference;
                    worksheet.Cells[currentRow, 10].Value = item.EventBegin;
                    worksheet.Cells[currentRow, 11].Value = item.EventEnd;
                    worksheet.Cells[currentRow, 12].Value = item.Apply ? YES_VALUE : NO_VALUE;
                },
                token);

                worksheet.Column(4).Style.Numberformat.Format = NUMBER_FORMAT;
                worksheet.Column(5).Style.Numberformat.Format = NUMBER_FORMAT;
                worksheet.Column(7).Style.Numberformat.Format = NUMBER_FORMAT;
                worksheet.Column(8).Style.Numberformat.Format = NUMBER_FORMAT;
                worksheet.Column(9).Style.Numberformat.Format = NUMBER_FORMAT;
                worksheet.Column(10).Style.Numberformat.Format = DATE_FORMAT;
                worksheet.Column(11).Style.Numberformat.Format = DATE_FORMAT;

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                worksheet.Protection.IsProtected = true; //--------Protect whole sheet
                worksheet.Protection.SetPassword("pw@1234");
                worksheet.Column(12).Style.Locked = false; //---

                await package.SaveAsync(token);

                templateStream.Position = 0;

                return templateStream;
            }
            finally
            {
                worksheet.Dispose();
                package.Dispose();
            }




        }
    }
}
