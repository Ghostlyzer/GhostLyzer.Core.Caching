namespace GhostLyzer.Core.Caching.Contracts
{
    /// <summary>
    /// Defines a request for invalidating a cache entry.
    /// </summary>
    public interface IInvalidateCacheRequest
    {
        /// <summary>
        /// Gets the key of the cache entry to invalidate.
        /// </summary>
        string CacheKey { get; }
    }
}
