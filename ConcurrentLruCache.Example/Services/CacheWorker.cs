using ConcurrentLruCache.Example.Model;
using ConcurrentLruCache.Example.Repository;
using ConcurrentLRUCache.Interface;

namespace ConcurrentLruCache.Example.Services;

public class CacheWorker : Worker 
{
    private readonly ILogger<Worker> _logger;
    private readonly IMockDatabaseRepository _repo;
    private readonly IConcurrentLruCache<string, Transaction> _cache;

    public CacheWorker(ILogger<CacheWorker> logger, IMockDatabaseRepository repo, IConcurrentLruCache<string, Transaction> cache) 
        : base(logger, repo)
    {
        _logger = logger;
        _repo = repo;
        _cache = cache;
    }

    protected override Transaction GetTransaction(string transactionId)
    {
        return _cache.GetOrAdd(transactionId);
    }
}