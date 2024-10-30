using Data.Common.Contracts;

namespace Application.Writing
{
    public interface IStreamlinedCollectionWriter<TItem>
    {
        Task<Stream> WriteAsync(IAsyncDataSourceIterator<TItem> dataSourceIterator, CancellationToken token);
    }

    public interface IStreamlinedCollectionWriterV2<TItem>
    {
        Task WriteAsync(Stream stream, IAsyncEnumerableQuery<TItem> query, CancellationToken cancellationToken = default);
    }
}
