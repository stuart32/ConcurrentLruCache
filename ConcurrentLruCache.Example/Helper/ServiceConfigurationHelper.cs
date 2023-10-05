

using ConcurrentLRUCache;
using ConcurrentLruCache.Example.Model;
using ConcurrentLruCache.Example.Repository;
using ConcurrentLruCache.Example.Services;
using ConcurrentLRUCache.Interface;
using ConcurrentLRUCache.Options;

namespace ConcurrentLruCache.Example.Helper;

public static class ServiceConfigurationHelper
{
    public static void ConfigureExampleServices(this IServiceCollection services, HostBuilderContext builder)
    {
        services.Configure<LruCacheOptions>(
            builder.Configuration.GetSection(
                key: nameof(LruCacheOptions)
                )
            );
        
        services.AddSingleton<ILruCachRepository<string, Transaction>, CsvTransactionRepository>();
        services.AddSingleton<IMockDatabaseRepository, CsvTransactionRepository>();
        services.AddSingleton<IConcurrentLruCache<string, Transaction>, ConcurrentLruCache<string, Transaction>>();
        
        services.AddSingleton<Worker,CacheWorker>();
        services.AddSingleton<Worker,CacheWorker>();
        //services.AddSingleton<Worker, RepositoryWorker>();
    }
}