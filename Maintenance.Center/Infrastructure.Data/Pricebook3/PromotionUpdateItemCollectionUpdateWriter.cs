using Data.Common.Contracts;
using Data.Definitions.Pricebook3;
using Data.Sql;
using Data.Sql.Provider;
using System.Data;
using System.Text;

namespace Infrastructure.Data.Pricebook3
{
    public class PromotionUpdateItemCollectionUpdateWriter : IAsyncDataWriter<IEnumerable<PromotionUpdateItem>>
    {
        private readonly ISqlProvider _provider;
        private readonly ISqlCaller _caller;
        private readonly int _commandTimeout;
        private readonly int _batchSize;

        public PromotionUpdateItemCollectionUpdateWriter(string connection, int commandTimeout, int batchSize)
        {
            _caller = new SqlCaller(_provider = new SqlServerProvider(connection));
            _commandTimeout = commandTimeout;
            _batchSize = batchSize;
        }

        public async Task WriteAsync(IEnumerable<PromotionUpdateItem> data, CancellationToken token)
        {
            IEnumerable<PromotionUpdateItem[]> batches = data.Chunk(_batchSize);

            SqlTransaction transaction = _caller.CreateScopedTransaction(IsolationLevel.ReadCommitted);

            try
            {
                foreach (PromotionUpdateItem[] batch in batches)
                {
                    StringBuilder bobTheBuilder = new();

                    foreach (PromotionUpdateItem item in batch)
                    {
                        bobTheBuilder.Append($"Update Pricebook3Promotions Set Apply={(item.Apply ? "1" : "0")} Where EventNumber={item.EventNumber} And Store={item.Store} And Sku={item.Sku}; ");
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
