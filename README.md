# DataverseConnection (.NET NuGet Package)

## Overview

**DataverseConnection** is a .NET 8 class library that provides reusable, dependency-injectable connection logic for Microsoft Dataverse. It is opinionated about interactive authentication: when used from the CLI you choose between three human-friendly credential types, while library callers can still plug in any `TokenCredential`.

The included `DataverseWhoAmI` console application demonstrates the library and verifies connectivity.

## Features

- Reusable .NET 8 library for Dataverse connectivity
- Dependency injection through `AddDataverse`, `AddDataverseWithOrganizationServices`, and `AddDataverseFactory`
- Three opinionated Azure Identity credential types:
  - `InteractiveBrowserCredential` (**default**)
  - `DeviceCodeCredential`
  - `AzureCliCredential`
- Any explicitly supplied `TokenCredential` when calling the library directly (for example `DefaultAzureCredential` or a service principal)
- Credential-specific Azure Identity options
- Persistent token caching by default for the interactive credentials, so you log in as rarely as possible
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

If no credential type or custom credential is specified, the library uses `InteractiveBrowserCredential` — a person running the tool from a computer can always complete a browser sign-in.

## Selecting a credential type

Set `DataverseOptions.CredentialType` to one of the three opinionated Azure Identity credentials. Selecting a type does not create a fallback chain.

| Value | Credential | Notes |
| --- | --- | --- |
| `InteractiveBrowserCredential` (default) | `InteractiveBrowserCredential` | Opens a browser sign-in; persistent token cache by default. |
| `DeviceCodeCredential` | `DeviceCodeCredential` | Prints a code to sign in from any device; persistent token cache by default. |
| `AzureCliCredential` | `AzureCliCredential` | Reuses an existing `az login` session (the `az` CLI owns its own cache). |

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

When you do **not** supply `InteractiveBrowserCredentialOptions`, the library enables persistent token caching automatically (see [Persistent token caching](#persistent-token-caching)). Supplying your own options means the library uses them as-is and does not add caching on your behalf.

## Providing a custom TokenCredential

The three built-in types are the opinionated choices for the CLI. When calling the library directly you are not limited to them: set `DataverseOptions.TokenCredential` to any credential — for example `DefaultAzureCredential`, a service principal, or a managed identity. An explicitly supplied `TokenCredential` always takes precedence over `CredentialType` and all credential-specific options.

```csharp
using Azure.Identity;

// Use DefaultAzureCredential (or any TokenCredential) when hosting the library yourself.
TokenCredential credential = new DefaultAzureCredential();

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

To inject a custom credential into the factory, set `DataverseOptions.TokenCredential` in the `configureOptions` callback (or per client via `CreateClient`). A per-client `DataverseOptions.TokenCredential` always has the highest precedence.

## Persistent token caching

For `InteractiveBrowserCredential` and `DeviceCodeCredential`, the library enables persistent token caching by default (when you do not pass your own credential-specific options). Tokens and the signed-in account are stored under `~/.dataverseconnection`, so subsequent runs — including separate CLI invocations — acquire tokens silently instead of prompting again. `AzureCliCredential` is unaffected because the `az` CLI manages its own cache.

The on-disk cache is encrypted using the operating system keychain (DPAPI on Windows, Keychain on macOS, **libsecret on Linux/WSL**). If encrypted storage is unavailable — common on headless Linux or WSL without libsecret — the library falls back to a non-persistent credential that prompts on every run, rather than writing tokens to disk unencrypted.

## Configuration

When `DataverseOptions.DataverseUrl` is empty, the library reads `DATAVERSE_URL` from the registered `IConfiguration`:

```json
{
  "DATAVERSE_URL": "https://yourorg.crm4.dynamics.com",
  "DATAVERSE_CREDENTIAL_TYPE": "browser"
}
```

`DATAVERSE_CREDENTIAL_TYPE` is optional and accepts `browser`, `devicecode`, or `azurecli`.

## WhoAmI verification tool

```powershell
cd DataverseWhoAmI
dotnet run
```

The tool executes `WhoAmIRequest` and prints the user, business unit, and organization IDs.

## License

MIT
