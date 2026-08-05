using System;
using Microsoft.Extensions.Configuration;

namespace DataverseConnection.Internal
{
    /// <summary>
    /// Applies the library's default "translation" from application settings onto a
    /// <see cref="DataverseOptions"/> instance. Only fills values the caller has not already
    /// set, so explicit properties (or a <c>configureOptions</c> override) always win.
    /// </summary>
    internal static class DataverseOptionsBinder
    {
        /// <summary>
        /// Reads the flat configuration keys <c>DATAVERSE_URL</c> and
        /// <c>DATAVERSE_CREDENTIAL_TYPE</c> and applies them to <paramref name="options"/>.
        /// </summary>
        public static void Bind(DataverseOptions options, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(configuration);

            if (string.IsNullOrWhiteSpace(options.DataverseUrl))
            {
                var url = configuration["DATAVERSE_URL"];
                if (!string.IsNullOrWhiteSpace(url))
                    options.DataverseUrl = url;
            }

            var credentialType = configuration["DATAVERSE_CREDENTIAL_TYPE"];
            if (!string.IsNullOrWhiteSpace(credentialType))
            {
                options.CredentialType = credentialType.Trim().ToLowerInvariant() switch
                {
                    "browser" => DataverseCredentialType.InteractiveBrowserCredential,
                    "devicecode" => DataverseCredentialType.DeviceCodeCredential,
                    "azcli" => DataverseCredentialType.AzureCliCredential,
                    _ => throw new ArgumentException(
                        $"Unknown DATAVERSE_CREDENTIAL_TYPE '{credentialType}'. Valid values: browser, devicecode, azcli.")
                };
            }
        }
    }
}
