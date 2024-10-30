using Application.Pricebook4;
using Data.Common.Contracts;
using Data.Definitions.Pricebook4.View;
using Data.Sql;
using Data.Sql.Mapping;
using Data.Sql.Provider;
using System.Data.Common;

namespace Infrastructure.Data.Pricebook4.View
{
    public class PromotionListItemsPagedQuery : IAsyncQuery<IEnumerable<PromotionListItem>, PromotionListItemsPagedRequest>
    {
        private readonly ISqlProvider _provider;
        private readonly ISqlCaller _caller;
        private readonly int _commandTimeout;
        private readonly int _requiredMinimumMargin;

        public PromotionListItemsPagedQuery(string connection, int commandTimeout, int requiredMinimumMargin)
        {
            _caller = new SqlCaller(_provider = new SqlServerProvider(connection));
            _commandTimeout = commandTimeout;
            _requiredMinimumMargin = requiredMinimumMargin;
        }
        public async Task<IEnumerable<PromotionListItem>> ExecuteAsync(PromotionListItemsPagedRequest parameter, CancellationToken token)
        {
            string query = "With OrderedSet As (Select Row_Number() Over (Order By {PART_SORT_ORDER}) As ResultOrdinal,Store,Sku,[Description],HighestCost,HighestRegularPrice,EventNumber,Price As PromoPrice,MarginPercent As Margin,PriceDifferencePercent As PriceDifference,EventBegin,EventEnd,Apply From Pricebook4PromotionsWithCalculatedMargins {PART_INTERNAL_SEARCH}) Select Store,Sku,[Description],HighestCost,HighestRegularPrice,EventNumber,PromoPrice,Margin,PriceDifference,EventBegin,EventEnd,Apply From OrderedSet OS ";

            bool notHasSearchQuery = string.IsNullOrWhiteSpace(parameter.SearchTerm);

            query = query.Replace("{PART_SORT_ORDER}", (parameter.SortColumn ?? "").ToLower() switch
            {
                "sort:sku:asc" => "Sku Asc",
                "sort:sku:desc" => "Sku Desc",
                "sort:description:asc" => "[Description] Asc",
                "sort:description:desc" => "[Description] Desc",
                "sort:store:asc:sku:asc" => "Store Asc, Sku Asc",
                _ => "Store Asc, Sku Asc"
            })
                .Replace("{PART_INTERNAL_SEARCH}", notHasSearchQuery ?
                    $"Where MarginPercent>={_requiredMinimumMargin} " :
                    $"Where MarginPercent>={_requiredMinimumMargin} And Cast(Sku as varchar(15)) Like '%' + @Search + '%' Or [Description] Like '%' + @Search + '%' Or Cast(Store as varchar(15)) Like '%' + @Search + '%'")
                + $"Where (OS.ResultOrdinal >= {(parameter.Page * parameter.PageSize) - parameter.PageSize + 1} And OS.ResultOrdinal <= {parameter.Page * parameter.PageSize}) ";

            using DbCommand command = _provider.CreateCommand(query);

            command.CommandTimeout = _commandTimeout;

            if (!notHasSearchQuery)
            {
                command.Parameters.Add(_provider.CreateInputParameter("@Search", parameter.SearchTerm!, System.Data.DbType.String));
            }

            IEnumerable<DataHolder> items = await _caller.GetAsync(new ReflectionDataMapper<DataHolder>(), command, token);

            return from item in items
                   select new PromotionListItem(
                       Store: item.Store,
                       Sku: item.Sku,
                       Description: item.Description,
                       HighestCost: item.HighestCost,
                       HighestRegularPrice: item.HighestRegularPrice,
                       EventNumber: item.EventNumber,
                       PromoPrice: item.PromoPrice,
                       Margin: item.Margin,
                       PriceDifference: item.PriceDifference,
                       EventBegin: item.EventBegin,
                       EventEnd: item.EventEnd,
                       Apply: item.Apply);
        }

        private class DataHolder
        {
            public int Store { get; set; }
            public int Sku { get; set; }
            public string? Description { get; set; }
            public decimal? HighestCost { get; set; }
            public decimal HighestRegularPrice { get; set; }
            public int EventNumber { get; set; }
            public decimal PromoPrice { get; set; }
            public decimal? Margin { get; set; }
            public decimal PriceDifference { get; set; }
            public DateTime EventBegin { get; set; }
            public DateTime EventEnd { get; set; }
            public bool Apply { get; set; }
        }
    }
}
