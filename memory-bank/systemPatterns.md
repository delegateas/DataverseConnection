# System Patterns

## Architecture Overview
- Single-project .NET 8 console application, packaged as a dotnet tool.
- Entry point: Program.cs (or Program.ts if using C# with TypeScript interop, but default is C#).
- Uses Microsoft.PowerPlatform.Dataverse.Client.ServiceClient for Dataverse operations.
- Authenticates using Azure.Identity.DefaultAzureCredential.

## Key Technical Decisions
- **Authentication**: DefaultAzureCredential is used to support local development, managed identity, and service principal scenarios without code changes.
- **Configuration**: Dataverse environment URL is provided as a command-line argument or via environment variable.
- **Error Handling**: All exceptions (authentication, connection, request) are caught and printed with clear messages.
- **Output**: WhoAmIRequest result is printed in a human-readable format (UserId, BusinessUnitId, OrganizationId).

## Design Patterns
- Minimal CLI pattern: Parse arguments, validate input, execute main logic, print result.
- Service abstraction: (Optional) Encapsulate Dataverse connection and WhoAmI logic in a service class for testability and separation of concerns.
- Dependency injection: Not required for this minimal tool, but can be added if extensibility is needed.

## Component Relationships
- Program/Main: Handles argument parsing and orchestration.
- DataverseService (optional): Handles connection and WhoAmIRequest logic.
- Azure.Identity: Provides authentication context for ServiceClient.

## Critical Implementation Paths
1. Parse Dataverse URL from args or environment.
2. Instantiate DefaultAzureCredential.
3. Create ServiceClient with Dataverse URL and credential.
4. Execute WhoAmIRequest.
5. Print result or error.
