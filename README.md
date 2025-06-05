# DataverseConnection (.NET NuGet Package)

## Overview

**DataverseConnection** is a .NET 8 class library (NuGet package) that provides secure, reusable, and dependency-injectable connection logic for Microsoft Dataverse. It enables any .NET application or service to connect to Dataverse using modern Azure authentication, with a single line of DI registration.

- **Primary Focus:** The DataverseConnection library and its `AddDataverse` extension method for DI.
- **Verification Tool:** The included WhoAmI CLI app demonstrates and verifies the library's functionality.

---

## Why Use DataverseConnection?

- **Modern, Secure Authentication:** Uses Azure DefaultAzureCredential — no secrets or legacy flows.
- **Rapid Integration:** Register Dataverse ServiceClient in your DI container with a single extension method.
- **Reusable:** Designed for use across multiple .NET projects and deployment scenarios.
- **Automation Ready:** Works seamlessly in CI/CD, cloud, and local development environments.
- **Extensible:** Built for maintainability and easy extension.

---

## Features

- Reusable .NET 8 class library for Dataverse connectivity
- `AddDataverse` extension method for `IServiceCollection` (DI registration)
- Authenticates using Azure DefaultAzureCredential (supports managed identity, Visual Studio, Azure CLI, etc.)
- Clear error handling and diagnostics
- Cross-platform: Windows, Linux, macOS

---

## Architecture

```
DataverseConnection (NuGet Library)
│
└── ServiceCollectionExtensions.cs
      └── AddDataverse (extension method for DI)
      └── DataverseOptions (configuration)
      └── Connection logic (encapsulated)
      
Example/Verification Tool:
DataverseWhoAmI (CLI)
└── Uses DataverseConnection via DI to verify connectivity
```

- **Separation of Concerns:** Connection logic is decoupled from application logic.
- **Dependency Injection:** ServiceClient is provided via DI for testability and maintainability.

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- Access to a Microsoft Dataverse environment
- Azure authentication configured (DefaultAzureCredential supported methods)

---

## Getting Started

### 1. Reference the NuGet Package

Add a reference to the DataverseConnection NuGet package in your .NET 8 project (or use the project reference if building locally).

### 2. Register Dataverse in DI

```csharp
using DataverseConnection;

var services = new ServiceCollection();
services.AddDataverse(options =>
{
    // This is optional, by default the url is fetched from DATAVERSE_URL from the configuration
    options.EnvironmentUrl = "<DataverseEnvironmentUrl>";
});
```

### 3. Resolve and Use ServiceClient

```csharp
var provider = services.BuildServiceProvider();
var serviceClient = provider.GetRequiredService<ServiceClient>();

// Use serviceClient for Dataverse operations
```

---

## Advanced: Using ServiceClientFactory for Long-Running or Multi-Instance Scenarios

For advanced scenarios—such as long-running jobs, background processing, or when you need multiple independent ServiceClient instances—you can use the `ServiceClientFactory`:

### 1. Register the Factory

```csharp
using DataverseConnection;

var services = new ServiceCollection();
services.AddDataverseFactory(options =>
{
    // This is optional, by default the url is fetched from DATAVERSE_URL from the configuration
    options.DataverseUrl = "<DataverseEnvironmentUrl>";
});
```

### 2. Resolve and Use the Factory

```csharp
var provider = services.BuildServiceProvider();
var factory = provider.GetRequiredService<IServiceClientFactory>();

// Create a new ServiceClient instance (optionally override options per call)
var serviceClient = factory.CreateClient(); // uses default options

// Or, provide custom options for this instance
var customClient = factory.CreateClient(new DataverseConnection.DataverseOptions
{
    DataverseUrl = "<AnotherDataverseEnvironmentUrl>"
});
```

> **Note:** The default singleton ServiceClient registration remains recommended for most use cases. Use the factory only when you need to create new, independent ServiceClient instances (e.g., for parallel, long-running, or isolated operations).

---

## Example: WhoAmI Verification Tool

The repository includes a CLI tool (`DataverseWhoAmI`) that demonstrates and verifies the DataverseConnection library.

### Add appsettings.json

```
{
  "DATAVERSE_URL": "https://yoururl.crm4.dynamics.com"
}
```

### Run the CLI Tool

```sh
cd DataverseWhoAmI
dotnet run
```

The tool prints the result of the WhoAmIRequest:

```
UserId:           <guid>
BusinessUnitId:   <guid>
OrganizationId:   <guid>
```

---

## License

MIT
