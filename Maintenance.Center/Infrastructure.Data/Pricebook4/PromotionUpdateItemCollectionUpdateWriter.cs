using Data.Common.Contracts;
using Data.Definitions.Pricebook4;
using Data.Sql;
using Data.Sql.Provider;
using System.Data;
using System.Text;

namespace Infrastructure.Data.Pricebook4
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
                        int store = item.Store,
                            sku = item.Sku,
                            promoNumber = item.PromoNumber,
                            apply = item.IsApplied ? 1 : 0;

                        bobTheBuilder.Append($"Update Pricebook4Promotions Set Apply = {apply} Where EventNumber={promoNumber} And Store={store} And Sku={sku}; ");
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
