namespace Data.Definitions.Pricebook2.View
{
    public record class PricingHistoryListItem(
        int Store,
        int Sku,
        string? Description,
        decimal CurrentPrice,
        decimal? PreviousPrice,
        DateTime DateChanged);
}
