# Project Brief

## Project Title
Dataverse WhoAmI .NET Tool

## Objective
Create a .NET 8 command-line tool that connects to Microsoft Dataverse, authenticates using Azure DefaultAzureCredential, and prints the result of a WhoAmIRequest using the Dataverse ServiceClient.

## Core Requirements
- .NET 8 project (console application or dotnet tool)
- Uses Microsoft.PowerPlatform.Dataverse.Client (ServiceClient)
- Authenticates with DefaultAzureCredential (Azure.Identity)
- Connects to a Dataverse environment
- Executes a WhoAmIRequest and prints the result to the console
- Usable as a dotnet tool

## Out of Scope
- UI/UX beyond console output
- Non-Azure authentication methods
- Advanced Dataverse operations beyond WhoAmIRequest

## Success Criteria
- Tool builds and runs on .NET 8
- Authenticates using Azure DefaultAzureCredential
- Successfully connects to Dataverse and prints WhoAmIRequest result
- Can be packaged and used as a dotnet tool
