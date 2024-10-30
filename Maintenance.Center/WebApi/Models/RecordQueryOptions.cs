namespace WebApi.Models
{
    public class RecordQueryOptions
    {
        public int PageSize { get; set; }
        public int Page { get; set; }
        public string? SearchTerm { get; set; }
        public string? SortColumn { get; set; }
    }
}
