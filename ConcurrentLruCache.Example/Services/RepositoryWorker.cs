using ConcurrentLruCache.Example.Model;
using ConcurrentLruCache.Example.Repository;

namespace ConcurrentLruCache.Example.Services;

public class RepositoryWorker : Worker 
{
    private readonly ILogger<Worker> _logger;
    private readonly IMockDatabaseRepository _repo;

    public RepositoryWorker(ILogger<RepositoryWorker> logger, IMockDatabaseRepository repo) 
        : base(logger, repo)
    {
        _logger = logger;
        _repo = repo;
    }

    protected override Transaction GetTransaction(string transactionId)
    {
        return _repo.GetTransaction(transactionId);
    }
}