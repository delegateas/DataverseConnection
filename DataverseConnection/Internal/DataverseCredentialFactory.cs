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
        public static TokenCredential Create(DataverseOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            if (options.TokenCredential is not null)
                return options.TokenCredential;

            return options.CredentialType switch
            {
                DataverseCredentialType.AzureCliCredential =>
                    options.AzureCliCredentialOptions is null
                        ? new AzureCliCredential()
                        : new AzureCliCredential(options.AzureCliCredentialOptions),

                // Interactive credentials use persistent token caching by default so the user logs in
                // as rarely as possible. Caller-supplied options are respected as-is.
                DataverseCredentialType.DeviceCodeCredential =>
                    options.DeviceCodeCredentialOptions is null
                        ? PersistentCredentialCache.CreateDeviceCode()
                        : new DeviceCodeCredential(options.DeviceCodeCredentialOptions),

                DataverseCredentialType.InteractiveBrowserCredential =>
                    options.InteractiveBrowserCredentialOptions is null
                        ? PersistentCredentialCache.CreateInteractiveBrowser()
                        : new InteractiveBrowserCredential(options.InteractiveBrowserCredentialOptions),

                _ => throw new ArgumentOutOfRangeException(
                    nameof(options.CredentialType),
                    options.CredentialType,
                    "Unsupported Dataverse credential type.")
            };
        }
    }
}
