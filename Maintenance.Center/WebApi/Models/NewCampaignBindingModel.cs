namespace WebApi.Models
{
    public class NewCampaignBindingModel
    {
        public string Description { get; set; } = string.Empty;
        public DateOnly Begin { get; set; }
        public DateOnly End { get; set; }
        public IEnumerable<string> Warehouses { get; set; } = Enumerable.Empty<string>();
        public IEnumerable<Guid> Scopes { get; set; } = Enumerable.Empty<Guid>();

    }
}
