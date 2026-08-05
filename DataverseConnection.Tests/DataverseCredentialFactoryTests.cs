using Azure.Core;
using Azure.Identity;
using DataverseConnection.Internal;
using Xunit;

namespace DataverseConnection.Tests;

public class DataverseCredentialFactoryTests
{
    [Fact]
    public void DefaultCredentialType_IsInteractiveBrowser()
    {
        Assert.Equal(
            DataverseCredentialType.InteractiveBrowserCredential,
            new DataverseOptions().CredentialType);
    }

    [Fact]
    public void Create_ReturnsAzureCliCredential()
    {
        var options = new DataverseOptions
        {
            CredentialType = DataverseCredentialType.AzureCliCredential
        };

        var credential = DataverseCredentialFactory.Create(options);

        Assert.IsType<AzureCliCredential>(credential);
    }

    [Fact]
    public void Create_ReturnsDeviceCodeCredential_WhenCallerSuppliesOptions()
    {
        var options = new DataverseOptions
        {
            CredentialType = DataverseCredentialType.DeviceCodeCredential,
            DeviceCodeCredentialOptions = new DeviceCodeCredentialOptions()
        };

        var credential = DataverseCredentialFactory.Create(options);

        Assert.IsType<DeviceCodeCredential>(credential);
    }

    [Fact]
    public void Create_ReturnsInteractiveBrowserCredential_WhenCallerSuppliesOptions()
    {
        var options = new DataverseOptions
        {
            CredentialType = DataverseCredentialType.InteractiveBrowserCredential,
            InteractiveBrowserCredentialOptions = new InteractiveBrowserCredentialOptions()
        };

        var credential = DataverseCredentialFactory.Create(options);

        Assert.IsType<InteractiveBrowserCredential>(credential);
    }

    [Theory]
    [InlineData(DataverseCredentialType.DeviceCodeCredential)]
    [InlineData(DataverseCredentialType.InteractiveBrowserCredential)]
    public void Create_WrapsInteractiveCredentials_WithPersistentCache_ByDefault(
        DataverseCredentialType credentialType)
    {
        var options = new DataverseOptions
        {
            CredentialType = credentialType
        };

        var credential = DataverseCredentialFactory.Create(options);

        // The interactive credentials are wrapped so tokens persist across runs by default.
        Assert.IsType<PersistentAuthCredential>(credential);
    }

    [Fact]
    public void Create_UsesExplicitTokenCredentialBeforeSelectedType()
    {
        var explicitCredential = new TestTokenCredential();
        var options = new DataverseOptions
        {
            CredentialType = DataverseCredentialType.AzureCliCredential,
            TokenCredential = explicitCredential
        };

        var credential = DataverseCredentialFactory.Create(options);

        Assert.Same(explicitCredential, credential);
    }

    [Fact]
    public void Create_ThrowsForUnsupportedCredentialType()
    {
        var options = new DataverseOptions
        {
            CredentialType = (DataverseCredentialType)999
        };

        Assert.Throws<ArgumentOutOfRangeException>(() => DataverseCredentialFactory.Create(options));
    }

    private sealed class TestTokenCredential : TokenCredential
    {
        public override AccessToken GetToken(
            TokenRequestContext requestContext,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public override ValueTask<AccessToken> GetTokenAsync(
            TokenRequestContext requestContext,
            CancellationToken cancellationToken) => throw new NotSupportedException();
    }
}
