using ConcurrentLRUCache.Interface;
using ConcurrentLRUCache.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace ConcurrentLRUCache.Tests;

[TestFixture]
public class ConcurrentLruCacheTests
{
    private IConcurrentLruCache<int, string> _testCache;
    private IOptions<LruCacheOptions> _mockOptions;
    private ILogger<ConcurrentLruCache<int, string>> _mockLogger;
    private ILruCachRepository<int, string> _mockRepo;
    
    [SetUp]
    public void Setup()
    {
        _mockLogger = Substitute.For<ILogger<ConcurrentLruCache<int, string>>>();
        _mockRepo = Substitute.For<ILruCachRepository<int, string>>();
        
        _mockRepo.GetCacheItem(1).Returns("A");
        _mockRepo.GetCacheItem(2).Returns("B");
        _mockRepo.GetCacheItem(3).Returns("C");
        _mockRepo.GetCacheItem(4).Returns("D");
        _mockRepo.GetCacheItem(5).Returns("E");
        _mockRepo.GetCacheItem(6).Returns("F");
        _mockRepo.GetCacheItem(7).Returns("G");
        _mockRepo.GetCacheItem(8).Returns("H");
        _mockRepo.GetCacheItem(9).Returns("I");
        _mockRepo.GetCacheItem(10).Returns("J");
        _mockRepo.GetCacheItem(11).Returns("K");
    }

    [Test]
    public void should_get_from_storage_if_not_present()
    {
        //Arrange
        _mockOptions = Microsoft.Extensions.Options.Options.Create<LruCacheOptions>(new LruCacheOptions
        {
            MaxThreshold = 4
        });
        _testCache = new ConcurrentLruCache<int, string>(_mockOptions, _mockLogger, _mockRepo);

        //Act
        var value = _testCache.GetOrAdd(1);

        //Assert
        // Repo should be called when nothing in the cache.
        _mockRepo.Received(1).GetCacheItem(1);
        Assert.AreEqual("A", value);
    }


    [Test]
    public void should_get_from_cache_if_present()
    {
        //Arrange
        _mockOptions = Microsoft.Extensions.Options.Options.Create<LruCacheOptions>(new LruCacheOptions
        {
            MaxThreshold = 4
        });
        _testCache = new ConcurrentLruCache<int, string>(_mockOptions, _mockLogger, _mockRepo);
        
        //Act, Assert
        var value = _testCache.GetOrAdd(1);
        _mockRepo.Received(1).GetCacheItem(1);
        Assert.AreEqual("A", value);
        
        value = _testCache.GetOrAdd(2);
        _mockRepo.Received(1).GetCacheItem(2);
        Assert.AreEqual("B", value);
        
        value = _testCache.GetOrAdd(3);
        _mockRepo.Received(1).GetCacheItem(3);
        Assert.AreEqual("C", value);
        
        // No second call to the Db meaning data was retrieved from the cache.
        value = _testCache.GetOrAdd(3);
        _mockRepo.Received(1).GetCacheItem(3); 
        Assert.AreEqual("C", value);
    }
    
    [Test]
    public void should_evict_least_recently_used_item()
    {
        //Arrange
        _mockOptions = Microsoft.Extensions.Options.Options.Create<LruCacheOptions>(new LruCacheOptions
        {
            MaxThreshold = 4
        });
        _testCache = new ConcurrentLruCache<int, string>(_mockOptions, _mockLogger, _mockRepo);
        
        //Act, Assert
        var value = _testCache.GetOrAdd(1);
        _mockRepo.Received(1).GetCacheItem(1);
        Assert.AreEqual("A", value);
        
        value = _testCache.GetOrAdd(2);
        _mockRepo.Received(1).GetCacheItem(2);
        Assert.AreEqual("B", value);
        
        value = _testCache.GetOrAdd(3);
        _mockRepo.Received(1).GetCacheItem(3);
        Assert.AreEqual("C", value);
        
        value = _testCache.GetOrAdd(4);
        _mockRepo.Received(1).GetCacheItem(4); 
        Assert.AreEqual("D", value);
        
        // Key 1 should be evicted here.
        value = _testCache.GetOrAdd(5);
        _mockRepo.Received(1).GetCacheItem(5); 
        Assert.AreEqual("E", value);
        
        //Second call to Db confirming key 1 was evicted.
        value = _testCache.GetOrAdd(1);
        _mockRepo.Received(2).GetCacheItem(1);
        Assert.AreEqual("A", value);
    }
    
    [Test]
    public void should_correctly_reorder_items_in_cache_after_use()
    {
        //Arrange
        _mockOptions = Microsoft.Extensions.Options.Options.Create<LruCacheOptions>(new LruCacheOptions
        {
            MaxThreshold = 4
        });
        _testCache = new ConcurrentLruCache<int, string>(_mockOptions, _mockLogger, _mockRepo);
        
        //Act, Assert
        var value = _testCache.GetOrAdd(1);
        _mockRepo.Received(1).GetCacheItem(1);
        Assert.AreEqual("A", value);
        
        value = _testCache.GetOrAdd(2);
        _mockRepo.Received(1).GetCacheItem(2);
        Assert.AreEqual("B", value);
        
        value = _testCache.GetOrAdd(3);
        _mockRepo.Received(1).GetCacheItem(3);
        Assert.AreEqual("C", value);
        
        //Re-call cached items, order should change.
        value = _testCache.GetOrAdd(1);
        _mockRepo.Received(1).GetCacheItem(1); 
        Assert.AreEqual("A", value);
        
        value = _testCache.GetOrAdd(2);
        _mockRepo.Received(1).GetCacheItem(2); 
        Assert.AreEqual("B", value);
        
        value = _testCache.GetOrAdd(4);
        _mockRepo.Received(1).GetCacheItem(4);
        Assert.AreEqual("D", value);
        
        // Key 3 should be evicted here as least recently used.
        value = _testCache.GetOrAdd(5);
        _mockRepo.Received(1).GetCacheItem(5);
        Assert.AreEqual("E", value);
        
        // Second call to the db for key 3 confirming it was evicted last round.
        value = _testCache.GetOrAdd(3);
        _mockRepo.Received(2).GetCacheItem(3);
        Assert.AreEqual("C", value);
    }
    
    [Test]
    public void invalidation_should_clear_cache()
    {
        //Arrange
        _mockOptions = Microsoft.Extensions.Options.Options.Create<LruCacheOptions>(new LruCacheOptions
        {
            MaxThreshold = 4
        });
        _testCache = new ConcurrentLruCache<int, string>(_mockOptions, _mockLogger, _mockRepo);

        _testCache.GetOrAdd(1);
        _testCache.GetOrAdd(2);

        //Act
        _testCache.Invalidate();
        
        //Assert
        _testCache.GetOrAdd(1);
        _testCache.GetOrAdd(2);
        
        //Second calls went to the Db again after invalidation.
        _mockRepo.Received(2).GetCacheItem(1);
        _mockRepo.Received(2).GetCacheItem(1);
        
    }
    
    [Test]
    public void should_return_default_value_if_repo_fails()
    {
        //Arrange
        _mockOptions = Microsoft.Extensions.Options.Options.Create<LruCacheOptions>(new LruCacheOptions
        {
            MaxThreshold = 4
        });
        _testCache = new ConcurrentLruCache<int, string>(_mockOptions, _mockLogger, _mockRepo);
        _mockRepo.GetCacheItem(4).Throws(new Exception("Data access failed"));

        _testCache.GetOrAdd(1);
        _testCache.GetOrAdd(2);

        //Act
        var value = _testCache.GetOrAdd(1);
        _mockRepo.Received(1).GetCacheItem(1);
        
        value = _testCache.GetOrAdd(2);
        _mockRepo.Received(1).GetCacheItem(2);

        var result = _testCache.GetOrAdd(4);

        
        //Assert
        Assert.AreEqual(default(string), result);
    }

    [Test]
    public void retrieval_from_cache_is_thread_safe()
    {
        //Arrange
        _mockOptions = Microsoft.Extensions.Options.Options.Create<LruCacheOptions>(new LruCacheOptions
        {
            MaxThreshold = 10
        });
        
        _testCache = new ConcurrentLruCache<int, string>(_mockOptions, _mockLogger, _mockRepo);
        
        var task1 = Task.Factory.StartNew(() => GetRangeFromCacheNTimes(_testCache, 1000, 10));
        var task2 = Task.Factory.StartNew(() => GetRangeFromCacheNTimes(_testCache, 1000, 10));
        
        //Act, Assert
        Assert.DoesNotThrow(() => Task.WaitAll(task1, task2));
        
        _mockRepo.Received(1).GetCacheItem(1);
        _mockRepo.Received(1).GetCacheItem(2);
        _mockRepo.Received(1).GetCacheItem(3);
        _mockRepo.Received(1).GetCacheItem(4);
        _mockRepo.Received(1).GetCacheItem(5);
        _mockRepo.Received(1).GetCacheItem(6);
        _mockRepo.Received(1).GetCacheItem(7);
        _mockRepo.Received(1).GetCacheItem(8);
        _mockRepo.Received(1).GetCacheItem(9);
        _mockRepo.Received(1).GetCacheItem(10);
    }
    
    [Test]
    public void eviction_from_cache_is_thread_safe()
    {
        //Arrange
        _mockOptions = Microsoft.Extensions.Options.Options.Create<LruCacheOptions>(new LruCacheOptions
        {
            MaxThreshold = 10
        });
        
        _testCache = new ConcurrentLruCache<int, string>(_mockOptions, _mockLogger, _mockRepo);
        
        var task1 = Task.Factory.StartNew(() => GetRangeFromCacheNTimes(_testCache, 1000, 11));
        var task2 = Task.Factory.StartNew(() => GetRangeFromCacheNTimes(_testCache, 1000, 11));
        
        //Act, Assert
        Assert.DoesNotThrow(() => Task.WaitAll(task1, task2));
    }

    private void GetRangeFromCacheNTimes(IConcurrentLruCache<int,string> cache, int calls, int range)
    {
        var keys = Enumerable.Range(1, range);
        var key = GetKey(keys).GetEnumerator();
        
        for(int i = 0; i < calls; i++)
        {
            if (key.Current == range)
            {
                key = GetKey(keys).GetEnumerator();
            }
            cache.GetOrAdd(key.Current + 1);
            key.MoveNext();
        }
    }

    private static IEnumerable<int> GetKey(IEnumerable<int> keys)
    {
        foreach (int key in keys)
        {
            yield return key;
        }
    }
}