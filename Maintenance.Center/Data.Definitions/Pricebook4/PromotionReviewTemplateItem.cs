namespace Data.Definitions.Pricebook4
{
    public record class PromotionReviewTemplateItem(
        int Store,
        int Sku,
        string? Description,
        decimal HighestCost,
        decimal HighestRegularPrice,
        int PromoNumber,
        decimal PromoPrice,
        decimal MarginPercent,
        decimal SaleOffPercent,
        DateTime PromoBegin,
        DateTime PromoEnd,
        bool Apply);
}
