using Application.Pricebook3;
using Data.Common.Contracts;
using Data.Definitions.Pricebook3.View;
using Data.Sql;
using Data.Sql.Mapping;
using Data.Sql.Provider;
using System.Data.Common;

namespace Infrastructure.Data.Pricebook3.View
{
    public class PricingHistoryListItemsPagedQuery : IAsyncQuery<IEnumerable<PricingHistoryListItem>, PricingChangeListItemsPagedRequest>
    {
        private readonly ISqlProvider _provider;
        private readonly ISqlCaller _caller;
        private readonly int _commandTimeout;

        public PricingHistoryListItemsPagedQuery(string connection, int commandTimeout)
        {
            _caller = new SqlCaller(_provider = new SqlServerProvider(connection));
            _commandTimeout = commandTimeout;
        }

        public async Task<IEnumerable<PricingHistoryListItem>> ExecuteAsync(PricingChangeListItemsPagedRequest parameter, CancellationToken token)
        {
            string query = "Declare @RecentPriceChanges Table (Id Int Primary Key,Sku Int,Price Decimal (18,3),[Timestamp] Datetime) Insert Into @RecentPriceChanges (Id,Sku,Price,[Timestamp]) Select Id,Sku,Price,[Timestamp] From (Select Row_Number() Over (Order By P3H.[Timestamp] Desc,P3H.Sku Asc) As Ordinal,P3H.Id,P3H.Sku,P3H.Price,P3H.[Timestamp] From Pricebook3PricingHistories P3H {PART_INTERNAL_SEARCH}) Base Where Ordinal>={PART_RESULT_ORDINAL_MIN} And Ordinal<={PART_RESULT_ORDINAL_MAX} Declare @OldPrices Table (Sku Int Primary Key,Price Decimal (18,3)) Insert Into @OldPrices (Sku,Price) Select Sku,Price From (Select Row_Number() Over (Partition By P3H.Sku Order By P3H.[Timestamp] Desc,P3H.Sku Asc) As Ordinal,P3H.Id,P3H.Sku,P3H.Price From Pricebook3PricingHistories P3H Join @RecentPriceChanges B On P3H.Sku=B.Sku Where P3H.Id < B.Id) B Where Ordinal=1 Select PPC.Sku,M.[Description],PPC.Price As CurrentPrice,OP.Price As PreviousPrice,PPC.[Timestamp] As DateChanged From @RecentPriceChanges PPC Left Join @OldPrices OP On PPC.Sku=OP.Sku Join Masterlist M On PPC.Sku=M.Sku Order By PPC.[Timestamp] Desc";

            bool notHasSearchQuery = string.IsNullOrWhiteSpace(parameter.SearchTerm);

            query = query.Replace("{PART_INTERNAL_SEARCH}", notHasSearchQuery ?
                    "" :
                    "Where Cast(P3H.Sku As Varchar(15)) Like '%' + @Search + '%' Or Cast(P3H.Store As Varchar(15)) Like '%' + @Search + '%' ")
                .Replace("{PART_RESULT_ORDINAL_MIN}", ((parameter.Page * parameter.PageSize) - parameter.PageSize + 1).ToString())
                .Replace("{PART_RESULT_ORDINAL_MAX}", (parameter.Page * parameter.PageSize).ToString());

            using DbCommand command = _provider.CreateCommand(query);

            command.CommandTimeout = _commandTimeout;

            if (!notHasSearchQuery)
            {
                command.Parameters.Add(_provider.CreateInputParameter("@Search", parameter.SearchTerm!, System.Data.DbType.String));
            }

            IEnumerable<DataHolder> items = await _caller.GetAsync(new ReflectionDataMapper<DataHolder>(), command, token);

            return from item in items
                   select new PricingHistoryListItem(
                       Sku: item.Sku,
                       Description: item.Description,
                       CurrentPrice: item.CurrentPrice,
                       PreviousPrice: item.PreviousPrice,
                       DateChanged: item.DateChanged);
        }

        private class DataHolder
        {
            public int Sku { get; set; }
            public string? Description { get; set; }
            public decimal CurrentPrice { get; set; }
            public decimal? PreviousPrice { get; set; }
            public DateTime DateChanged { get; set; }

        }
    }
}
