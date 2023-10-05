using System.Globalization;
using ConcurrentLruCache.Example.Model;
using ConcurrentLRUCache.Interface;
using CsvHelper;

namespace ConcurrentLruCache.Example.Repository;

public class CsvTransactionRepository : ILruCachRepository<string, Transaction>, IMockDatabaseRepository
{
    private List<Transaction> _transactions;
    
    public CsvTransactionRepository()
    {
        InitialiseDb();
    }

    List<string> IMockDatabaseRepository.GetTransactionIds()
    {
        return _transactions.Select(x => x.transaction_id).ToList();
    }

    public Transaction GetTransaction(string transactionId)
    {
        Thread.Sleep(500);

        return _transactions.SingleOrDefault(t => t.transaction_id == transactionId);
    }

    public Transaction GetCacheItem(string key)
    {
        return GetTransaction(key);
    }

    private void InitialiseDb()
    {
        try
        {
            using (var reader = new StreamReader("./Data/taxlot_accounting_transactions.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                _transactions = csv.GetRecords<Transaction>().ToList();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}