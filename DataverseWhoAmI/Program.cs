using System;
using System.Threading.Tasks;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Xrm.Sdk;
using DataverseConnection;

class Program
{
    static async Task<int> Main(string[] args)
    {
        try
        {
            // Setup configuration
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            // Resolve which of the three opinionated credential types to use.
            // Precedence: DATAVERSE_CREDENTIAL_TYPE config/env > InteractiveBrowser default.
            var credentialType = ResolveCredentialType(configuration);
            Console.WriteLine($"Using credential: {credentialType}");

            // Setup DI and register ServiceClient and interfaces
            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddDataverseWithOrganizationServices(o => o.CredentialType = credentialType);
            services.AddDataverseFactory(o => o.CredentialType = credentialType);

            using var serviceProvider = services.BuildServiceProvider();

            var whoAmIRequest = new WhoAmIRequest();

            // 1. ServiceClient
            var serviceClient = serviceProvider.GetRequiredService<ServiceClient>();
            var response1 = (WhoAmIResponse)await serviceClient.ExecuteAsync(whoAmIRequest);
            Console.WriteLine("WhoAmIRequest via ServiceClient:");
            PrintResponse(response1);

            // 2. IOrganizationServiceAsync2
            var orgServiceAsync2 = serviceProvider.GetRequiredService<IOrganizationServiceAsync2>();
            var response2 = (WhoAmIResponse)await orgServiceAsync2.ExecuteAsync(whoAmIRequest);
            Console.WriteLine("WhoAmIRequest via IOrganizationServiceAsync2:");
            PrintResponse(response2);

            // 3. IOrganizationServiceAsync
            var orgServiceAsync = serviceProvider.GetRequiredService<IOrganizationServiceAsync>();
            var response3 = (WhoAmIResponse)await orgServiceAsync.ExecuteAsync(whoAmIRequest);
            Console.WriteLine("WhoAmIRequest via IOrganizationServiceAsync:");
            PrintResponse(response3);

            // 4. IOrganizationService (sync)
            var orgService = serviceProvider.GetRequiredService<IOrganizationService>();
            var response4 = (WhoAmIResponse)orgService.Execute(whoAmIRequest);
            Console.WriteLine("WhoAmIRequest via IOrganizationService (sync):");
            PrintResponse(response4);

            // 5. Client from factory
            var factory = serviceProvider.GetRequiredService<IServiceClientFactory>();
            var factoryClient = factory.CreateClient();
            var response5 = (WhoAmIResponse)await factoryClient.ExecuteAsync(whoAmIRequest);
            Console.WriteLine("WhoAmIRequest via IServiceClientFactory:");
            PrintResponse(response5);

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("An error occurred while connecting to Dataverse or executing WhoAmIRequest:");
            Console.Error.WriteLine(ex.Message);
            return 99;
        }
    }

    static DataverseCredentialType ResolveCredentialType(IConfiguration configuration)
    {
        // Configured via appsettings.json or the DATAVERSE_CREDENTIAL_TYPE environment variable.
        var configured = configuration["DATAVERSE_CREDENTIAL_TYPE"];
        if (string.IsNullOrWhiteSpace(configured))
            return DataverseCredentialType.InteractiveBrowserCredential;

        if (TryParseCredentialType(configured, out var credentialType))
            return credentialType;

        throw new ArgumentException(
            $"Unknown DATAVERSE_CREDENTIAL_TYPE '{configured}'. Valid values: browser, devicecode, azurecli.");
    }

    static bool TryParseCredentialType(string value, out DataverseCredentialType credentialType)
    {
        switch (value.Trim().ToLowerInvariant())
        {
            case "browser":
            case "interactive":
            case "interactivebrowser":
                credentialType = DataverseCredentialType.InteractiveBrowserCredential;
                return true;
            case "device":
            case "devicecode":
                credentialType = DataverseCredentialType.DeviceCodeCredential;
                return true;
            case "az":
            case "cli":
            case "azurecli":
                credentialType = DataverseCredentialType.AzureCliCredential;
                return true;
            default:
                credentialType = default;
                return false;
        }
    }

    static void PrintResponse(WhoAmIResponse response)
    {
        Console.WriteLine($"  UserId:         {response.UserId}");
        Console.WriteLine($"  BusinessUnitId: {response.BusinessUnitId}");
        Console.WriteLine($"  OrganizationId: {response.OrganizationId}");
        Console.WriteLine();
    }
}
