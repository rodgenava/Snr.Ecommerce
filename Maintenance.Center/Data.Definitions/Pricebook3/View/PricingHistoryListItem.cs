namespace Data.Definitions.Pricebook3.View
{
    public record class PricingHistoryListItem(
        int Sku,
        string? Description,
        decimal CurrentPrice,
        decimal? PreviousPrice,
        DateTime DateChanged);
}
