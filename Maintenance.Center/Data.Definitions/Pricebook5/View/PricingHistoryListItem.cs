namespace Data.Definitions.Pricebook5.View
{
    public record class PricingHistoryListItem(
       int Sku,
       string? Description,
       decimal CurrentPrice,
       decimal? PreviousPrice,
       DateTime DateChanged);
}
