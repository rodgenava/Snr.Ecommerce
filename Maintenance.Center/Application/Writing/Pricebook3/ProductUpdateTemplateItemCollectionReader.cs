using Data.Definitions.Pricebook3;
using OfficeOpenXml;

namespace Application.Writing.Pricebook3
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
            
            ExcelWorksheet lazadaWorksheet = workbook.Worksheets["Lazada"] ?? throw new ApplicationLogicException($"Missing required worksheet: Lazada."); 
            
            int lazadaWorksheetRowsCount = lazadaWorksheet.Dimension.End.Row;

            try
            {
                for (int currentRow = 2; currentRow <= lazadaWorksheetRowsCount; ++currentRow)
                {
                    ExcelRange skuRange = lazadaWorksheet.Cells[currentRow, 1];

                    if (skuRange.Value == null || !int.TryParse(skuRange.Value.ToString(), out int sku))
                    {
                        throw new ApplicationLogicException($"Bad data was caught at Sheet \"{lazadaWorksheet.Name}\"; Column \"{lazadaWorksheet.Cells[1, 1].Value}\"; CellAddess \"{skuRange.Address}\" ({skuRange.Value})");
                    }

                    ExcelRange thresholdRange = lazadaWorksheet.Cells[currentRow, 2];

                    if (thresholdRange.Value == null || !int.TryParse(thresholdRange.Value.ToString(), out int threshold))
                    {
                        throw new ApplicationLogicException($"Bad data was caught at Sheet \"{lazadaWorksheet.Name}\"; Column \"{lazadaWorksheet.Cells[1, 2].Value}\"; CellAddess \"{thresholdRange.Address}\" ({thresholdRange.Value})");
                    }

                    ExcelRange excludemarginRange = lazadaWorksheet.Cells[currentRow, 3];

                    string excludemarginRangeValue;

                    if (excludemarginRange.Value == null || !YES_NO_VALUES.Contains(excludemarginRangeValue = (excludemarginRange.Value.ToString() ?? "").Trim().ToUpper()))
                    {
                        throw new ApplicationLogicException($"Bad data was caught at Sheet \"{lazadaWorksheet.Name}\"; Column \"{lazadaWorksheet.Cells[1, 3].Value}\"; CellAddess \"{excludemarginRange.Address}\" ({excludemarginRange.Value})");
                    }

                    ExcelRange overridePriceRange = lazadaWorksheet.Cells[currentRow, 4];

                    decimal? overridePriceRangeValue = default;

                    if (overridePriceRange.Value != null)
                    {
                        if (!decimal.TryParse(overridePriceRange.Value.ToString(), out decimal overridePrice))
                        {
                            throw new ApplicationLogicException($"Bad data was caught at Sheet \"{lazadaWorksheet.Name}\"; Column \"{lazadaWorksheet.Cells[1, 4].Value}\"; CellAddess \"{overridePriceRange.Address}\" ({overridePriceRange.Value})");
                        }
                        else
                        {
                            overridePriceRangeValue = overridePrice;
                        }
                    }

                    ExcelRange overrideQuantityRange = lazadaWorksheet.Cells[currentRow, 5];

                    decimal? overrideQuantityRangeValue = default;

                    if (overrideQuantityRange.Value != null)
                    {
                        if (!decimal.TryParse(overrideQuantityRange.Value.ToString(), out decimal overrideQuantity))
                        {
                            throw new ApplicationLogicException($"Bad data was caught at Sheet \"{lazadaWorksheet.Name}\"; Column \"{lazadaWorksheet.Cells[1, 5].Value}\"; CellAddess \"{overrideQuantityRange.Address}\" ({overrideQuantityRange.Value})");
                        }
                        else
                        {
                            overrideQuantityRangeValue = overrideQuantity;
                        }
                    }


                    ExcelRange hidePriceRange = lazadaWorksheet.Cells[currentRow, 6];

                    string hidePriceRangeValue;

                    if (hidePriceRange.Value == null || !YES_NO_VALUES.Contains(hidePriceRangeValue = (hidePriceRange.Value.ToString() ?? "").Trim().ToUpper()))
                    {
                        throw new ApplicationLogicException($"Bad data was caught at Sheet \"{lazadaWorksheet.Name}\"; Column \"{lazadaWorksheet.Cells[1, 6].Value}\"; CellAddess \"{hidePriceRange.Address}\" ({hidePriceRange.Value})");
                    }

                    ExcelRange hideQuantityRange = lazadaWorksheet.Cells[currentRow, 7];

                    string hideQuantityRangeValue;

                    if (hideQuantityRange.Value == null || !YES_NO_VALUES.Contains(hideQuantityRangeValue = (hideQuantityRange.Value.ToString() ?? "").Trim().ToUpper()))
                    {
                        throw new ApplicationLogicException($"Bad data was caught at Sheet \"{lazadaWorksheet.Name}\"; Column \"{lazadaWorksheet.Cells[1, 7].Value}\"; CellAddess \"{hideQuantityRange.Address}\" ({hideQuantityRange.Value})");
                    }

                    ExcelRange inventoryWeightRange = lazadaWorksheet.Cells[currentRow, 8];

                    if (inventoryWeightRange.Value == null || !decimal.TryParse(inventoryWeightRange.Value.ToString(), out decimal inventoryAllocationWeight))
                    {
                        throw new ApplicationLogicException($"Bad data was caught at Sheet \"{lazadaWorksheet.Name}\"; Column \"{lazadaWorksheet.Cells[1, 8].Value}\"; CellAddess \"{inventoryWeightRange.Address}\" ({inventoryWeightRange.Value})");
                    }

                    items.Add(new ProductUpdateTemplateItem(
                        Sku: sku,
                        Threshold: threshold,
                        ExcludeMargin: excludemarginRangeValue == YES_VALUE,
                        OverridePrice: overridePriceRangeValue,
                        OverrideQuantity: overrideQuantityRangeValue,
                        HidePriceInUpdate: hidePriceRangeValue == YES_VALUE,
                        HideQuantityInUpdate: hideQuantityRangeValue == YES_VALUE,
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
