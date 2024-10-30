using Application.Pricebook2;
using Data.Common.Contracts;
using Data.Definitions.Pricebook2.View;
using Data.Sql;
using Data.Sql.Mapping;
using Data.Sql.Provider;
using System.Data.Common;

namespace Infrastructure.Data.Pricebook2.View
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
            string query = "Declare @RecentPriceChanges Table (Id Int Primary Key,Store Int,Sku Int,Price Decimal (18,3),[Timestamp] Datetime) Insert Into @RecentPriceChanges (Id,Store,Sku,Price,[Timestamp]) Select Id,Store,Sku,Price,[Timestamp] From (Select Row_Number() Over (Order By P2H.[Timestamp] Desc,P2H.Sku Asc,P2H.Store Asc) As Ordinal,P2H.Id,P2H.Sku,P2H.Store,P2H.[Timestamp],P2H.Price From Pricebook2PricingHistories P2H {PART_INTERNAL_SEARCH}) ResultSet Where Ordinal>={PART_RESULT_ORDINAL_MIN} And Ordinal<={PART_RESULT_ORDINAL_MAX} Declare @OldPrices Table (Store Int,Sku Int,Price Decimal (18,3),Primary Key(Store,Sku)) Insert Into @OldPrices (Store,Sku,Price) Select Store,Sku,Price From (Select Row_Number() Over (Partition By P2H.Sku,P2H.Store Order By P2H.[Timestamp] Desc,P2H.Sku Asc,P2H.Store Asc) As Ordinal,P2H.Sku,P2H.Store,P2H.Price From Pricebook2PricingHistories P2H Join @RecentPriceChanges B On P2H.Sku=B.Sku And P2H.Store=B.Store Where P2H.Id < B.Id) B Where Ordinal=1 Select PPC.Store,PPC.Sku,M.[Description],PPC.Price As CurrentPrice,OP.Price As PreviousPrice,PPC.[Timestamp] As DateChanged From @RecentPriceChanges PPC Left Join @OldPrices OP On PPC.Store=OP.Store And PPC.Sku=OP.Sku Join Masterlist M On PPC.Sku=M.Sku Order By PPC.[Timestamp] Desc ";

            bool notHasSearchQuery = string.IsNullOrWhiteSpace(parameter.SearchTerm);

            query = query.Replace("{PART_INTERNAL_SEARCH}", notHasSearchQuery ?
                    "" :
                    "Where Cast(P2H.Sku As Varchar(15)) Like '%' + @Search + '%' Or Cast(P2H.Store As Varchar(15)) Like '%' + @Search + '%' ")
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
