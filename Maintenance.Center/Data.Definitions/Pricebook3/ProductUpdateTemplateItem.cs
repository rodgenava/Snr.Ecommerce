namespace Data.Definitions.Pricebook3
{
    public record class ProductUpdateTemplateItem(
        int Sku,
        int Threshold,
        bool ExcludeMargin,
        decimal? OverridePrice,
        decimal? OverrideQuantity,
        bool HidePriceInUpdate,
        bool HideQuantityInUpdate,
        decimal InventoryAllocationWeight);

}
