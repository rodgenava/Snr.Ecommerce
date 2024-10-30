namespace Data.Definitions.Pricebook2.View
{
    public record MetromartBuyXTakeYConfigurationListItem(
        int Store,
        int Sku,
        string Description,
        DateOnly Begin,
        DateOnly End,
        decimal BuyQuantity,
        decimal TakeQuantity);
}
