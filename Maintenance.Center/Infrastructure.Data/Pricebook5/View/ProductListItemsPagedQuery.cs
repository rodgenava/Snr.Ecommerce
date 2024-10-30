using Application.Pricebook5;
using Data.Common.Contracts;
using Data.Definitions.Pricebook5.View;
using Data.Sql;
using Data.Sql.Mapping;
using Data.Sql.Provider;
using System.Data;
using System.Data.Common;

namespace Infrastructure.Data.Pricebook5.View
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
            string query = "With OrderedSet As (Select Row_Number()Over(Order By {PART_SORT_ORDER})As ResultOrdinal,P5P.Sku,P5P.[Description],P5P.AvgSales8Weeks,P5P.TotalQuantity,P5P.Threshold,P5P.HighestCost,P5P.HighestRegularPrice,P5P.AdjustedPrice,P5P.OnlinePrice,P5P.ExcludeMargin From Pricebook5ComputationBase P5P {PART_INTERNAL_SEARCH})Select SPC.ShopeeId, OS.Sku,OS.[Description],OS.AvgSales8Weeks,OS.TotalQuantity,OS.Threshold,OS.HighestCost,OS.HighestRegularPrice,OS.AdjustedPrice,OS.OnlinePrice,OS.ExcludeMargin From OrderedSet OS Join ShopeeProductConfigurations SPC On OS.Sku=SPC.Sku ";

            bool notHasSearchQuery = string.IsNullOrWhiteSpace(parameter.SearchTerm);

            query = query.Replace("{PART_SORT_ORDER}", (parameter.SortColumn ?? "").ToLower() switch
            {
                "sort:sku:asc" => "P5P.Sku Asc",
                "sort:sku:desc" => "P5P.Sku Desc",
                "sort:description:asc" => "P5P.[Description] Asc",
                "sort:description:desc" => "P5P.[Description] Desc",
                _ => "P5P.Sku Asc"
            })
                .Replace("{PART_INTERNAL_SEARCH}", notHasSearchQuery ?
                    "" :
                    "Where CAST(P5P.Sku as varchar(15))Like '%' + @Search + '%' Or P5P.[Description] Like '%' + @Search + '%' ")
                + $"Where(OS.ResultOrdinal>={parameter.Page * parameter.PageSize - parameter.PageSize + 1} And OS.ResultOrdinal<={parameter.Page * parameter.PageSize})";

            using DbCommand command = _provider.CreateCommand(query);

            command.CommandTimeout = _commandTimeout;

            if (!notHasSearchQuery)
            {
                command.Parameters.Add(_provider.CreateInputParameter("@Search", parameter.SearchTerm!, DbType.String));
            }

            IEnumerable<DataHolder> items = await _caller.GetAsync(new ReflectionDataMapper<DataHolder>(), command, token);

            return from item in items
                   select new ProductListItem(
                       ShopeeId: item.ShopeeId,
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
            public long ShopeeId { get; set; }
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
