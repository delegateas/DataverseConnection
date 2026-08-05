# DataverseConnection (.NET NuGet Package)

## Overview

**DataverseConnection** is a .NET 8 class library that provides reusable, dependency-injectable connection logic for Microsoft Dataverse. It supports an explicitly provided `TokenCredential` and selectable Azure Identity credential types.

The included `DataverseWhoAmI` console application demonstrates the library and verifies connectivity.

## Features

- Reusable .NET 8 library for Dataverse connectivity
- Dependency injection through `AddDataverse`, `AddDataverseWithOrganizationServices`, and `AddDataverseFactory`
- Selectable Azure Identity authentication:
  - `DefaultAzureCredential`
  - `AzureCliCredential`
  - `DeviceCodeCredential`
  - `InteractiveBrowserCredential`
  - Any explicitly supplied `TokenCredential`
- Credential-specific Azure Identity options
- Token caching isolated by credential instance and Dataverse resource
- Cross-platform support for Windows, Linux, and macOS

## Prerequisites

- .NET 8 SDK
- Access to a Microsoft Dataverse environment
- An Azure identity with access to that environment

## Basic registration

```csharp
using DataverseConnection;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddDataverse(options =>
{
    // Optional when DATAVERSE_URL is available through IConfiguration.
    options.DataverseUrl = "https://yourorg.crm4.dynamics.com";
});
```

If no credential type or custom credential is specified, the library preserves its previous behavior and uses `DefaultAzureCredential`.

## Selecting a credential type

Set `DataverseOptions.CredentialType` to force the connection to use one Azure Identity credential implementation. Selecting a type does not create a fallback chain.

### Azure CLI only

This requires an authenticated Azure CLI session, normally created with `az login`.

```csharp
services.AddDataverse(options =>
{
    options.DataverseUrl = "https://yourorg.crm4.dynamics.com";
    options.CredentialType = DataverseCredentialType.AzureCliCredential;
});
```

You can provide Azure CLI-specific options:

```csharp
using Azure.Identity;

services.AddDataverse(options =>
{
    options.CredentialType = DataverseCredentialType.AzureCliCredential;
    options.AzureCliCredentialOptions = new AzureCliCredentialOptions
    {
        TenantId = "<tenant-id>"
    };
});
```

### Device code only

```csharp
using Azure.Identity;

services.AddDataverse(options =>
{
    options.DataverseUrl = "https://yourorg.crm4.dynamics.com";
    options.CredentialType = DataverseCredentialType.DeviceCodeCredential;
    options.DeviceCodeCredentialOptions = new DeviceCodeCredentialOptions
    {
        TenantId = "<tenant-id>",
        ClientId = "<application-client-id>"
    };
});
```

The device-code credential presents instructions that let the user authenticate from a browser, including on a different device.

### Interactive browser only

```csharp
using Azure.Identity;

services.AddDataverse(options =>
{
    options.DataverseUrl = "https://yourorg.crm4.dynamics.com";
    options.CredentialType = DataverseCredentialType.InteractiveBrowserCredential;
    options.InteractiveBrowserCredentialOptions = new InteractiveBrowserCredentialOptions
    {
        TenantId = "<tenant-id>",
        ClientId = "<application-client-id>"
    };
});
```

This forces the library to use `InteractiveBrowserCredential` instead of trying credentials from the `DefaultAzureCredential` chain. Azure Identity may still reuse authentication cached by the credential instance or browser session.

### DefaultAzureCredential with custom options

```csharp
using Azure.Identity;

services.AddDataverse(options =>
{
    options.CredentialType = DataverseCredentialType.DefaultAzureCredential;
    options.DefaultAzureCredentialOptions = new DefaultAzureCredentialOptions
    {
        TenantId = "<tenant-id>"
    };
});
```

## Providing a custom TokenCredential

An explicitly supplied `TokenCredential` always takes precedence over `CredentialType` and all credential-specific options.

```csharp
TokenCredential credential = GetCredential();

services.AddDataverse(options =>
{
    options.DataverseUrl = "https://yourorg.crm4.dynamics.com";
    options.TokenCredential = credential;
});
```

## Resolving and using ServiceClient

```csharp
using Microsoft.PowerPlatform.Dataverse.Client;

using var provider = services.BuildServiceProvider();
var serviceClient = provider.GetRequiredService<ServiceClient>();
```

To register the related organization-service interfaces as well:

```csharp
services.AddDataverseWithOrganizationServices(options =>
{
    options.DataverseUrl = "https://yourorg.crm4.dynamics.com";
    options.CredentialType = DataverseCredentialType.AzureCliCredential;
});
```

This registers:

- `ServiceClient`
- `IOrganizationServiceAsync2`
- `IOrganizationServiceAsync`
- `IOrganizationService`

## ServiceClientFactory

Use `IServiceClientFactory` when you need separate `ServiceClient` instances:

```csharp
services.AddDataverseFactory(options =>
{
    options.DataverseUrl = "https://yourorg.crm4.dynamics.com";
    options.CredentialType = DataverseCredentialType.DeviceCodeCredential;
});

using var provider = services.BuildServiceProvider();
var factory = provider.GetRequiredService<IServiceClientFactory>();

var defaultClient = factory.CreateClient();

var interactiveClient = factory.CreateClient(new DataverseOptions
{
    DataverseUrl = "https://anotherorg.crm4.dynamics.com",
    CredentialType = DataverseCredentialType.InteractiveBrowserCredential
});
```

The optional `defaultCredential` argument to `AddDataverseFactory` is used only when `DefaultAzureCredential` is selected. A per-client `DataverseOptions.TokenCredential` still has the highest precedence.

## Configuration

When `DataverseOptions.DataverseUrl` is empty, the library reads `DATAVERSE_URL` from the registered `IConfiguration`:

```json
{
  "DATAVERSE_URL": "https://yourorg.crm4.dynamics.com"
}
```

## WhoAmI verification tool

```powershell
cd DataverseWhoAmI
dotnet run
```

The tool executes `WhoAmIRequest` and prints the user, business unit, and organization IDs.

## License

MIT
