namespace Data.Definitions.Pricebook4.View
{
    public record class PricingHistoryListItem(
        int Store,
        int Sku,
        string? Description,
        decimal CurrentPrice,
        decimal? PreviousPrice,
        DateTime DateChanged);
}
