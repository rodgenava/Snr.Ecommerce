using Data.Common.Contracts;
using Data.Definitions.Pricebook2;
using Data.Sql;
using Data.Sql.Provider;
using System.Data;
using System.Text;

namespace Infrastructure.Data.Pricebook2
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
                await transaction.ExecuteNonQueryAsync("Truncate Table Pricebook2Items", token);
                await transaction.ExecuteNonQueryAsync("Truncate Table MetromartProductConfigurations", token);
                await transaction.ExecuteNonQueryAsync("Truncate Table PickarooProductConfigurations", token);
                await transaction.ExecuteNonQueryAsync("Truncate Table GrabMartProductConfigurations", token);
                await transaction.ExecuteNonQueryAsync("Truncate Table PandaMartProductConfigurations", token);

                foreach (ProductUpdateTemplateItem[] batch in batches)
                {
                    StringBuilder bobTheBuilderPricebook2 = new ("Insert Into Pricebook2Items(Store,Sku,Threshold,ExcludeMargin,OverridePrice,InMetroMart,InPickARoo,InGrabMart,InPandaMart) Values ");
                    StringBuilder bobTheBuilderMetroMart = new ("Insert Into MetromartProductConfigurations(Store,Sku) Values ");
                    StringBuilder bobTheBuilderPickARoo = new ("Insert Into PickarooProductConfigurations(Store,Sku) Values ");
                    StringBuilder bobTheBuilderGrabMart = new ("Insert Into GrabMartProductConfigurations(Store,Sku) Values ");
                    StringBuilder bobTheBuilderPandaMart = new("Insert Into PandaMartProductConfigurations(Store,Sku) Values ");

                    bool hasMetroMart = false,
                        hasPickARoo = false,
                        hasGrabMart = false,
                        hasPandaMart = false;

                    int size = batch.Length;

                    for (int i = 0; i < size; ++i)
                    {
                        var item = batch[i];

                        int store = item.Store,
                            sku = item.Sku,
                            threshold = item.Threshold,
                            excludeMargin = item.ExcludeMargin ? 1 : 0;

                        bool inMetroMart = item.InMetroMart,
                            inPickARoo = item.InPickARoo,
                            inGrabMart = item.InGrabMart,
                            inPandaMart = item.InPandaMart;

                        string overridePrice = !item.OverridePrice.HasValue ? "Null" : item.OverridePrice.Value.ToString();

                        bobTheBuilderPricebook2.Append($"({store},{sku},{threshold},{excludeMargin},{overridePrice},{(inMetroMart ? 1 : 0)},{(inPickARoo ? 1 : 0)},{(inGrabMart ? 1 : 0)},{(inPandaMart ? 1 : 0)}){(i + 1 != size ? "," : "")} ");

                        if (inMetroMart)
                        {
                            if (!hasMetroMart)
                            {
                                hasMetroMart = true;
                            }

                            bobTheBuilderMetroMart.Append($"({store},{sku}),");
                        }

                        if (inPickARoo)
                        {
                            if (!hasPickARoo)
                            {
                                hasPickARoo = true;
                            }

                            bobTheBuilderPickARoo.Append($"({store},{sku}),");
                        }

                        if (inGrabMart)
                        {
                            if (!hasGrabMart)
                            {
                                hasGrabMart = true;
                            }

                            bobTheBuilderGrabMart.Append($"({store},{sku}),");
                        }

                        if (inPandaMart)
                        {
                            if (!hasPandaMart)
                            {
                                hasPandaMart = true;
                            }

                            bobTheBuilderPandaMart.Append($"({store},{sku}),");
                        }
                    }

                    await transaction.ExecuteNonQueryAsync(bobTheBuilderPricebook2.ToString(), token);

                    if (hasMetroMart)
                    {
                        string query = bobTheBuilderMetroMart.ToString();

                        await transaction.ExecuteNonQueryAsync(query.Remove(query.Length - 1), token);
                    }

                    if (hasPickARoo)
                    {
                        string query = bobTheBuilderPickARoo.ToString();

                        await transaction.ExecuteNonQueryAsync(query.Remove(query.Length - 1), token);
                    }

                    if (hasGrabMart)
                    {
                        string query = bobTheBuilderGrabMart.ToString();

                        await transaction.ExecuteNonQueryAsync(query.Remove(query.Length - 1), token);
                    }

                    if (hasPandaMart)
                    {
                        string query = bobTheBuilderPandaMart.ToString();

                        await transaction.ExecuteNonQueryAsync(query.Remove(query.Length - 1), token);
                    }
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
