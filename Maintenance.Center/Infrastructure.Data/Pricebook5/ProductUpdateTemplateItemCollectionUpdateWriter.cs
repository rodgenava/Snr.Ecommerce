using Data.Common.Contracts;
using Data.Definitions.Pricebook5;
using Data.Sql;
using Data.Sql.Provider;
using System.Data;
using System.Text;

namespace Infrastructure.Data.Pricebook5
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

            int sku,
                excludeMargin,
                threshold;

            string overridePrice,
                overrideQuantity;

            StringBuilder bobTheBuilder;

            try
            {
                foreach (ProductUpdateTemplateItem[] batch in batches)
                {
                    bobTheBuilder = new();

                    foreach (ProductUpdateTemplateItem item in batch)
                    {
                        sku = item.Sku;
                        excludeMargin = item.ExcludeMargin ? 1 : 0;
                        threshold = item.Threshold;

                        overridePrice = item.OverridePrice.HasValue ? item.OverridePrice.Value.ToString() : "NULL";
                        overrideQuantity = item.OverrideQuantity.HasValue ? item.OverrideQuantity.Value.ToString() : "NULL";

                        bobTheBuilder.Append($"Update Pricebook5Items Set Threshold={threshold},ExcludeMargin={excludeMargin},OverridePrice={overridePrice} Where Sku={sku}; ");

                        bobTheBuilder.Append($"Update ShopeeProductConfigurations Set ExcludeInStockUpdate={(item.ExcludeInStockUpdate ? "1" : "0")},ExcludeInPriceUpdate={(item.ExcludeInPriceUpdate ? "1" : "0")},InventoryAllocationWeight={item.InventoryAllocationWeight:0.00},OverrideQuantity={overrideQuantity} Where Sku={sku}; ");
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
