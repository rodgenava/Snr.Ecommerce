namespace Data.Definitions.Pricebook4.View
{
    public record class WeightedSkuListItem(
        int Sku,
        string? Department,
        string? Subdepartment,
        string? Class,
        string? Subclass,
        string? Description,
        string? SkuTypeCode,
        decimal AverageWeight);
}
