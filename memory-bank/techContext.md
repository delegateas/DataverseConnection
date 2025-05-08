# Tech Context

## Technologies Used
- **.NET 8**: Target framework for the console application/dotnet tool.
- **Microsoft.PowerPlatform.Dataverse.Client**: Provides ServiceClient for Dataverse connectivity and operations.
- **Azure.Identity**: Supplies DefaultAzureCredential for modern Azure authentication.
- **Command-line interface (CLI)**: Tool is invoked via dotnet CLI.

## Development Setup
- Requires .NET 8 SDK installed.
- Project is structured as a console application, optionally packaged as a dotnet tool for global/local installation.
- Dependencies managed via NuGet.
- Source code in C# (Program.cs and supporting files).
- Build and run using standard dotnet CLI commands.

## Technical Constraints
- Only DefaultAzureCredential is supported for authentication (no username/password, client secret, or certificate flows).
- Dataverse environment URL must be provided as a command-line argument or environment variable.
- Tool must run cross-platform (Windows, Linux, macOS) as supported by .NET 8 and dependencies.

## Dependencies
- Microsoft.PowerPlatform.Dataverse.Client (NuGet)
- Azure.Identity (NuGet)
- .NET 8 SDK

## Tool Usage Patterns
- Install as a local or global dotnet tool (`dotnet tool install`).
- Run from command line, passing Dataverse URL as argument or setting environment variable.
- Output is printed to standard output for easy integration with scripts or CI/CD pipelines.
