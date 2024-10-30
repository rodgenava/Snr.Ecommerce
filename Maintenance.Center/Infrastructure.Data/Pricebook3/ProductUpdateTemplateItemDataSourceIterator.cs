using Data.Common.Contracts;
using Data.Definitions.Pricebook3;
using Data.Sql;
using Data.Sql.Mapping;
using Data.Sql.Provider;
using System.Data.Common;

namespace Infrastructure.Data.Pricebook3
{

    public class ProductUpdateTemplateItemDataSourceIterator : IAsyncDataSourceIterator<ProductUpdateTemplateItem>
    {
        private readonly ISqlProvider _provider;
        private readonly ISqlCaller _caller;
        private readonly int _commandTimeout;

        public ProductUpdateTemplateItemDataSourceIterator(string connection,int commandTimeout)
        {
            _caller = new SqlCaller(_provider = new SqlServerProvider(connection));
            _commandTimeout = commandTimeout;
        }
        public async Task IterateAsync(Action<ProductUpdateTemplateItem> itemCallback, CancellationToken token)
        {
            using DbCommand command = _provider.CreateCommand("Select P3I.Sku,P3I.Threshold,P3I.ExcludeMargin,P3I.OverridePrice,LPC.OverrideQuantity,LPC.HidePriceInUpdate As LazadaHidePriceInUpdate,LPC.HideQuantityInUpdate,LPC.InventoryAllocationWeight From Pricebook3Items P3I Left Join LazadaProductConfigurations LPC On P3I.Sku=LPC.Sku ");
            command.CommandTimeout = _commandTimeout;

            await _caller.IterateAsync(
                new ReflectionDataMapper<DataHolder>(),
                (DataHolder item) =>
                {
                    itemCallback.Invoke(
                        new ProductUpdateTemplateItem(
                            Sku: item.Sku,
                            Threshold: item.Threshold,
                            ExcludeMargin: item.ExcludeMargin,
                            OverridePrice: item.OverridePrice,
                            OverrideQuantity: item.OverrideQuantity,
                            HidePriceInUpdate: item.HidePriceInUpdate,
                            HideQuantityInUpdate: item.HideQuantityInUpdate,
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
            public bool HidePriceInUpdate { get; set; }
            public bool HideQuantityInUpdate { get; set; }
            public decimal InventoryAllocationWeight { get; set; }
        }
    }
}
