# Product Context

## Why This Project Exists
Connecting to Microsoft Dataverse programmatically is a common requirement for developers and administrators working with Power Platform and Dynamics 365. Verifying connectivity and authentication is often the first troubleshooting step, but existing tools may require manual configuration, legacy authentication, or lack automation support.

## Problems Solved
- Provides a simple, repeatable way to verify Dataverse connectivity and identity from the command line.
- Eliminates the need for legacy authentication flows by leveraging Azure DefaultAzureCredential, supporting modern, secure, and automated authentication scenarios.
- Reduces friction for developers and DevOps engineers integrating Dataverse checks into CI/CD pipelines or local scripts.
- **Enables reuse of secure Dataverse connection logic across multiple projects via a NuGet package and AddDataverse extension method.**

## How It Should Work
- User runs the tool from the command line, optionally providing the Dataverse environment URL.
- The tool authenticates using DefaultAzureCredential, requiring no explicit secrets or credentials if the environment is configured (supports managed identity, Visual Studio, Azure CLI, etc.).
- The tool connects to Dataverse and executes a WhoAmIRequest.
- The result (UserId, BusinessUnitId, OrganizationId) is printed to the console in a clear, readable format.
- **The Dataverse connection logic is provided as a reusable class library (NuGet), with an AddDataverse extension method for easy DI registration in any .NET project.**

## User Experience Goals
- Zero configuration for users with Azure authentication already set up.
- Fast feedback: connection and identity result in seconds.
- Clear error messages if authentication or connectivity fails.
- Usable both interactively and in automation (e.g., CI/CD).
- **Reusable connection logic for rapid integration in other .NET solutions.**
