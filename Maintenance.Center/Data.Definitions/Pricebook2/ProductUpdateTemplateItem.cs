namespace Data.Definitions.Pricebook2
{
    public record ProductUpdateTemplateItem(
        int Store, 
        int Sku, 
        int Threshold,
        bool ExcludeMargin, 
        decimal? OverridePrice, 
        bool InMetroMart, 
        bool InPickARoo, 
        bool InGrabMart,
        bool InPandaMart);
}
