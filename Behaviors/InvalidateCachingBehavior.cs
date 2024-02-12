using EasyCaching.Core;
using GhostLyzer.Core.Caching.Contracts;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GhostLyzer.Core.Caching.Behaviors
{
    /// <summary>
    /// Provides cache invalidation behavior for requests in a pipeline.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public class InvalidateCachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull, IRequest<TResponse> where TResponse : notnull
    {
        private readonly ILogger<InvalidateCachingBehavior<TRequest, TResponse>> _logger;
        private readonly IEasyCachingProvider _cachingProvider;
        private readonly IInvalidateCacheRequest _invalidateCacheRequest;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidateCachingBehavior{TRequest, TResponse}"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cachingFactory">The caching factory.</param>
        /// <param name="invalidateCacheRequest">The invalidate cache request.</param>
        public InvalidateCachingBehavior(
            ILogger<InvalidateCachingBehavior<TRequest, TResponse>> logger,
            IEasyCachingProviderFactory cachingFactory,
            IInvalidateCacheRequest invalidateCacheRequest)
        {
            _logger = logger;
            _cachingProvider = cachingFactory.GetCachingProvider("mem");
            _invalidateCacheRequest = invalidateCacheRequest;
        }

        /// <summary>
        /// Handles the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="next">The next delegate in the pipeline.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResponse}"/> representing the result of the asynchronous operation.</returns>
        /// <remarks>
        /// If the request implements <see cref="IInvalidateCacheRequest"/>, the cache entry with the key specified by the request is invalidated.
        /// Otherwise, the request is handled by the next delegate in the pipeline.
        /// </remarks>
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (request is not IInvalidateCacheRequest || _invalidateCacheRequest == null)
            {
                // No cache request was found. Let's continue executing the pipeline
                return await next();
            }

            var cacheKey = _invalidateCacheRequest.CacheKey;
            var response = await next();

            await _cachingProvider.RemoveAsync(cacheKey);

            _logger.LogDebug("Cache data with cache key: {CacheKey} has been removed.", cacheKey);

            return response;
        }
    }
}
