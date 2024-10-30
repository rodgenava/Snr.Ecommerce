using Data.Common.Contracts;
using Data.Definitions.Pricebook2;
using Data.Sql;
using Data.Sql.Mapping;
using Data.Sql.Provider;
using System.Data.Common;

namespace Infrastructure.Data.Pricebook2
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
            using DbCommand command = _provider.CreateCommand("Select Store,Sku,Threshold,ExcludeMargin,OverridePrice,InMetroMart,InPickARoo,InGrabMart,InPandaMart From Pricebook2Items ");
            command.CommandTimeout = _commandTimeout;

            await _caller.IterateAsync(
                new ReflectionDataMapper<DataHolder>(),
                (DataHolder item) =>
                {
                    itemCallback(
                    new ProductUpdateTemplateItem(
                       item.Store,
                       item.Sku,
                       item.Threshold,
                       item.ExcludeMargin,
                       item.OverridePrice,
                       item.InMetroMart,
                       item.InPickARoo,
                       item.InGrabMart,
                       item.InPandaMart));
                },
                command,
                token);
        }

        private class DataHolder
        {
            public int Store { get; set; }
            public int Sku { get; set; }
            public int Threshold { get; set; }
            public bool ExcludeMargin { get; set; }
            public decimal? OverridePrice { get; set; }
            public bool InMetroMart { get; set; }
            public bool InPickARoo { get; set; }
            public bool InGrabMart { get; set; }
            public bool InPandaMart { get; set; }
        }
    }
}
