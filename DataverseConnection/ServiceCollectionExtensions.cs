using System;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Xrm.Sdk;

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
                var memoryCache = sp.GetRequiredService<IMemoryCache>();
                var configuration = sp.GetService<IConfiguration>();
                var defaultCredential = new DefaultAzureCredential();
                return Internal.ServiceClientBuilder.Build(
                    options,
                    memoryCache,
                    configuration,
                    defaultCredential
                );
            });

            return services;
        }

        /// <summary>
        /// Registers a ServiceClientFactory for advanced scenarios where new ServiceClient instances are required.
        /// Existing ServiceClient and interface registrations remain unchanged.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configureOptions">Optional action to configure default DataverseOptions for the factory.</param>
        /// <param name="defaultCredential">Optional default TokenCredential for the factory.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddDataverseFactory(
            this IServiceCollection services,
            Action<DataverseOptions>? configureOptions = null,
            TokenCredential? defaultCredential = null)
        {
            var options = new DataverseOptions();
            configureOptions?.Invoke(options);

            services.AddMemoryCache();

            services.AddSingleton<IServiceClientFactory>(sp =>
            {
                var memoryCache = sp.GetRequiredService<IMemoryCache>();
                var configuration = sp.GetRequiredService<IConfiguration>();
                return new ServiceClientFactory(
                    memoryCache,
                    configuration,
                    defaultCredential,
                    options
                );
            });

            return services;
        }

        /// <summary>
        /// Registers a Dataverse ServiceClient and all related organization service interfaces for dependency injection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configureOptions">Action to configure DataverseOptions.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddDataverseWithOrganizationServices(this IServiceCollection services, Action<DataverseOptions>? configureOptions = null)
        {
            services.AddDataverse(configureOptions);
            services.AddSingleton<IOrganizationServiceAsync2>(sp => sp.GetRequiredService<ServiceClient>());
            services.AddSingleton<IOrganizationServiceAsync>(sp => sp.GetRequiredService<ServiceClient>());
            services.AddSingleton<IOrganizationService>(sp => sp.GetRequiredService<ServiceClient>());
            return services;
        }
    }
}
