using Data.Definitions.Pricebook2;
using OfficeOpenXml;

namespace Application.Writing.Pricebook2
{
    public class ProductUpdateTemplateItemCollectionReader : ICollectionReader<ProductUpdateTemplateItem, Stream>
    {
        public IEnumerable<ProductUpdateTemplateItem> ReadFrom(Stream collection)
        {
            const string YES_VALUE = "YES";

            string[] YES_NO_VALUES = new string[] { YES_VALUE, "NO" };

            List<ProductUpdateTemplateItem> items = new();

            ExcelPackage package = new(collection);

            IEnumerable<ExcelWorksheet> worksheets = package.Workbook.Worksheets;

            const int SKU_COLUMN = 1,
                    THRESHOLD_COLUMN = 2,
                    excludeMarginColumn = 3,
                    overridePriceColumn = 4,
                    inMetromartColumn = 5,
                    inPickARooColumn = 6,
                    inGrabColumn = 7,
                    inPandaColumn = 8;

            try
            {
                foreach (ExcelWorksheet worksheet in worksheets)
                {
                    int store = int.Parse(worksheet.Name);

                    int rowsCount = worksheet.Dimension.End.Row;

                    for (int currentRow = 2; currentRow <= rowsCount; ++currentRow)
                    {

                        ExcelRange skuRange = worksheet.Cells[currentRow, SKU_COLUMN];

                        if (skuRange.Value == null || !int.TryParse(skuRange.Value.ToString(), out int sku))
                        {
                            throw new ApplicationLogicException($"Bad data was caught at Sheet \"{worksheet.Name}\"; Column \"{worksheet.Cells[1, SKU_COLUMN].Value}\"; CellAddess \"{skuRange.Address}\" ({skuRange.Value})");
                        }

                        ExcelRange thresholdRange = worksheet.Cells[currentRow, THRESHOLD_COLUMN];

                        if (thresholdRange.Value == null || !int.TryParse(thresholdRange.Value.ToString(), out int threshold))
                        {
                            throw new ApplicationLogicException($"Bad data was caught at Sheet \"{worksheet.Name}\"; Column \"{worksheet.Cells[1, THRESHOLD_COLUMN].Value}\"; CellAddess \"{thresholdRange.Address}\" ({thresholdRange.Value})");
                        }

                        ExcelRange excludemarginRange = worksheet.Cells[currentRow, excludeMarginColumn];

                        string excludemarginRangeValue;

                        if (excludemarginRange.Value == null || !YES_NO_VALUES.Contains(excludemarginRangeValue = excludemarginRange.Value.ToString()!.Trim().ToUpper()))
                        {
                            throw new ApplicationLogicException($"Bad data was caught at Sheet \"{worksheet.Name}\"; Column \"{worksheet.Cells[1, excludeMarginColumn].Value}\"; CellAddess \"{excludemarginRange.Address}\" ({excludemarginRange.Value})");
                        }

                        bool excludeMargin = excludemarginRangeValue == YES_VALUE;


                        ExcelRange overridePriceRange = worksheet.Cells[currentRow, overridePriceColumn];

                        decimal? overridePriceRangeValue = default;

                        if (overridePriceRange.Value != null)
                        {
                            if (!decimal.TryParse(overridePriceRange.Value.ToString(), out decimal overridePrice))
                            {
                                throw new ApplicationLogicException($"Bad data was caught at Sheet \"{worksheet.Name}\"; Column \"{worksheet.Cells[1, overridePriceColumn].Value}\"; CellAddess \"{overridePriceRange.Address}\" ({overridePriceRange.Value})");
                            }
                            else
                            {
                                overridePriceRangeValue = overridePrice;
                            }
                        }

                        ExcelRange inMetromartRange = worksheet.Cells[currentRow, inMetromartColumn];

                        string inMetromartRangeValue;

                        if (inMetromartRange.Value == null || !YES_NO_VALUES.Contains(inMetromartRangeValue = inMetromartRange.Value.ToString()!.Trim().ToUpper()))
                        {
                            throw new ApplicationLogicException($"Bad data was caught at Sheet \"{worksheet.Name}\"; Column \"{worksheet.Cells[1, inMetromartColumn].Value}\"; CellAddess \"{inMetromartRange.Address}\" ({inMetromartRange.Value})");
                        }

                        bool inMetromart = inMetromartRangeValue == YES_VALUE;

                        ExcelRange inPickarooRange = worksheet.Cells[currentRow, inPickARooColumn];

                        string inPickarooRangeValue;

                        if (inPickarooRange.Value == null || !YES_NO_VALUES.Contains(inPickarooRangeValue = inPickarooRange.Value.ToString()!.ToUpper()))
                        {
                            throw new ApplicationLogicException($"Bad data was caught at Sheet \"{worksheet.Name}\"; Column \"{worksheet.Cells[1, inPickARooColumn].Value}\"; CellAddess \"{inPickarooRange.Address}\" ({inPickarooRange.Value})");
                        }

                        bool inPickaroo = inPickarooRangeValue == YES_VALUE;

                        ExcelRange inGrabRange = worksheet.Cells[currentRow, inGrabColumn];

                        string inGrabRangeValue;

                        if (inGrabRange.Value == null || !YES_NO_VALUES.Contains(inGrabRangeValue = inGrabRange.Value.ToString()!.ToUpper()))
                        {
                            throw new ApplicationLogicException($"Bad data was caught at Sheet \"{worksheet.Name}\"; Column \"{worksheet.Cells[1, inGrabColumn].Value}\"; CellAddess \"{inGrabRange.Address}\" ({inGrabRange.Value})");
                        }

                        bool inGrab = inGrabRangeValue == YES_VALUE;

                        //PandaMart

                        ExcelRange inPandaRange = worksheet.Cells[currentRow, inPandaColumn];

                        string inPandaRangeValue;

                        if (inPandaRange.Value == null || !YES_NO_VALUES.Contains(inPandaRangeValue = inPandaRange.Value.ToString()!.ToUpper()))
                        {
                            throw new ApplicationLogicException($"Bad data was caught at Sheet \"{worksheet.Name}\"; Column \"{worksheet.Cells[1, inPandaColumn].Value}\"; CellAddess \"{inPandaRange.Address}\" ({inPandaRange.Value})");
                        }

                        bool inPanda = inPandaRangeValue == YES_VALUE;

                        items.Add(
                            new ProductUpdateTemplateItem(
                                Store: store,
                                Sku: sku,
                                Threshold: threshold,
                                ExcludeMargin: excludeMargin,
                                OverridePrice: overridePriceRangeValue,
                                InMetroMart: inMetromart,
                                InPickARoo: inPickaroo,
                                InGrabMart: inGrab,
                                InPandaMart: inPanda));
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
            finally
            {
                package.Dispose();
            }
        }
    }
}
