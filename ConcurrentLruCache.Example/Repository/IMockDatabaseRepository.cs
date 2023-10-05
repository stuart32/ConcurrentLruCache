using ConcurrentLruCache.Example.Model;

namespace ConcurrentLruCache.Example.Repository;

public interface IMockDatabaseRepository
{
    List<string> GetTransactionIds();
    
    Transaction GetTransaction(string transactionId);
}