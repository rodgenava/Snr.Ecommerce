using Data.Common.Contracts;
using Data.Definitions.Pricebook3;
using Data.Sql;
using Data.Sql.Mapping;
using Data.Sql.Provider;
using System.Data.Common;

namespace Infrastructure.Data.Pricebook3
{
    public class PromotionReviewTemplateItemDataSourceIterator : IAsyncDataSourceIterator<PromotionReviewTemplateItem>
    {
        private readonly ISqlProvider _provider;
        private readonly ISqlCaller _caller;
        private readonly int _commandTimeout;
        private readonly int _requiredMinimumMargin;

        public PromotionReviewTemplateItemDataSourceIterator(string connection, int commandTimeout, int minRequiredMargin)
        {
            _caller = new SqlCaller(_provider = new SqlServerProvider(connection));
            _commandTimeout = commandTimeout;
            _requiredMinimumMargin = minRequiredMargin;
        }

        public async Task IterateAsync(Action<PromotionReviewTemplateItem> itemCallback, CancellationToken token)
        {
            using DbCommand command = _provider.CreateCommand($"Select Store,Sku,Description,HighestCost,HighestRegularPrice,EventNumber,Price As PromoPrice,MarginPercent As Margin,PriceDifferencePercent As PriceDifference,EventBegin,EventEnd,Apply From Pricebook3PromotionsWithCalculatedMargins Where MarginPercent>={_requiredMinimumMargin} Order By Store,Sku ");
            command.CommandTimeout = _commandTimeout;

            await _caller.IterateAsync(
                new ReflectionDataMapper<DataHolder>(),
                (DataHolder item) =>
                {
                    itemCallback.Invoke(
                        new PromotionReviewTemplateItem(
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
            public decimal PromoPrice { get; set; }
            public decimal Margin { get; set; }
            public decimal PriceDifference { get; set; }
            public DateTime EventBegin { get; set; }
            public DateTime EventEnd { get; set; }
            public bool Apply { get; set; }
        }
    }
}
