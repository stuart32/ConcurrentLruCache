Here is my implementation of an LRU cache.

I have utilised two data structures, a Dictionary for fast lookups from the cache
and a LinkedList for fast reordering of the cache to maintain knowledge of the least recently used item. 

My solution utilises an interface IConcurrentLruCache which contains two methods GetOrAdd and Invalidate.

The GetOrAdd method will get an item from the cache if it is available otherwise it will retrieve the value from the store.

The Invalidate method will clear the cache of all data. 

I have also implemented an repository interface called ILruCacheRepository. This can be used for retrieving data from a data store into the cache.

There is an LruCacheOptions class which contains the configurable options for the cache. 
Here you can set the max length of the cache and also whether you would like to log out the additions and evictions from the cache or not.

I have made the cache thread safe by locking the GetOrAdd call so only one thread can access it at a time.

There are several Unit tests in the ConcurrentLRUCache.Tests project.



