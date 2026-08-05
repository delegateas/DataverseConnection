using System;
using System.Runtime.CompilerServices;
using System.Threading;
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
        private static readonly ConditionalWeakTable<TokenCredential, CredentialCacheIdentity> CredentialCacheIdentities = new();
        private static long _nextCredentialCacheIdentity;

        public static ServiceClient Build(
            DataverseOptions options,
            IMemoryCache memoryCache,
            IConfiguration? configuration,
            TokenCredential credential)
        {
            string? dataverseUrl = options.DataverseUrl;
            if (string.IsNullOrWhiteSpace(dataverseUrl))
            {
                dataverseUrl = configuration?["DATAVERSE_URL"];
            }

            if (string.IsNullOrWhiteSpace(dataverseUrl))
                throw new InvalidOperationException("DataverseUrl must be provided via options or configuration (DATAVERSE_URL).");

            var credentialCacheIdentity = CredentialCacheIdentities.GetValue(
                credential,
                _ => new CredentialCacheIdentity(Interlocked.Increment(ref _nextCredentialCacheIdentity)));
            var resource = $"{new Uri(dataverseUrl).GetLeftPart(UriPartial.Authority)}/.default";

            async Task<string> TokenProvider(string url)
            {
                var cacheKey = $"dataverse_token_{credentialCacheIdentity.Value}_{resource}";
                if (memoryCache.TryGetValue<string>(cacheKey, out var cachedToken) && cachedToken != null)
                    return cachedToken;

                var tokenRequestContext = new TokenRequestContext([resource]);
                var token = await credential.GetTokenAsync(tokenRequestContext, default);

                var expiresOn = token.ExpiresOn.UtcDateTime;
                var now = DateTime.UtcNow;
                var expiration = expiresOn - TimeSpan.FromMinutes(5);
                if (expiration <= now)
                {
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

        private sealed record CredentialCacheIdentity(long Value);
    }
}
