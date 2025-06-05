using System;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;

namespace DataverseConnection
{
    /// <summary>
    /// Options for configuring Dataverse connection.
    /// </summary>
    public class DataverseOptions
    {
        /// <summary>
        /// The Dataverse environment URL (e.g., https://org.crm4.dynamics.com).
        /// </summary>
        public string DataverseUrl { get; set; } = string.Empty;

        /// <summary>
        /// The Azure TokenCredential to use. If not set, DefaultAzureCredential is used.
        /// </summary>
        public TokenCredential? TokenCredential { get; set; }
    }

    /// <summary>
    /// Extension methods for IServiceCollection to add Dataverse ServiceClient.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers a Dataverse ServiceClient for dependency injection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configureOptions">Action to configure DataverseOptions.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddDataverse(this IServiceCollection services, Action<DataverseOptions>? configureOptions = null)
        {
            var options = new DataverseOptions();
            configureOptions?.Invoke(options);

            // Ensure MemoryCache is registered
            services.AddMemoryCache();

            services.AddSingleton(sp =>
            {
                // Determine DataverseUrl: options first, then configuration
                string? dataverseUrl = options.DataverseUrl;
                if (string.IsNullOrWhiteSpace(dataverseUrl))
                {
                    var config = sp.GetService<IConfiguration>();
                    dataverseUrl = config?["DATAVERSE_URL"];
                }

                if (string.IsNullOrWhiteSpace(dataverseUrl))
                    throw new InvalidOperationException("DataverseUrl must be provided via options or configuration (DATAVERSE_URL).");

                var credential = options.TokenCredential ?? new DefaultAzureCredential();
                var resource = $"{new Uri(dataverseUrl).GetLeftPart(UriPartial.Authority)}/.default";

                var memoryCache = sp.GetRequiredService<IMemoryCache>();

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
            });

            return services;
        }
    }
}
