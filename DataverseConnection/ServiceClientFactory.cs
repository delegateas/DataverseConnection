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
        private readonly DataverseOptions _defaultOptions;
        private readonly object _defaultCredentialGate = new();
        private TokenCredential? _defaultOptionsCredential;

        public ServiceClientFactory(
            IMemoryCache memoryCache,
            IConfiguration configuration,
            DataverseOptions? defaultOptions = null)
        {
            _memoryCache = memoryCache;
            _configuration = configuration;
            _defaultOptions = defaultOptions ?? new DataverseOptions();
        }

        public ServiceClient CreateClient(DataverseOptions? options = null)
        {
            var effectiveOptions = options ?? _defaultOptions;
            var dataverseUrl = Internal.ServiceClientBuilder.ResolveDataverseUrl(
                effectiveOptions,
                _configuration);
            var credential = options is null
                ? GetOrCreateDefaultCredential(dataverseUrl)
                : Internal.DataverseCredentialFactory.Create(options, dataverseUrl);

            return Internal.ServiceClientBuilder.Build(
                effectiveOptions,
                _memoryCache,
                _configuration,
                credential
            );
        }

        private TokenCredential GetOrCreateDefaultCredential(string dataverseUrl)
        {
            if (_defaultOptionsCredential is not null)
                return _defaultOptionsCredential;

            lock (_defaultCredentialGate)
            {
                return _defaultOptionsCredential ??=
                    Internal.DataverseCredentialFactory.Create(_defaultOptions, dataverseUrl);
            }
        }
    }
}
