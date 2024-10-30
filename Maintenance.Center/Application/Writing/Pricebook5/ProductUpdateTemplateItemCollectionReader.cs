using Data.Definitions.Pricebook5;
using OfficeOpenXml;

namespace Application.Writing.Pricebook5
{
    public class ProductUpdateTemplateItemCollectionReader : ICollectionReader<ProductUpdateTemplateItem, Stream>
    {
        public IEnumerable<ProductUpdateTemplateItem> ReadFrom(Stream collection)
        {
            const string YES_VALUE = "YES";

            string[] YES_NO_VALUES = new string[] { YES_VALUE, "NO" };

            List<ProductUpdateTemplateItem> items = new();

            var package = new ExcelPackage(collection);

            ExcelWorkbook workbook = package.Workbook;
            
            ExcelWorksheet shopeeWorksheet = workbook.Worksheets["Shopee"] ?? throw new ApplicationLogicException($"Missing required worksheet: Shopee."); 
            
            int shopeeWorksheetRowsCount = shopeeWorksheet.Dimension.End.Row;

            try
            {
                for (int currentRow = 2; currentRow <= shopeeWorksheetRowsCount; ++currentRow)
                {
                    ExcelRange shopeeIdRange = shopeeWorksheet.Cells[currentRow, 1];

                    if (shopeeIdRange.Value == null || !long.TryParse(shopeeIdRange.Value.ToString(), out long shopeeId))
                    {
                        throw new ApplicationLogicException($"Bad data was caught at Sheet \"{shopeeWorksheet.Name}\"; Column \"{shopeeWorksheet.Cells[1, 1].Value}\"; CellAddess \"{shopeeIdRange.Address}\" ({shopeeIdRange.Value})");
                    }

                    ExcelRange skuRange = shopeeWorksheet.Cells[currentRow, 2];

                    if (skuRange.Value == null || !int.TryParse(skuRange.Value.ToString(), out int sku))
                    {
                        throw new ApplicationLogicException($"Bad data was caught at Sheet \"{shopeeWorksheet.Name}\"; Column \"{shopeeWorksheet.Cells[1, 2].Value}\"; CellAddess \"{skuRange.Address}\" ({skuRange.Value})");
                    }

                    ExcelRange thresholdRange = shopeeWorksheet.Cells[currentRow, 3];

                    if (thresholdRange.Value == null || !int.TryParse(thresholdRange.Value.ToString(), out int threshold))
                    {
                        throw new ApplicationLogicException($"Bad data was caught at Sheet \"{shopeeWorksheet.Name}\"; Column \"{shopeeWorksheet.Cells[1, 3].Value}\"; CellAddess \"{thresholdRange.Address}\" ({thresholdRange.Value})");
                    }

                    ExcelRange excludemarginRange = shopeeWorksheet.Cells[currentRow, 4];

                    string excludemarginRangeValue;

                    if (excludemarginRange.Value == null || !YES_NO_VALUES.Contains(excludemarginRangeValue = (excludemarginRange.Value.ToString() ?? "").Trim().ToUpper()))
                    {
                        throw new ApplicationLogicException($"Bad data was caught at Sheet \"{shopeeWorksheet.Name}\"; Column \"{shopeeWorksheet.Cells[1, 4].Value}\"; CellAddess \"{excludemarginRange.Address}\" ({excludemarginRange.Value})");
                    }

                    ExcelRange overridePriceRange = shopeeWorksheet.Cells[currentRow, 5];

                    decimal? overridePriceRangeValue = default;

                    if (overridePriceRange.Value != null)
                    {
                        if (!decimal.TryParse(overridePriceRange.Value.ToString(), out decimal overridePrice))
                        {
                            throw new ApplicationLogicException($"Bad data was caught at Sheet \"{shopeeWorksheet.Name}\"; Column \"{shopeeWorksheet.Cells[1, 5].Value}\"; CellAddess \"{overridePriceRange.Address}\" ({overridePriceRange.Value})");
                        }
                        else
                        {
                            overridePriceRangeValue = overridePrice;
                        }
                    }

                    ExcelRange overrideQuantityRange = shopeeWorksheet.Cells[currentRow, 6];

                    decimal? overrideQuantityRangeValue = default;

                    if (overrideQuantityRange.Value != null)
                    {
                        if (!decimal.TryParse(overrideQuantityRange.Value.ToString(), out decimal overrideQuantity))
                        {
                            throw new ApplicationLogicException($"Bad data was caught at Sheet \"{shopeeWorksheet.Name}\"; Column \"{shopeeWorksheet.Cells[1, 6].Value}\"; CellAddess \"{overrideQuantityRange.Address}\" ({overrideQuantityRange.Value})");
                        }
                        else
                        {
                            overrideQuantityRangeValue = overrideQuantity;
                        }
                    }

                    ExcelRange hideQuantityRange = shopeeWorksheet.Cells[currentRow, 7];

                    string hideQuantityRangeValue;

                    if (hideQuantityRange.Value == null || !YES_NO_VALUES.Contains(hideQuantityRangeValue = (hideQuantityRange.Value.ToString() ?? "").Trim().ToUpper()))
                    {
                        throw new ApplicationLogicException($"Bad data was caught at Sheet \"{shopeeWorksheet.Name}\"; Column \"{shopeeWorksheet.Cells[1, 7].Value}\"; CellAddess \"{hideQuantityRange.Address}\" ({hideQuantityRange.Value})");
                    }

                    ExcelRange hidePriceRange = shopeeWorksheet.Cells[currentRow, 8];

                    string hidePriceRangeValue;

                    if (hidePriceRange.Value == null || !YES_NO_VALUES.Contains(hidePriceRangeValue = (hidePriceRange.Value.ToString() ?? "").Trim().ToUpper()))
                    {
                        throw new ApplicationLogicException($"Bad data was caught at Sheet \"{shopeeWorksheet.Name}\"; Column \"{shopeeWorksheet.Cells[1, 8].Value}\"; CellAddess \"{hidePriceRange.Address}\" ({hidePriceRange.Value})");
                    }

                    ExcelRange inventoryWeightRange = shopeeWorksheet.Cells[currentRow, 9];

                    if (inventoryWeightRange.Value == null || !decimal.TryParse(inventoryWeightRange.Value.ToString(), out decimal inventoryAllocationWeight))
                    {
                        throw new ApplicationLogicException($"Bad data was caught at Sheet \"{shopeeWorksheet.Name}\"; Column \"{shopeeWorksheet.Cells[1, 9].Value}\"; CellAddess \"{inventoryWeightRange.Address}\" ({inventoryWeightRange.Value})");
                    }

                    items.Add(new ProductUpdateTemplateItem(
                        ShopeeId : shopeeId,
                        Sku: sku,
                        Threshold: threshold,
                        ExcludeMargin: excludemarginRangeValue == YES_VALUE,
                        OverridePrice: overridePriceRangeValue,
                        OverrideQuantity: overrideQuantityRangeValue,
                        ExcludeInStockUpdate: hideQuantityRangeValue == YES_VALUE,
                        ExcludeInPriceUpdate: hidePriceRangeValue == YES_VALUE,
                        InventoryAllocationWeight: inventoryAllocationWeight));
                }
            }
            finally
            {
                workbook.Dispose();
                package.Dispose();
            }

            return items;
        }
    }
}
