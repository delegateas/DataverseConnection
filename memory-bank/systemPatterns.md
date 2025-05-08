# System Patterns

## Architecture Overview
- Two-project solution:
  - **DataverseWhoAmI**: .NET 8 console application (CLI tool).
  - **DataverseConnection**: .NET 8 class library providing reusable Dataverse connection logic (NuGet-ready).
- Entry point: Program.cs in DataverseWhoAmI.
- Uses Microsoft.PowerPlatform.Dataverse.Client.ServiceClient for Dataverse operations.
- Authenticates using Azure.Identity.DefaultAzureCredential.
- Dependency injection is used to provide ServiceClient via the AddDataverse extension method.

## Key Technical Decisions
- **Authentication**: DefaultAzureCredential is used to support local development, managed identity, and service principal scenarios without code changes.
- **Configuration**: Dataverse environment URL is provided as a command-line argument or via environment variable.
- **Error Handling**: All exceptions (authentication, connection, request) are caught and printed with clear messages.
- **Output**: WhoAmIRequest result is printed in a human-readable format (UserId, BusinessUnitId, OrganizationId).
- **Separation of Concerns**: Connection logic is encapsulated in a reusable library, decoupled from CLI logic.
- **Extensibility**: AddDataverse extension method enables easy DI registration in any .NET project.

## Design Patterns
- Minimal CLI pattern: Parse arguments, validate input, execute main logic, print result.
- **Extension method pattern**: AddDataverse registers ServiceClient for DI.
- **Dependency injection**: Used for ServiceClient provisioning and testability.
- Service abstraction: Connection logic is encapsulated in the DataverseConnection library for reuse.

## Component Relationships
- Program/Main: Handles argument parsing and orchestration.
- DataverseConnection (library): Handles connection logic and DI registration.
- ServiceClient: Provided via DI to the CLI app.
- Azure.Identity: Provides authentication context for ServiceClient.

## Critical Implementation Paths
1. Parse Dataverse URL from args or environment.
2. Register ServiceClient using AddDataverse extension method.
3. Resolve ServiceClient from DI.
4. Execute WhoAmIRequest.
5. Print result or error.
