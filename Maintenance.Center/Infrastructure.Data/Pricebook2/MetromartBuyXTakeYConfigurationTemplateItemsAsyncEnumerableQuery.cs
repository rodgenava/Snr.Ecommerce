using Data.Common.Contracts;
using Data.Definitions.Pricebook2;
using Data.Sql;
using Data.Sql.Mapping;
using Data.Sql.Provider;
using System.Data.Common;
using System.Runtime.CompilerServices;

namespace Infrastructure.Data.Pricebook2
{
    public class MetromartBuyXTakeYConfigurationTemplateItemsAsyncEnumerableQuery : IAsyncEnumerableQuery<MetromartBuyXTakeYConfigurationTemplateItem>
    {
        private readonly ISqlProvider _provider;
        private readonly ISqlCaller _caller;
        private readonly int _commandTimeout;

        public MetromartBuyXTakeYConfigurationTemplateItemsAsyncEnumerableQuery(string connection, int commandTimeout)
        {
            _caller = new SqlCaller(_provider = new SqlServerProvider(connection));
            _commandTimeout = commandTimeout;
        }

        public async IAsyncEnumerable<MetromartBuyXTakeYConfigurationTemplateItem> ExecuteAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using DbCommand command = _provider.CreateCommand("Select MC.Store,MC.Sku,M.[Description],MC.CampaignBegin,MC.CampaignEnd,MC.BuyQuantity,MC.TakeQuantity From MetromartBuyXTakeYConfigurations MC Join Masterlist M On MC.Sku=M.Sku");
            command.CommandTimeout = _commandTimeout;

            IAsyncEnumerable<DataHolder> items = _caller.GetAsyncEnumerable(
                dataMapper: new ReflectionDataMapper<DataHolder>(),
                command: command,
                cancellationToken: cancellationToken);

            await foreach(DataHolder item in items)
            {
                yield return new MetromartBuyXTakeYConfigurationTemplateItem(
                    Store: item.Store,
                    Sku: item.Sku,
                    Description: item.Description,
                    Begin: DateOnly.FromDateTime(item.Begin),
                    End: DateOnly.FromDateTime(item.End),
                    BuyQuantity: item.BuyQuantity,
                    TakeQuantity: item.TakeQuantity);
            }
        }

        private class DataHolder
        {
            public int Store { get; set; }
            public int Sku { get; set; }
            public string Description { get; set; } = string.Empty;
            public DateTime Begin { get; set; }
            public DateTime End { get; set; }
            public decimal BuyQuantity { get; set; }
            public decimal TakeQuantity { get; set; }
        }
    }
}
