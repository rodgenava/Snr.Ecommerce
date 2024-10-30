namespace Data.Definitions.Pricebook4
{
    public record class ProductUpdateTemplateItem(int Store, int Sku, bool ExcludeMargin, decimal? OverridePrice);
}
