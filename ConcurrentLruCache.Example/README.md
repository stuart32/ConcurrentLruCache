This is a basic console application showing how the LRU cache implementation can me used.

I have implemented an ILruCacheRepository which will pulls transactions from a csv file and uses that as a mock database.

In addition to that I have written a Worker service which has a task to loop back and fourth through the transactions attempting to retrieve them.

The worker service has two implementations, one that utilised the LRU cache and another that does a standard call to the database.

The Lru caches can be configured using the LruCacheOptions in the appsettings.json file.

Running this application will show how each implementation performs the worker task.

Multiple instances of the CacheWorker can be injected into the service in the ServiceConfigurationHelper.

Some example scenarios to try:

```cs
// Two cache workers with logging enabled
// here you can see what is getting added to the cache and what is being removed.
services.AddScoped<Worker,CacheWorker>();
services.AddScoped<Worker,CacheWorker>();

// One cache worker and one repository worker to compare speeds on varying max thresholds.
services.AddScoped<Worker,CacheWorker>();
services.AddScoped<Worker, RepositoryWorker>();

// Multiple cache workers with one repository
services.AddScoped<Worker,CacheWorker>();
services.AddScoped<Worker,CacheWorker>();
services.AddScoped<Worker, RepositoryWorker>();
``` 

The number of loops can be configured in the Worker class which will change the run time.


