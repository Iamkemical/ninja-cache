namespace NinjaCache;

public class CacheEntry<T>
{
    public T Value { get; set; }
    public DateTimeOffset? ExpiryTime { get; set; }

    public bool IsExpired()
    {
        return ExpiryTime.HasValue && DateTimeOffset.UtcNow > ExpiryTime;
    }
}