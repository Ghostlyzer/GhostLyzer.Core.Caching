using EasyCaching.Core;
using GhostLyzer.Core.Caching.Contracts;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GhostLyzer.Core.Caching.Behaviors
{
    /// <summary>
    /// Provides caching behavior for requests in a pipeline.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull, IRequest<TResponse> where TResponse : notnull
    {
        private readonly ICacheRequest _cacheRequest;
        private readonly IEasyCachingProvider _cachingProvider;
        private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;
        private readonly int defaultCacheExpirationInHours = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="CachingBehavior{TRequest, TResponse}"/> class.
        /// </summary>
        /// <param name="cacheRequest">The cache request.</param>
        /// <param name="cachingFactory">The caching factory.</param>
        public CachingBehavior(
            ICacheRequest cacheRequest,
            IEasyCachingProviderFactory cachingFactory,
            ILogger<CachingBehavior<TRequest, TResponse>> logger)
        {
            _cacheRequest = cacheRequest;
            _cachingProvider = cachingFactory.GetCachingProvider("mem");
            _logger = logger;
        }

        /// <summary>
        /// Handles the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="next">The next delegate in the pipeline.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResponse}"/> representing the result of the asynchronous operation.</returns>
        /// <remarks>
        /// If the request implements <see cref="ICacheRequest"/> and a cached response exists, the cached response is returned.
        /// Otherwise, the request is handled by the next delegate in the pipeline and the response is cached for future use.
        /// </remarks>
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (request is not ICacheRequest || _cacheRequest == null) 
            {
                // No cache request was found. Let's just continue executing the pipeline
                return await next();
            }

            var cacheKey = _cacheRequest.CacheKey;
            var cachedResponse = await _cachingProvider.GetAsync<TResponse>(cacheKey, cancellationToken);

            if (cachedResponse.Value != null)
            {
                _logger.LogDebug("Response retrieved {TRequest} from cache. Cache Key: {CacheKey}",
                    typeof(TRequest).FullName, cacheKey);
                return cachedResponse.Value;
            }

            var response = await next();

            var expirationTime = _cacheRequest.AbsoluteExpirationRelativeToNow ??
                DateTime.Now.AddHours(defaultCacheExpirationInHours);

            await _cachingProvider.SetAsync(cacheKey, response, expirationTime.TimeOfDay);

            _logger.LogDebug("Caching response for {TRequest} with Cache Key: {CacheKey}.",
                typeof(TRequest).FullName, cacheKey);

            return response;
        }
    }
}
