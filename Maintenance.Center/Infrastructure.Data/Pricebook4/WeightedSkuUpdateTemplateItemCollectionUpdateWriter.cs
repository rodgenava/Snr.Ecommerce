using Data.Common.Contracts;
using Data.Definitions.Pricebook4;
using Data.Sql;
using Data.Sql.Provider;
using System.Data;
using System.Text;

namespace Infrastructure.Data.Pricebook4
{
    public class WeightedSkuUpdateTemplateItemCollectionUpdateWriter : IAsyncDataWriter<IEnumerable<WeightedSkuUpdateTemplateItem>>
    {
        private readonly ISqlProvider _provider;
        private readonly ISqlCaller _caller;
        private readonly int _commandTimeout;
        private readonly int _batchSize;

        public WeightedSkuUpdateTemplateItemCollectionUpdateWriter(string connection, int commandTimeout, int batchSize)
        {
            _caller = new SqlCaller(_provider = new SqlServerProvider(connection));
            _commandTimeout = commandTimeout;
            _batchSize = batchSize;
        }

        public async Task WriteAsync(IEnumerable<WeightedSkuUpdateTemplateItem> data, CancellationToken token)
        {
            IEnumerable<WeightedSkuUpdateTemplateItem[]> batches = data.Chunk(_batchSize);

            SqlTransaction transaction = _caller.CreateScopedTransaction(IsolationLevel.ReadCommitted);

            try
            {
                await transaction.ExecuteNonQueryAsync("Truncate Table [SNR_ECOMMERCE].[dbo].[WeightedItems] ", token);

                foreach (WeightedSkuUpdateTemplateItem[] batch in batches)
                {
                    StringBuilder bobTheBuilder = new("Insert Into [SNR_ECOMMERCE].[dbo].[WeightedItems] (Sku, AverageWeight) Values ");

                    int size = batch.Length;

                    for (int i = 0; i < size; ++i)
                    {
                        WeightedSkuUpdateTemplateItem item = batch[i];
                        bobTheBuilder.Append($"({item.Sku}, {item.AverageWeight}){(i + 1 != size ? "," : "")} ");
                    }
                    await transaction.ExecuteNonQueryAsync(bobTheBuilder.ToString(), token);
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
                transaction.Dispose();
            }
        }
    }
}
