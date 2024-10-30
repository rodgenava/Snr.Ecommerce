namespace Data.Definitions.Pricebook5
{
    public record class ProductUpdateTemplateItem(
        long ShopeeId,
        int Sku,
        int Threshold,
        bool ExcludeMargin,
        decimal? OverridePrice,
        decimal? OverrideQuantity,
        bool ExcludeInStockUpdate,
        bool ExcludeInPriceUpdate,
        decimal InventoryAllocationWeight);
}
