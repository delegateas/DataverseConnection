# Tech Context

## Technologies Used
- **.NET 8**: Target framework for both the console application and the class library.
- **Microsoft.PowerPlatform.Dataverse.Client**: Provides ServiceClient for Dataverse connectivity and operations.
- **Azure.Identity**: Supplies DefaultAzureCredential for modern Azure authentication.
- **Microsoft.Extensions.DependencyInjection**: Used for DI in both CLI and library.
- **DataverseConnection (class library)**: Provides reusable AddDataverse extension method for DI registration.
- **Command-line interface (CLI)**: Tool is invoked via dotnet CLI.

## Development Setup
- Requires .NET 8 SDK installed.
- Solution is structured as two projects:
  - DataverseWhoAmI (console app)
  - DataverseConnection (class library, NuGet-ready)
- DataverseWhoAmI references DataverseConnection via project reference.
- Dependencies managed via NuGet.
- Source code in C# (Program.cs, ServiceCollectionExtensions.cs, etc.).
- Build and run using standard dotnet CLI commands.

## Technical Constraints
- Only DefaultAzureCredential is supported for authentication (no username/password, client secret, or certificate flows).
- Dataverse environment URL must be provided as a command-line argument or environment variable.
- Tool and library must run cross-platform (Windows, Linux, macOS) as supported by .NET 8 and dependencies.
- Connection logic must be reusable via NuGet package and AddDataverse extension method.

## Dependencies
- Microsoft.PowerPlatform.Dataverse.Client (NuGet)
- Azure.Identity (NuGet)
- Microsoft.Extensions.DependencyInjection (NuGet, included in .NET 8)
- .NET 8 SDK

## Tool Usage Patterns
- Install as a local or global dotnet tool (`dotnet tool install`).
- Run from command line, passing Dataverse URL as argument or setting environment variable.
- Output is printed to standard output for easy integration with scripts or CI/CD pipelines.
- **For reuse:** Reference DataverseConnection NuGet package and call AddDataverse in DI setup.
