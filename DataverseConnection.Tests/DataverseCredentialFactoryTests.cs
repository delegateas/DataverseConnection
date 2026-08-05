using Azure.Core;
using Azure.Identity;
using DataverseConnection.Internal;
using Xunit;

namespace DataverseConnection.Tests;

public class DataverseCredentialFactoryTests
{
    [Theory]
    [InlineData(DataverseCredentialType.DefaultAzureCredential, typeof(DefaultAzureCredential))]
    [InlineData(DataverseCredentialType.AzureCliCredential, typeof(AzureCliCredential))]
    [InlineData(DataverseCredentialType.DeviceCodeCredential, typeof(DeviceCodeCredential))]
    [InlineData(DataverseCredentialType.InteractiveBrowserCredential, typeof(InteractiveBrowserCredential))]
    public void Create_ReturnsSelectedCredentialType(
        DataverseCredentialType credentialType,
        Type expectedType)
    {
        var options = new DataverseOptions
        {
            CredentialType = credentialType
        };

        var credential = DataverseCredentialFactory.Create(options);

        Assert.IsType(expectedType, credential);
    }

    [Fact]
    public void Create_UsesCredentialSpecificOptions()
    {
        var defaultCredential = DataverseCredentialFactory.Create(new DataverseOptions
        {
            CredentialType = DataverseCredentialType.DefaultAzureCredential,
            DefaultAzureCredentialOptions = new DefaultAzureCredentialOptions()
        });
        var azureCliCredential = DataverseCredentialFactory.Create(new DataverseOptions
        {
            CredentialType = DataverseCredentialType.AzureCliCredential,
            AzureCliCredentialOptions = new AzureCliCredentialOptions()
        });
        var deviceCodeCredential = DataverseCredentialFactory.Create(new DataverseOptions
        {
            CredentialType = DataverseCredentialType.DeviceCodeCredential,
            DeviceCodeCredentialOptions = new DeviceCodeCredentialOptions()
        });
        var interactiveBrowserCredential = DataverseCredentialFactory.Create(new DataverseOptions
        {
            CredentialType = DataverseCredentialType.InteractiveBrowserCredential,
            InteractiveBrowserCredentialOptions = new InteractiveBrowserCredentialOptions()
        });

        Assert.IsType<DefaultAzureCredential>(defaultCredential);
        Assert.IsType<AzureCliCredential>(azureCliCredential);
        Assert.IsType<DeviceCodeCredential>(deviceCodeCredential);
        Assert.IsType<InteractiveBrowserCredential>(interactiveBrowserCredential);
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
    public void Create_UsesProvidedDefaultCredentialForDefaultSelection()
    {
        var providedDefaultCredential = new TestTokenCredential();
        var options = new DataverseOptions
        {
            CredentialType = DataverseCredentialType.DefaultAzureCredential
        };

        var credential = DataverseCredentialFactory.Create(options, providedDefaultCredential);

        Assert.Same(providedDefaultCredential, credential);
    }

    [Fact]
    public void Create_DoesNotUseProvidedDefaultCredentialForForcedSelection()
    {
        var providedDefaultCredential = new TestTokenCredential();
        var options = new DataverseOptions
        {
            CredentialType = DataverseCredentialType.DeviceCodeCredential
        };

        var credential = DataverseCredentialFactory.Create(options, providedDefaultCredential);

        Assert.IsType<DeviceCodeCredential>(credential);
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
