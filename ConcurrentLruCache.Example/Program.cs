using ConcurrentLruCache.Example.Helper;
using ConcurrentLruCache.Example.Services;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContex ,services) => services.ConfigureExampleServices(hostContex))
    .Build();

var workers = host.Services.GetRequiredService<IEnumerable<Worker>>();

Parallel.ForEach(workers, worker =>
{
    worker.ExecuteAsync(new CancellationToken());
});