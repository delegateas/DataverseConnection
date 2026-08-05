using System;
using Azure.Core;
using Azure.Identity;

namespace DataverseConnection.Internal
{
    /// <summary>
    /// Creates the exact Azure Identity credential requested by <see cref="DataverseOptions"/>.
    /// </summary>
    internal static class DataverseCredentialFactory
    {
        public static TokenCredential Create(
            DataverseOptions options,
            TokenCredential? defaultCredential = null)
        {
            ArgumentNullException.ThrowIfNull(options);

            if (options.TokenCredential is not null)
                return options.TokenCredential;

            return options.CredentialType switch
            {
                DataverseCredentialType.DefaultAzureCredential =>
                    defaultCredential ?? CreateDefaultAzureCredential(options.DefaultAzureCredentialOptions),

                DataverseCredentialType.AzureCliCredential =>
                    options.AzureCliCredentialOptions is null
                        ? new AzureCliCredential()
                        : new AzureCliCredential(options.AzureCliCredentialOptions),

                DataverseCredentialType.DeviceCodeCredential =>
                    options.DeviceCodeCredentialOptions is null
                        ? new DeviceCodeCredential()
                        : new DeviceCodeCredential(options.DeviceCodeCredentialOptions),

                DataverseCredentialType.InteractiveBrowserCredential =>
                    options.InteractiveBrowserCredentialOptions is null
                        ? new InteractiveBrowserCredential()
                        : new InteractiveBrowserCredential(options.InteractiveBrowserCredentialOptions),

                _ => throw new ArgumentOutOfRangeException(
                    nameof(options.CredentialType),
                    options.CredentialType,
                    "Unsupported Dataverse credential type.")
            };
        }

        private static DefaultAzureCredential CreateDefaultAzureCredential(
            DefaultAzureCredentialOptions? options)
        {
            return options is null
                ? new DefaultAzureCredential()
                : new DefaultAzureCredential(options);
        }
    }
}
