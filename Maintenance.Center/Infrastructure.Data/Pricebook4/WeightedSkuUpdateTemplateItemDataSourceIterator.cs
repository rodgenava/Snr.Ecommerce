using Data.Common.Contracts;
using Data.Definitions.Pricebook4;
using Data.Sql;
using Data.Sql.Mapping;
using Data.Sql.Provider;
using System.Data.Common;

namespace Infrastructure.Data.Pricebook4
{

    public class WeightedSkuUpdateTemplateItemDataSourceIterator : IAsyncDataSourceIterator<WeightedSkuUpdateTemplateItem>
    {
        private readonly ISqlProvider _provider;
        private readonly ISqlCaller _caller;
        private readonly int _commandTimeout;

        public WeightedSkuUpdateTemplateItemDataSourceIterator(string connection, int commandTimeout)
        {
            _caller = new SqlCaller(_provider = new SqlServerProvider(connection));
            _commandTimeout = commandTimeout;
        }

        public async Task IterateAsync(Action<WeightedSkuUpdateTemplateItem> itemCallback, CancellationToken token)
        {
            using DbCommand command = _provider.CreateCommand("Select Sku, AverageWeight From [SNR_ECOMMERCE].[dbo].[WeightedItems]");
            command.CommandTimeout = _commandTimeout;

            await _caller.IterateAsync(
                new ReflectionDataMapper<DataHolder>(),
                (DataHolder item) =>
                {
                    itemCallback.Invoke(
                        new WeightedSkuUpdateTemplateItem(
                            Sku: item.Sku,
                            AverageWeight: item.AverageWeight));
                },
                command,
                token);
        }

        private class DataHolder
        {
            public int Sku { get; set; }
            public decimal AverageWeight { get; set; }
        }
    }
}
