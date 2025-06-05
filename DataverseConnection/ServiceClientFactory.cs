using System;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace DataverseConnection
{
    /// <summary>
    /// Factory for creating new instances of <see cref="ServiceClient"/>.
    /// Use for advanced scenarios such as long-running jobs or when multiple independent clients are required.
    /// </summary>
    public class ServiceClientFactory : IServiceClientFactory
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IConfiguration _configuration;
        private readonly TokenCredential _defaultCredential;
        private readonly DataverseOptions _defaultOptions;

        public ServiceClientFactory(
            IMemoryCache memoryCache,
            IConfiguration configuration,
            TokenCredential? defaultCredential = null,
            DataverseOptions? defaultOptions = null)
        {
            _memoryCache = memoryCache;
            _configuration = configuration;
            _defaultCredential = defaultCredential ?? new DefaultAzureCredential();
            _defaultOptions = defaultOptions ?? new DataverseOptions();
        }

        public ServiceClient CreateClient(DataverseOptions? options = null)
        {
            var effectiveOptions = options ?? _defaultOptions;

            return Internal.ServiceClientBuilder.Build(
                effectiveOptions,
                _memoryCache,
                _configuration,
                _defaultCredential
            );
        }
    }
}
