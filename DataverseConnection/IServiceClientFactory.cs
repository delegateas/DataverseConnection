using Microsoft.PowerPlatform.Dataverse.Client;

namespace DataverseConnection
{
    /// <summary>
    /// Factory interface for creating new instances of <see cref="ServiceClient"/>.
    /// Use for advanced scenarios such as long-running jobs or when multiple independent clients are required.
    /// </summary>
    public interface IServiceClientFactory
    {
        /// <summary>
        /// Creates a new <see cref="ServiceClient"/> instance.
        /// </summary>
        /// <param name="options">
        /// Optional Dataverse connection options. If null, uses default configuration.
        /// </param>
        /// <returns>A new <see cref="ServiceClient"/> instance.</returns>
        ServiceClient CreateClient(DataverseOptions? options = null);
    }
}
