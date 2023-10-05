using System.Diagnostics;
using ConcurrentLruCache.Example.Model;
using ConcurrentLruCache.Example.Repository;

namespace ConcurrentLruCache.Example.Services;

public abstract class Worker
{
    private readonly ILogger<Worker> _logger;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly IMockDatabaseRepository _repository;
    private const int LOOP_COUNT = 5;

    protected Worker(ILogger<Worker> logger, IMockDatabaseRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    protected abstract Transaction GetTransaction(string transactionId);

    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var ids = _repository.GetTransactionIds();
        var sw = new Stopwatch();

        sw.Start();
        var start = DateTimeOffset.Now;
        _logger.LogInformation("Started at: {time}", start);

        for (int i = 0; i < LOOP_COUNT; i++)
        {
            LoopThroughTransactionIds(ids);
        }

        var end = DateTimeOffset.Now;
        _logger.LogInformation("Finished at: {time}", end);
        sw.Stop();

        var timespan = end - start;
        
        _logger.LogInformation("{worker} took {timespan} to complete",this.GetType().Name, timespan);
    }

    private void LoopThroughTransactionIds(List<string> ids)
    {
        //Loop through ids forward
        for (int i = 0; i < ids.Count; i++)
        {
            GetTransaction(ids[i]);
        }
        
        //Loop through ids backwards
        for (int i = ids.Count-1; i >= 0; i--)
        {
            GetTransaction(ids[i]);
        }
    }
}