namespace Data.Definitions.Pricebook2.View
{
    public record class ProductListItem(
        int Sku,
        string? Description,
        string? Class,
        string? Subclass,
        string? Department,
        string? Subdepartment,
        string? SkuTypeCode,
        string? MmsStatus);
}
