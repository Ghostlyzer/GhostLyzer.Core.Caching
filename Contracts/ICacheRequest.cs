namespace GhostLyzer.Core.Caching.Contracts
{
    /// <summary>
    /// Defines a request for caching data.
    /// </summary>
    public interface ICacheRequest
    {
        /// <summary>
        /// Gets the key of the cache entry.
        /// </summary>
        string CacheKey { get; }

        /// <summary>
        /// Gets the absolute expiration time, relative to now.
        /// </summary>
        /// <value>
        /// The absolute expiration time, relative to now, or null if the cache entry does not expire.
        /// </value>
        DateTime? AbsoluteExpirationRelativeToNow { get; }
    }
}
