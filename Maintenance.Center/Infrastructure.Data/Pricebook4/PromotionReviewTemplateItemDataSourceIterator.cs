using Data.Common.Contracts;
using Data.Definitions.Pricebook4;
using Data.Sql;
using Data.Sql.Mapping;
using Data.Sql.Provider;
using System.Data.Common;

namespace Infrastructure.Data.Pricebook4
{
    public class PromotionReviewTemplateItemDataSourceIterator : IAsyncDataSourceIterator<PromotionReviewTemplateItem>
    {
        private readonly ISqlProvider _provider;
        private readonly ISqlCaller _caller;
        private readonly int _commandTimeout;
        private readonly int _minimumMargin;

        public PromotionReviewTemplateItemDataSourceIterator(string connection, int commandTimeout, int minimumMargin)
        {
            _caller = new SqlCaller(_provider = new SqlServerProvider(connection));
            _commandTimeout = commandTimeout;
            _minimumMargin = minimumMargin;
        }

        public async Task IterateAsync(Action<PromotionReviewTemplateItem> itemCallback, CancellationToken token)
        {
            using DbCommand command = _provider.CreateCommand($"Select Store,Sku,[Description],HighestCost,HighestRegularPrice,EventNumber,Price,MarginPercent,PriceDifferencePercent,EventBegin,EventEnd,Apply From Pricebook4PromotionsWithCalculatedMargins Where MarginPercent>={_minimumMargin} Order By Store,Sku ");
            command.CommandTimeout = _commandTimeout;

            await _caller.IterateAsync(
                new ReflectionDataMapper<DataHolder>(),
                (DataHolder item) =>
                {
                    itemCallback(
                        new PromotionReviewTemplateItem(
                            Store: item.Store,
                            Sku: item.Sku,
                            Description: item.Description,
                            HighestCost: item.HighestCost,
                            HighestRegularPrice: item.HighestRegularPrice,
                            PromoNumber: item.EventNumber,
                            PromoPrice: item.Price,
                            MarginPercent: item.MarginPercent,
                            SaleOffPercent: item.PriceDifferencePercent,
                            PromoBegin: item.EventBegin,
                            PromoEnd: item.EventEnd,
                            Apply: item.Apply));
                },
                command,
                token);
        }

        private class DataHolder
        {
            public int Store { get; set; }
            public int Sku { get; set; }
            public string? Description { get; set; }
            public decimal HighestCost { get; set; }
            public decimal HighestRegularPrice { get; set; }
            public int EventNumber { get; set; }
            public decimal Price { get; set; }
            public decimal MarginPercent { get; set; }
            public decimal PriceDifferencePercent { get; set; }
            public DateTime EventBegin { get; set; }
            public DateTime EventEnd { get; set; }
            public bool Apply { get; set; }
        }
    }
}
