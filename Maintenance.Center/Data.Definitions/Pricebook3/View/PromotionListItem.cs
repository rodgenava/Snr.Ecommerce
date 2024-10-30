namespace Data.Definitions.Pricebook3.View
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
