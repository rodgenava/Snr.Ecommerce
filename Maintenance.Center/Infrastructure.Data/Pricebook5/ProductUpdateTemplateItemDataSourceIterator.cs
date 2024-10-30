using Data.Common.Contracts;
using Data.Definitions.Pricebook5;
using Data.Sql;
using Data.Sql.Mapping;
using Data.Sql.Provider;
using System.Data.Common;

namespace Infrastructure.Data.Pricebook5
{

    public class ProductUpdateTemplateItemDataSourceIterator : IAsyncDataSourceIterator<ProductUpdateTemplateItem>
    {
        private readonly ISqlProvider _provider;
        private readonly ISqlCaller _caller;
        private readonly int _commandTimeout;

        public ProductUpdateTemplateItemDataSourceIterator(string connection, int commandTimeout)
        {
            _caller = new SqlCaller(_provider = new SqlServerProvider(connection));
            _commandTimeout = commandTimeout;
        }

        public async Task IterateAsync(Action<ProductUpdateTemplateItem> itemCallback, CancellationToken token)
        {
            using DbCommand command = _provider.CreateCommand("Select P5I.Sku,P5I.Threshold,P5I.ExcludeMargin,P5I.OverridePrice,SPC.OverrideQuantity,SPC.ShopeeId,SPC.ExcludeInPriceUpdate,SPC.ExcludeInStockUpdate,SPC.InventoryAllocationWeight From Pricebook5Items P5I Join ShopeeProductConfigurations SPC On P5I.Sku=SPC.Sku ");
            command.CommandTimeout = _commandTimeout;

            await _caller.IterateAsync(
                new ReflectionDataMapper<DataHolder>(),
                (item) =>
                {
                    itemCallback.Invoke(
                        new ProductUpdateTemplateItem(
                            Sku: item.Sku,
                            Threshold: item.Threshold,
                            ExcludeMargin: item.ExcludeMargin,
                            OverridePrice: item.OverridePrice,
                            OverrideQuantity: item.OverrideQuantity,
                            ShopeeId: item.ShopeeId,
                            ExcludeInPriceUpdate: item.ExcludeInPriceUpdate,
                            ExcludeInStockUpdate: item.ExcludeInStockUpdate,
                            InventoryAllocationWeight: item.InventoryAllocationWeight));
                },
                command,
                token);
        }

        private class DataHolder
        {
            public int Sku { get; set; }
            public int Threshold { get; set; }
            public bool ExcludeMargin { get; set; }
            public decimal? OverridePrice { get; set; }
            public decimal? OverrideQuantity { get; set; }
            public long ShopeeId { get; set; }
            public bool ExcludeInPriceUpdate { get; set; }
            public bool ExcludeInStockUpdate { get; set; }
            public decimal InventoryAllocationWeight { get; set; }
        }
    }
}
