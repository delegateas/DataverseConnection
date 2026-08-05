using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace DataverseConnection
{
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
            services.AddMemoryCache();

            services.AddSingleton(sp =>
            {
                var memoryCache = sp.GetRequiredService<IMemoryCache>();
                var configuration = sp.GetService<IConfiguration>();

                // Defaults come from application settings; the configureOptions override wins last.
                var options = new DataverseOptions();
                if (configuration is not null)
                    Internal.DataverseOptionsBinder.Bind(options, configuration);
                configureOptions?.Invoke(options);

                var credential = Internal.DataverseCredentialFactory.Create(options);
                return Internal.ServiceClientBuilder.Build(
                    options,
                    memoryCache,
                    configuration,
                    credential
                );
            });

            return services;
        }

        /// <summary>
        /// Registers a ServiceClientFactory for advanced scenarios where new ServiceClient instances are required.
        /// Existing ServiceClient and interface registrations remain unchanged.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configureOptions">
        /// Optional action to configure default DataverseOptions for the factory. To use a custom
        /// credential, set <see cref="DataverseOptions.TokenCredential"/> here.
        /// </param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddDataverseFactory(
            this IServiceCollection services,
            Action<DataverseOptions>? configureOptions = null)
        {
            services.AddMemoryCache();

            services.AddSingleton<IServiceClientFactory>(sp =>
            {
                var memoryCache = sp.GetRequiredService<IMemoryCache>();
                var configuration = sp.GetRequiredService<IConfiguration>();

                // Defaults come from application settings; the configureOptions override wins last.
                var options = new DataverseOptions();
                Internal.DataverseOptionsBinder.Bind(options, configuration);
                configureOptions?.Invoke(options);

                return new ServiceClientFactory(
                    memoryCache,
                    configuration,
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
