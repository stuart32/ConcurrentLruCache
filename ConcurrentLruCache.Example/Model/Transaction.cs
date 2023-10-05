namespace ConcurrentLruCache.Example.Model;

public class Transaction
{
    public string transaction_id { get; set; }
    
    public string type { get; set; }
    
    public string units { get; set; }
    
    public string transaction_price { get; set; }
    
    public string transaction_date { get; set; }
    
    public string settlement_date { get; set; }
    
    public string total_consideration { get; set; }
    
    public string client_internal { get; set; }
    
    public string ticker { get; set; }
    
    public string sleeve { get; set; }
    
    public string trade_to_portfolio_rate { get; set; }
}