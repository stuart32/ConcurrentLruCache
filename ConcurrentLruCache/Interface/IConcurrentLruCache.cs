namespace ConcurrentLRUCache.Interface;

public interface IConcurrentLruCache<TKey, TValue>
{
    TValue GetOrAdd(TKey key);

    void Invalidate();
}