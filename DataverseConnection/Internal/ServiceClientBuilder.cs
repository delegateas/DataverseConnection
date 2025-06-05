using System;
using System.Threading.Tasks;
using Azure.Core;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace DataverseConnection.Internal
{
    /// <summary>
    /// Internal helper for constructing ServiceClient instances with consistent logic.
    /// </summary>
    internal static class ServiceClientBuilder
    {
        public static ServiceClient Build(
            DataverseOptions options,
            IMemoryCache memoryCache,
            IConfiguration? configuration,
            TokenCredential defaultCredential)
        {
            // Determine DataverseUrl: options first, then configuration
            string? dataverseUrl = options.DataverseUrl;
            if (string.IsNullOrWhiteSpace(dataverseUrl))
            {
                dataverseUrl = configuration?["DATAVERSE_URL"];
            }

            if (string.IsNullOrWhiteSpace(dataverseUrl))
                throw new InvalidOperationException("DataverseUrl must be provided via options or configuration (DATAVERSE_URL).");

            var credential = options.TokenCredential ?? defaultCredential;
            var resource = $"{new Uri(dataverseUrl).GetLeftPart(UriPartial.Authority)}/.default";

            // Token provider function for ServiceClient
            async Task<string> TokenProvider(string url)
            {
                var cacheKey = $"dataverse_token_{resource}";
                if (memoryCache.TryGetValue<string>(cacheKey, out var cachedToken) && cachedToken != null)
                    return cachedToken;

                var tokenRequestContext = new TokenRequestContext([resource]);
                var token = await credential.GetTokenAsync(tokenRequestContext, default);

                // Set expiration 5 minutes before actual expiry, but never in the past
                var expiresOn = token.ExpiresOn.UtcDateTime;
                var now = DateTime.UtcNow;
                var expiration = expiresOn - TimeSpan.FromMinutes(5);
                if (expiration <= now)
                {
                    // If token lifetime is less than 5 minutes, expire 1 minute before, or immediately if needed
                    expiration = expiresOn > now.AddMinutes(1) ? expiresOn - TimeSpan.FromMinutes(1) : now.AddSeconds(10);
                }

                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = expiration
                };
                memoryCache.Set(cacheKey, token.Token, cacheEntryOptions);

                return token.Token;
            }

            var serviceClient = new ServiceClient(
                new Uri(dataverseUrl),
                tokenProviderFunction: TokenProvider);

            if (!serviceClient.IsReady)
                throw new InvalidOperationException("ServiceClient is not ready. Check your credentials and Dataverse URL.");

            return serviceClient;
        }
    }
}
