using Azure.Core;
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
        private readonly TokenCredential _defaultOptionsCredential;
        private readonly DataverseOptions _defaultOptions;

        public ServiceClientFactory(
            IMemoryCache memoryCache,
            IConfiguration configuration,
            DataverseOptions? defaultOptions = null)
        {
            _memoryCache = memoryCache;
            _configuration = configuration;
            _defaultOptions = defaultOptions ?? new DataverseOptions();
            _defaultOptionsCredential = Internal.DataverseCredentialFactory.Create(_defaultOptions);
        }

        public ServiceClient CreateClient(DataverseOptions? options = null)
        {
            var effectiveOptions = options ?? _defaultOptions;
            var credential = options is null
                ? _defaultOptionsCredential
                : Internal.DataverseCredentialFactory.Create(options);

            return Internal.ServiceClientBuilder.Build(
                effectiveOptions,
                _memoryCache,
                _configuration,
                credential
            );
        }
    }
}
