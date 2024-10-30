using Application.Pricebook2;
using Data.Common.Contracts;
using Data.Definitions.Pricebook2.View;
using Data.Sql;
using Data.Sql.Mapping;
using Data.Sql.Provider;
using System.Data.Common;

namespace Infrastructure.Data.Pricebook2.View
{
    public class MetromartBuyXTakeYConfigurationListItemsPagedQuery : IAsyncQuery<IEnumerable<MetromartBuyXTakeYConfigurationListItem>, MetromartBuyXTakeYConfigurationListItemsPagedRequest>
    {
        private readonly ISqlProvider _provider;
        private readonly ISqlCaller _caller;
        private readonly int _commandTimeout;

        public MetromartBuyXTakeYConfigurationListItemsPagedQuery(string connection, int commandTimeout)
        {
            _caller = new SqlCaller(_provider = new SqlServerProvider(connection));
            _commandTimeout = commandTimeout;
        }

        public async Task<IEnumerable<MetromartBuyXTakeYConfigurationListItem>> ExecuteAsync(MetromartBuyXTakeYConfigurationListItemsPagedRequest parameter, CancellationToken token)
        {
            string query = "With OrderedSet As(Select Row_Number()Over(Order By {PART_SORT_ORDER})As ResultOrdinal,MC.Store,MC.Sku,M.[Description],MC.CampaignBegin,MC.CampaignEnd,MC.BuyQuantity,MC.TakeQuantity From MetromartBuyXTakeYConfigurations MC Join Masterlist M On MC.Sku=M.Sku {PART_INTERNAL_SEARCH})Select OS.ResultOrdinal,OS.Store,OS.Sku,OS.[Description],OS.CampaignBegin,OS.CampaignEnd,OS.BuyQuantity,OS.TakeQuantity From OrderedSet OS";

            bool notHasSearchQuery = string.IsNullOrWhiteSpace(parameter.SearchTerm);

            query = query.Replace("{PART_SORT_ORDER}", (parameter.SortColumn ?? "").ToLower() switch
            {
                "sort:sku:asc" => "M.Sku Asc",
                "sort:sku:desc" => "M.Sku Desc",
                "sort:description:asc" => "M.[Description] Asc",
                "sort:description:desc" => "M.[Description] Desc",
                _ => "M.Sku Asc"
            })
                .Replace("{PART_INTERNAL_SEARCH}", notHasSearchQuery ?
                    "" :
                    "Where Cast(M.Sku as varchar(15)) Like '%' + @Search + '%' Or M.[Description] Like '%' + @Search + '%' ")
                + $"Where (OS.ResultOrdinal >= {(parameter.Page * parameter.PageSize) - parameter.PageSize + 1} And OS.ResultOrdinal <= {parameter.Page * parameter.PageSize}) ";

            using DbCommand command = _provider.CreateCommand(query);

            command.CommandTimeout = _commandTimeout;

            if (!notHasSearchQuery)
            {
                command.Parameters.Add(_provider.CreateInputParameter("@Search", parameter.SearchTerm!, System.Data.DbType.String));
            }

            IEnumerable<DataHolder> items = await _caller.GetAsync(new ReflectionDataMapper<DataHolder>(), command, token);

            return from item in items
                   select new MetromartBuyXTakeYConfigurationListItem(
                       Store: item.Store,
                       Sku: item.Sku,
                       Description: item.Description,
                       Begin: DateOnly.FromDateTime(item.CampaignBegin),
                       End: DateOnly.FromDateTime(item.CampaignEnd),
                       BuyQuantity: item.BuyQuantity,
                       TakeQuantity: item.TakeQuantity);
        }

        private class DataHolder
        {
            public int Store { get; set; }
            public int Sku { get; set; }
            public string Description { get; set; } = string.Empty;
            public DateTime CampaignBegin { get; set; }
            public DateTime CampaignEnd { get; set; }
            public decimal BuyQuantity { get; set; }
            public decimal TakeQuantity { get; set; }

        }
    }
}
