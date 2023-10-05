using ConcurrentLRUCache.Interface;
using ConcurrentLRUCache.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ConcurrentLRUCache;

public sealed class ConcurrentLruCache<TKey,TValue> : IConcurrentLruCache<TKey, TValue> 
    where TKey : notnull 
{
    private readonly LruCacheOptions _options;
    private readonly ILogger<ConcurrentLruCache<TKey, TValue>> _logger;
    private readonly ILruCachRepository<TKey, TValue> _cacheRepo;
    private readonly object _cacheLock;

    private Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>> _cachedItems;
    private LinkedList<KeyValuePair<TKey, TValue>> _orderedItems;

    public ConcurrentLruCache(IOptions<LruCacheOptions> options, ILogger<ConcurrentLruCache<TKey, TValue>> logger, ILruCachRepository<TKey, TValue> cacheRepo)
    {
        _options = options.Value;
        _logger = logger;
        _cacheRepo = cacheRepo;
        
        //Initialise data structures
        _cachedItems = new Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>>();
        _orderedItems = new LinkedList<KeyValuePair<TKey,TValue>>();
        _cacheLock = new object();
    }
    
    /// <summary>
    /// Returns the value for the associated key from the cache if available, otherwise will retrieve it from the store.
    /// </summary>
    /// <param name="key">unique identifier for the value.</param>
    /// <returns>The value for the associated key</returns>
    public TValue GetOrAdd(TKey key)
    {
        // Return value from the cache if available, otherwise get from Db.
        lock (_cacheLock)
        {
            return _cachedItems.ContainsKey(key) ? GetFromCache(key) : GetFromStorage(key);
        }
    }

    /// <summary>
    /// Clears the cache of all values.
    /// </summary>
    public void Invalidate()
    {
        //Clear data from cache.
        lock (_cacheLock)
        {
            _orderedItems.Clear();
            _cachedItems.Clear();
        }
        
        _logger.LogInformation("LRU Cache has been invalidated.");
    }
    
    private TValue GetFromCache(TKey key)
    {
        var node = _cachedItems[key];
        _orderedItems.Remove(node);
        _orderedItems.AddFirst(node);
        
        return node.Value.Value;
    }

    private void AddToCache(TKey key, TValue value)
    {
        _orderedItems.AddFirst(new KeyValuePair<TKey, TValue>(key, value));
        _cachedItems.Add(key, _orderedItems.First);
        
        if(_options.LogChanges)
            _logger.LogInformation($"Item with key '{key}' has been added to the cache.");
    }

    private void EvictFromCache()
    {
        //Evict the least recently used item from the cache.
        var lastValue = _orderedItems.Last();
    
        _orderedItems.RemoveLast();
        _cachedItems.Remove(lastValue.Key);
        
        if(_options.LogChanges)
            _logger.LogInformation($"Item with key '{lastValue.Key} has been evicted from the cache.");
    }

    private TValue GetFromStorage(TKey key)
    {
        var result = default(TValue);

        try
        {
            result = _cacheRepo.GetCacheItem(key);
            
            if (result is not null) 
            {
                if (_orderedItems.Count < _options.MaxThreshold)
                {
                    AddToCache(key, result);
                }
                else
                {
                    AddToCache(key, result);
                    EvictFromCache();
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError($"Failed to get key '{key}' from storage. {e}");
            result = default(TValue);
        }
        
        return result;
    }
}
