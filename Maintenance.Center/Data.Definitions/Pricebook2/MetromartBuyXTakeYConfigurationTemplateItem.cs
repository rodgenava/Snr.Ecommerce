namespace Data.Definitions.Pricebook2
{
    public record MetromartBuyXTakeYConfigurationTemplateItem(
        int Store,
        int Sku,
        string Description,
        DateOnly Begin,
        DateOnly End,
        decimal BuyQuantity,
        decimal TakeQuantity);
}
