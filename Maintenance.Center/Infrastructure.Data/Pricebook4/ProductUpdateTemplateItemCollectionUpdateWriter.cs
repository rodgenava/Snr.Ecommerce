using Data.Common.Contracts;
using Data.Definitions.Pricebook4;
using Data.Sql;
using Data.Sql.Provider;
using System.Data;
using System.Text;

namespace Infrastructure.Data.Pricebook4
{
    public class ProductUpdateTemplateItemCollectionUpdateWriter : IAsyncDataWriter<IEnumerable<ProductUpdateTemplateItem>>
    {
        private readonly ISqlProvider _provider;
        private readonly ISqlCaller _caller;
        private readonly int _commandTimeout;
        private readonly int _batchSize;

        public ProductUpdateTemplateItemCollectionUpdateWriter(string connection, int commandTimeout, int batchSize)
        {
            _caller = new SqlCaller(_provider = new SqlServerProvider(connection));
            _commandTimeout = commandTimeout;
            _batchSize = batchSize;
        }

        public async Task WriteAsync(IEnumerable<ProductUpdateTemplateItem> data, CancellationToken token)
        {
            IEnumerable<ProductUpdateTemplateItem[]> batches = data.Chunk(_batchSize);

            SqlTransaction transaction = _caller.CreateScopedTransaction(IsolationLevel.ReadCommitted);

            try
            {
                await transaction.ExecuteNonQueryAsync("Truncate Table Pricebook4Items ", token);

                foreach(ProductUpdateTemplateItem[] batch in batches)
                {
                    StringBuilder bobTheBuilder = new ("Insert Into Pricebook4Items(Store,Sku,ExcludeMargin,OverridePrice) Values ");

                    int size = batch.Length;

                    for (int i = 0; i < size; ++i)
                    {
                        ProductUpdateTemplateItem item = batch[i];

                        int store = item.Store,
                            sku = item.Sku,
                            excludeMargin = item.ExcludeMargin ? 1 : 0;

                        string overridePrice = !item.OverridePrice.HasValue ? "NULL" : item.OverridePrice.Value.ToString();

                        bobTheBuilder.Append($"({store},{sku},{excludeMargin},{overridePrice}) {(i + 1 != size ? "," : "")}");
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
