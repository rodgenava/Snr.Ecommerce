using Data.Common.Contracts;
using Data.Definitions.Pricebook3;
using Data.Sql;
using Data.Sql.Provider;
using System.Data;
using System.Text;

namespace Infrastructure.Data.Pricebook3
{
    public class ProductUpdateTemplateItemCollectionUpdateWriter : IAsyncDataWriter<IEnumerable<ProductUpdateTemplateItem>>
    {
        private readonly ISqlCaller _caller;
        private readonly int _commandTimeout;
        private readonly int _batchSize;

        public ProductUpdateTemplateItemCollectionUpdateWriter(string connection, int commandTimeout, int batchSize)
        {
            _caller = new SqlCaller(new SqlServerProvider(connection));
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

            try
            {
                foreach (ProductUpdateTemplateItem[] batch in batches)
                {
                    StringBuilder bobTheBuilder = new();

                    foreach (ProductUpdateTemplateItem item in batch)
                    {
                        sku = item.Sku;
                        excludeMargin = item.ExcludeMargin ? 1 : 0;
                        threshold = item.Threshold;

                        overridePrice = item.OverridePrice.HasValue ? item.OverridePrice.Value.ToString() : "NULL";
                        overrideQuantity = item.OverrideQuantity.HasValue ? item.OverrideQuantity.Value.ToString() : "NULL";

                        bobTheBuilder.Append($"Update Pricebook3Items Set Threshold={threshold},ExcludeMargin={excludeMargin},OverridePrice={overridePrice} Where Sku={sku}; ");

                        bobTheBuilder.Append($"Update LazadaProductConfigurations Set HidePriceInUpdate={(item.HidePriceInUpdate ? "1" : "0")},HideQuantityInUpdate={(item.HideQuantityInUpdate ? "1" : "0")},InventoryAllocationWeight={item.InventoryAllocationWeight:0.00},OverrideQuantity={overrideQuantity} Where Sku={sku}; ");
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
