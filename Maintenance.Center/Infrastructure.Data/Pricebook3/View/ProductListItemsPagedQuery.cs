using Application.Pricebook3;
using Data.Common.Contracts;
using Data.Definitions.Pricebook3.View;
using Data.Sql;
using Data.Sql.Mapping;
using Data.Sql.Provider;
using System.Data.Common;

namespace Infrastructure.Data.Pricebook3.View
{
    public class ProductListItemsPagedQuery : IAsyncQuery<IEnumerable<ProductListItem>, ProductListItemsPagedRequest>
    {
        private readonly ISqlProvider _provider;
        private readonly ISqlCaller _caller;
        private readonly int _commandTimeout;

        public ProductListItemsPagedQuery(string connection, int commandTimeout)
        {
            _caller = new SqlCaller(_provider = new SqlServerProvider(connection));
            _commandTimeout = commandTimeout;

        }
        public async Task<IEnumerable<ProductListItem>> ExecuteAsync(ProductListItemsPagedRequest parameter, CancellationToken token)
        {
            string query = "With OrderedSet As ( Select Row_Number() Over ( Order By {PART_SORT_ORDER} ) As ResultOrdinal,P3P.Sku,P3P.[Description],P3P.AvgSales8Weeks,P3P.TotalQuantity,P3P.Threshold,P3P.HighestCost,P3P.HighestRegularPrice,P3P.AdjustedPrice,P3P.OnlinePrice,P3P.ExcludeMargin From Pricebook3ComputationBase P3P {PART_INTERNAL_SEARCH} ) Select OS.Sku,OS.[Description],OS.AvgSales8Weeks,OS.TotalQuantity,OS.Threshold,OS.HighestCost,OS.HighestRegularPrice,OS.AdjustedPrice,OS.OnlinePrice,OS.ExcludeMargin From OrderedSet OS ";

            bool notHasSearchQuery = string.IsNullOrWhiteSpace(parameter.SearchTerm);

            query = query.Replace("{PART_SORT_ORDER}", (parameter.SortColumn ?? "").ToLower() switch
            {
                "sort:sku:asc" => "P3P.Sku Asc",
                "sort:sku:desc" => "P3P.Sku Desc",
                "sort:description:asc" => "P3P.[Description] Asc",
                "sort:description:desc" => "P3P.[Description] Desc",
                _ => "P3P.Sku Asc"
            })
                .Replace("{PART_INTERNAL_SEARCH}", notHasSearchQuery ?
                    "" :
                    "Where CAST(P3P.Sku as varchar(15)) Like '%' + @Search + '%' Or P3P.[Description] Like '%' + @Search + '%' ")
                + $"Where (OS.ResultOrdinal >= {(parameter.Page * parameter.PageSize) - parameter.PageSize + 1} And OS.ResultOrdinal <= {parameter.Page * parameter.PageSize}) ";

            using DbCommand command = _provider.CreateCommand(query);

            command.CommandTimeout = _commandTimeout;

            if (!notHasSearchQuery)
            {
                command.Parameters.Add(_provider.CreateInputParameter("@Search", parameter.SearchTerm!, System.Data.DbType.String));
            }

            IEnumerable<DataHolder> items = await _caller.GetAsync(new ReflectionDataMapper<DataHolder>(), command, token);

            return from item in items
                   select new ProductListItem(
                       Sku: item.Sku,
                       Description: item.Description,
                       AverageSales8Weeks: item.AvgSales8Weeks,
                       TotalQuantity: item.TotalQuantity,
                       Threshold: item.Threshold,
                       Cost: item.HighestCost,
                       RegularPrice: item.HighestRegularPrice,
                       AdjustedPrice: item.AdjustedPrice,
                       OnlinePrice: item.OnlinePrice,
                       ExcludeMargin: item.ExcludeMargin);
        }

        private class DataHolder
        {
            public int Sku { get; set; }
            public string? Description { get; set; }
            public decimal AvgSales8Weeks { get; set; }
            public decimal TotalQuantity { get; set; }
            public int Threshold { get; set; }
            public decimal HighestCost { get; set; }
            public decimal HighestRegularPrice { get; set; }
            public decimal AdjustedPrice { get; set; }
            public decimal OnlinePrice { get; set; }
            public bool ExcludeMargin { get; set; }
        }
    }
}
