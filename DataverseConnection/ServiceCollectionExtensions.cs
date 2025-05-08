using System;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.PowerPlatform.Dataverse.Client;

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
        public static IServiceCollection AddDataverse(this IServiceCollection services, Action<DataverseOptions> configureOptions)
        {
            if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));

            var options = new DataverseOptions();
            configureOptions(options);

            if (string.IsNullOrWhiteSpace(options.DataverseUrl))
                throw new ArgumentException("DataverseUrl must be provided in DataverseOptions.");

            services.AddSingleton<ServiceClient>(sp =>
            {
                var credential = options.TokenCredential ?? new DefaultAzureCredential();
                var resource = $"{new Uri(options.DataverseUrl).GetLeftPart(UriPartial.Authority)}/.default";

                // Token provider function for ServiceClient
                async Task<string> TokenProvider(string url)
                {
                    var tokenRequestContext = new TokenRequestContext(new[] { resource });
                    var token = await credential.GetTokenAsync(tokenRequestContext, default);
                    return token.Token;
                }

                var serviceClient = new ServiceClient(
                    new Uri(options.DataverseUrl),
                    tokenProviderFunction: TokenProvider);

                if (!serviceClient.IsReady)
                    throw new InvalidOperationException("ServiceClient is not ready. Check your credentials and Dataverse URL.");

                return serviceClient;
            });

            return services;
        }
    }
}
