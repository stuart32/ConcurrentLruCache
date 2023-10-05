namespace ConcurrentLRUCache.Interface;

public interface ILruCachRepository<TKey, TValue>
   where TKey : notnull
{
   TValue GetCacheItem(TKey key);
}