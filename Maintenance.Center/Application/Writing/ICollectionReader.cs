namespace Application.Writing
{
    public interface ICollectionReader<TItem, TCollection>
    {
        IEnumerable<TItem> ReadFrom(TCollection collection);
    }

    public interface ICollectionReaderV2<TItem, TCollection>
    {
        IEnumerable<TItem> ReadFrom(TCollection collection, Func<TItem, (bool IsValid, string Message)>? specification = null, bool throwIfSpecificationNotMet = false);
    }
}
