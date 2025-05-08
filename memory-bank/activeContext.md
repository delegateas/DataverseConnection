# Active Context

## Current Work Focus
- Initializing project documentation (Memory Bank) for the Dataverse WhoAmI .NET Tool.
- Capturing requirements, architecture, technology stack, and product context.

## Recent Changes
- Created projectbrief.md, productContext.md, systemPatterns.md, and techContext.md with detailed project information.

## Next Steps
- Scaffold a new .NET 8 console application suitable for packaging as a dotnet tool.
- Add references to Microsoft.PowerPlatform.Dataverse.Client and Azure.Identity via NuGet.
- Implement logic to:
  - Parse Dataverse URL from command-line or environment variable.
  - Authenticate using DefaultAzureCredential.
  - Connect to Dataverse and execute WhoAmIRequest.
  - Print result to console.
- Add error handling and clear output formatting.
- Document implementation progress and update memory bank as work proceeds.

## Active Decisions and Considerations
- Only DefaultAzureCredential will be supported for authentication.
- Tool will be CLI-first, with no additional UI.
- Output will be human-readable and suitable for automation.

## Important Patterns and Preferences
- Minimal CLI pattern for argument parsing and execution.
- Service abstraction for Dataverse logic (optional, for testability).
- Cross-platform compatibility.

## Learnings and Project Insights
- Memory Bank structure ensures all project context is preserved and accessible for future work.
- .NET 8 and Azure.Identity provide a modern, secure foundation for Dataverse tooling.
