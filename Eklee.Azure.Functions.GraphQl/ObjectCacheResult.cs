namespace Eklee.Azure.Functions.GraphQl
{
    public class ObjectCacheResult<T>
    {
        public ObjectCacheResult(T value, bool isFromCache)
        {
            Value = value;
            IsFromCache = isFromCache;

        }
        public bool IsFromCache { get; }
        public T Value { get; }
    }
}