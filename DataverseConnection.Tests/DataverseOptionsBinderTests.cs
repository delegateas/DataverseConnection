using DataverseConnection.Internal;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace DataverseConnection.Tests;

public class DataverseOptionsBinderTests
{
    private static IConfiguration Config(params (string Key, string Value)[] values)
    {
        var dict = new Dictionary<string, string?>();
        foreach (var (key, value) in values)
            dict[key] = value;

        return new ConfigurationBuilder()
            .AddInMemoryCollection(dict)
            .Build();
    }

    [Fact]
    public void Bind_ReadsUrlFromConfiguration()
    {
        var options = new DataverseOptions();

        DataverseOptionsBinder.Bind(options, Config(("DATAVERSE_URL", "https://org.crm4.dynamics.com")));

        Assert.Equal("https://org.crm4.dynamics.com", options.DataverseUrl);
    }

    [Theory]
    [InlineData("azcli", DataverseCredentialType.AzureCliCredential)]
    [InlineData("devicecode", DataverseCredentialType.DeviceCodeCredential)]
    [InlineData("browser", DataverseCredentialType.InteractiveBrowserCredential)]
    [InlineData("AZCLI", DataverseCredentialType.AzureCliCredential)]
    [InlineData("  DeviceCode  ", DataverseCredentialType.DeviceCodeCredential)]
    public void Bind_ParsesCredentialType_CaseInsensitively(string configured, DataverseCredentialType expected)
    {
        var options = new DataverseOptions();

        DataverseOptionsBinder.Bind(options, Config(("DATAVERSE_CREDENTIAL_TYPE", configured)));

        Assert.Equal(expected, options.CredentialType);
    }

    [Fact]
    public void Bind_DoesNotOverwriteExplicitUrl()
    {
        var options = new DataverseOptions { DataverseUrl = "https://explicit.crm4.dynamics.com" };

        DataverseOptionsBinder.Bind(options, Config(("DATAVERSE_URL", "https://config.crm4.dynamics.com")));

        Assert.Equal("https://explicit.crm4.dynamics.com", options.DataverseUrl);
    }

    [Fact]
    public void Bind_LeavesDefaultCredentialType_WhenKeyAbsent()
    {
        var options = new DataverseOptions();

        DataverseOptionsBinder.Bind(options, Config());

        Assert.Equal(DataverseCredentialType.InteractiveBrowserCredential, options.CredentialType);
    }

    [Fact]
    public void Bind_ThrowsForUnknownCredentialType()
    {
        var options = new DataverseOptions();

        Assert.Throws<ArgumentException>(
            () => DataverseOptionsBinder.Bind(options, Config(("DATAVERSE_CREDENTIAL_TYPE", "bogus"))));
    }
}
