using Data.Common.Contracts;
using Data.Definitions.Pricebook4;
using Data.Sql;
using Data.Sql.Mapping;
using Data.Sql.Provider;
using System.Data.Common;

namespace Infrastructure.Data.Pricebook4
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
            using DbCommand command = _provider.CreateCommand("Select Store,Sku,ExcludeMargin,OverridePrice From Pricebook4Items");
            command.CommandTimeout = _commandTimeout;

            await _caller.IterateAsync(
                new ReflectionDataMapper<DataHolder>(),
                (DataHolder item) =>
                {
                    itemCallback.Invoke(
                        new ProductUpdateTemplateItem(
                            Store: item.Store,
                            Sku: item.Sku,
                            ExcludeMargin: item.ExcludeMargin,
                            OverridePrice: item.OverridePrice));
                },
                command,
                token);
        }

        private class DataHolder
        {
            public int Store { get; set; }
            public int Sku { get; set; }
            public bool ExcludeMargin { get; set; }
            public decimal? OverridePrice { get; set; }
        }
    }
}
