namespace Data.Definitions.Pricebook5.View
{
    public record class PromotionListItem(
        int Store,
        int Sku,
        string? Description,
        decimal? HighestCost,
        decimal HighestRegularPrice,
        int EventNumber,
        decimal PromoPrice,
        decimal? Margin,
        decimal PriceDifference,
        DateTime EventBegin,
        DateTime EventEnd,
        bool Apply);
}
