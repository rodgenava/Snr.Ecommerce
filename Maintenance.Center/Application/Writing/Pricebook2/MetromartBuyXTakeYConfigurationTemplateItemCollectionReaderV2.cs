using Data.Definitions.Pricebook2;
using OfficeOpenXml;

namespace Application.Writing.Pricebook2
{
    public class MetromartBuyXTakeYConfigurationTemplateItemCollectionReaderV2 : ICollectionReaderV2<MetromartBuyXTakeYConfigurationTemplateItem, Stream>
    {
        private readonly string _dateFormat;

        public MetromartBuyXTakeYConfigurationTemplateItemCollectionReaderV2(string dateFormat)
        {
            _dateFormat = dateFormat;
        }

        public IEnumerable<MetromartBuyXTakeYConfigurationTemplateItem> ReadFrom(Stream collection, Func<MetromartBuyXTakeYConfigurationTemplateItem, (bool IsValid, string Message)>? specification = null, bool throwIfSpecificationNotMet = false)
        {
            const int skuColumn = 1,
                descriptionColumn = 2,
                campaignBeginColumn = 3,
                campaignEndColumn = 4,
                buyQuantityColumn = 5,
                takeQuantityColumn = 6;

            using var package = new ExcelPackage(collection);

            foreach(ExcelWorksheet worksheet in package.Workbook.Worksheets)
            {
                int store = int.Parse(worksheet.Name);

                int rowsCount = worksheet.Dimension.End.Row;

                for (int currentRow = 2; currentRow <= rowsCount; ++currentRow)
                {
                    ExcelRange skuRange = worksheet.Cells[currentRow, skuColumn];

                    if (skuRange.Value == null || !int.TryParse(skuRange.Value.ToString(), out int sku))
                    {
                        throw new ApplicationLogicException($"Bad data was caught at Sheet \"{worksheet.Name}\"; Column \"{worksheet.Cells[1, skuColumn].Value}\"; CellAddess \"{skuRange.Address}\" ({skuRange.Value})");
                    }

                    string description = worksheet.Cells[currentRow, descriptionColumn].Value.ToString() ?? string.Empty;

                    ExcelRange campaignBeginRange = worksheet.Cells[currentRow, campaignBeginColumn];

                    if(campaignBeginRange.Value == null || !DateOnly.TryParseExact(s: campaignBeginRange.Value.ToString(), format: _dateFormat, out DateOnly campaignBegin))
                    {
                        throw new ApplicationLogicException($"Bad data was caught at Sheet \"{worksheet.Name}\"; Column \"{worksheet.Cells[1, campaignBeginColumn].Value}\"; CellAddess \"{campaignBeginRange.Address}\" ({campaignBeginRange.Value})");
                    }

                    ExcelRange campaignEndRange = worksheet.Cells[currentRow, campaignEndColumn];

                    if (campaignEndRange.Value == null || !DateOnly.TryParseExact(s: campaignEndRange.Value.ToString(), format: _dateFormat, out DateOnly campaignEnd))
                    {
                        throw new ApplicationLogicException($"Bad data was caught at Sheet \"{worksheet.Name}\"; Column \"{worksheet.Cells[1, campaignEndColumn].Value}\"; CellAddess \"{campaignEndRange.Address}\" ({campaignEndRange.Value})");
                    }

                    ExcelRange buyQuantityRange = worksheet.Cells[currentRow, buyQuantityColumn];

                    if (buyQuantityRange.Value == null || !decimal.TryParse(buyQuantityRange.Value.ToString(), out decimal buyQuantity))
                    {
                        throw new ApplicationLogicException($"Bad data was caught at Sheet \"{worksheet.Name}\"; Column \"{worksheet.Cells[1, buyQuantityColumn].Value}\"; CellAddess \"{buyQuantityRange.Address}\" ({buyQuantityRange.Value})");
                    }

                    ExcelRange takeQuantityRange = worksheet.Cells[currentRow, takeQuantityColumn];

                    if (takeQuantityRange.Value == null || !decimal.TryParse(takeQuantityRange.Value.ToString(), out decimal takeQuantity))
                    {
                        throw new ApplicationLogicException($"Bad data was caught at Sheet \"{worksheet.Name}\"; Column \"{worksheet.Cells[1, takeQuantityColumn].Value}\"; CellAddess \"{takeQuantityRange.Address}\" ({takeQuantityRange.Value})");
                    }

                    var item = new MetromartBuyXTakeYConfigurationTemplateItem(
                        Store: store,
                        Sku: sku,
                        Description: description,
                        Begin: campaignBegin,
                        End: campaignEnd,
                        BuyQuantity: buyQuantity,
                        TakeQuantity: takeQuantity);

                    if(specification != null)
                    {
                        (bool IsValid, string Message) = specification.Invoke(item);

                        if (!IsValid && throwIfSpecificationNotMet)
                        {
                            throw new ApplicationLogicException($"An item failed to satisfy specifications: Sheet \"{worksheet.Name}\"; Row \"{currentRow}\" - {Message}");
                        }
                        else
                        {
                            continue;
                        }
                    }
                    
                    yield return item;
                }
            }
        }
    }
}
