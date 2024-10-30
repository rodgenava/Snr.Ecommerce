namespace Data.Definitions.Pricebook5.View
{
    public record class ProductListItem(
        long ShopeeId,
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
