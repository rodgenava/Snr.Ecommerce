using Application.Pricebook4;
using Data.Common.Contracts;
using Data.Definitions.Pricebook4.View;
using Data.Sql;
using Data.Sql.Mapping;
using Data.Sql.Provider;
using System.Data.Common;

namespace Infrastructure.Data.Pricebook4.View
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
            string query = "Declare @RecentPriceChanges Table (Id Int Primary Key,Store Int,Sku Int,Price Decimal (18,3),[Timestamp] Datetime) Insert Into @RecentPriceChanges (Id,Store,Sku,Price,[Timestamp]) Select Id,Store,Sku,Price,[Timestamp] From (Select Row_Number() Over (Order By P4H.[Timestamp] Desc,P4H.Sku Asc,P4H.Store Asc) As Ordinal,P4H.Id,P4H.Sku,P4H.Store,P4H.[Timestamp],P4H.Price From Pricebook4PricingHistories P4H {PART_INTERNAL_SEARCH}) ResultSet Where Ordinal>={PART_RESULT_ORDINAL_MIN} And Ordinal<={PART_RESULT_ORDINAL_MAX} Declare @OldPrices Table (Store Int,Sku Int,Price Decimal (18,3),Primary Key(Store,Sku)) Insert Into @OldPrices (Store,Sku,Price) Select Store,Sku,Price From (Select Row_Number() Over (Partition By P4H.Sku,P4H.Store Order By P4H.[Timestamp] Desc,P4H.Sku Asc,P4H.Store Asc) As Ordinal,P4H.Sku,P4H.Store,P4H.Price From Pricebook4PricingHistories P4H Join @RecentPriceChanges B On P4H.Sku=B.Sku And P4H.Store=B.Store Where P4H.Id < B.Id) B Where Ordinal=1 Select PPC.Store,PPC.Sku,M.[Description],PPC.Price As CurrentPrice,OP.Price As PreviousPrice,PPC.[Timestamp] As DateChanged From @RecentPriceChanges PPC Left Join @OldPrices OP On PPC.Store=OP.Store And PPC.Sku=OP.Sku Join Masterlist M On PPC.Sku=M.Sku Order By PPC.[Timestamp] Desc ";

            bool notHasSearchQuery = string.IsNullOrWhiteSpace(parameter.SearchTerm);

            query = query.Replace("{PART_INTERNAL_SEARCH}", notHasSearchQuery ?
                    "" :
                    "Where Cast(P4H.Sku As Varchar(15)) Like '%' + @Search + '%' Or Cast(P4H.Store As Varchar(15)) Like '%' + @Search + '%' ")
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
                       Store: item.Store,
                       Sku: item.Sku,
                       Description: item.Description,
                       CurrentPrice: item.CurrentPrice,
                       PreviousPrice: item.PreviousPrice,
                       DateChanged: item.DateChanged);
        }

        private class DataHolder
        {
            public int Store { get; set; }
            public int Sku { get; set; }
            public string? Description { get; set; }
            public decimal CurrentPrice { get; set; }
            public decimal? PreviousPrice { get; set; }
            public DateTime DateChanged { get; set; }

        }
    }
}
