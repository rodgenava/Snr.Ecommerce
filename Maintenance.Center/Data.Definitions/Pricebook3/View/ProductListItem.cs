namespace Data.Definitions.Pricebook3.View
{
    public record class ProductListItem(
        int Sku,
        string? Description,
        decimal AverageSales8Weeks,
        decimal TotalQuantity,
        int Threshold,
        decimal Cost,
        decimal RegularPrice,
        decimal AdjustedPrice,
        decimal OnlinePrice,
        bool ExcludeMargin);
}
