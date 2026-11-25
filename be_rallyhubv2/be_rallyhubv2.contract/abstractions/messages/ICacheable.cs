namespace be_rallyhubv2.contract.abstractions.messages;

public interface ICacheable
{
    public string CacheKey { get; }
    public int CacheMinutes { get; }
}