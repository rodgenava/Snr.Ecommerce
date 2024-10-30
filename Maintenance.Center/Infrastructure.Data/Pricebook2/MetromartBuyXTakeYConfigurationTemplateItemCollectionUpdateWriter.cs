using Data.Common.Contracts;
using Data.Definitions.Pricebook2;
using Data.Sql;
using Data.Sql.Provider;
using System.Data;
using System.Data.Common;
using System.Text;

namespace Infrastructure.Data.Pricebook2
{
    public class MetromartBuyXTakeYConfigurationTemplateItemCollectionUpdateWriter : IAsyncDataWriter<IEnumerable<MetromartBuyXTakeYConfigurationTemplateItem>>
    {
        private readonly ISqlProvider _provider;
        private readonly ISqlCaller _caller;
        private readonly int _commandTimeout;
        private readonly int _batchSize;

        public MetromartBuyXTakeYConfigurationTemplateItemCollectionUpdateWriter(string connection, int commandTimeout, int batchSize)
        {
            _caller = new SqlCaller(_provider = new SqlServerProvider(connection));
            _commandTimeout = commandTimeout;
            _batchSize = batchSize;
        }

        public async Task WriteAsync(IEnumerable<MetromartBuyXTakeYConfigurationTemplateItem> data, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            IEnumerable<MetromartBuyXTakeYConfigurationTemplateItem[]> batches = data.Chunk(_batchSize);

            SqlTransaction transaction = _caller.CreateScopedTransaction(IsolationLevel.ReadCommitted);

            DbCommand command = _provider.CreateCommand(commandString: "");

            command.CommandTimeout = _commandTimeout;

            try
            {
                await transaction.ExecuteNonQueryAsync("Truncate Table MetromartBuyXTakeYConfigurations", token);

                var bobTheBuilder = new StringBuilder();

                foreach (MetromartBuyXTakeYConfigurationTemplateItem[] batch in batches)
                {
                    bobTheBuilder.Append("Insert Into MetromartBuyXTakeYConfigurations(Store,Sku,CampaignBegin,CampaignEnd,BuyQuantity,TakeQuantity) Values ");

                    int size = batch.Length;

                    for (int i = 0; i < size; ++i)
                    {
                        batch[i].Deconstruct(
                            out int store,
                            out int sku,
                            out _,
                            out DateOnly begin,
                            out DateOnly end,
                            out decimal buyQuantity,
                            out decimal takeQuantity);

                        bobTheBuilder.Append($"({store},{sku},'{begin:yyyy-MM-dd}','{end:yyyy-MM-dd}',{buyQuantity},{takeQuantity}){(i + 1 != size ? "," : "")}");
                    }

                    command.CommandText = bobTheBuilder.ToString();

                    await transaction.ExecuteNonQueryAsync(command, token);

                    bobTheBuilder.Clear();
                }

                await transaction.CommitAsync(token);
            }
            catch
            {
                await transaction.RollbackAsync(token);

                throw;
            }
            finally
            {
                await command.DisposeAsync();
                transaction.Dispose();
            }
        }
    }
}
