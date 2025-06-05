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

            // Setup DI and register ServiceClient and interfaces
            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddDataverseWithOrganizationServices();
            services.AddDataverseFactory();

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

    static void PrintResponse(WhoAmIResponse response)
    {
        Console.WriteLine($"  UserId:         {response.UserId}");
        Console.WriteLine($"  BusinessUnitId: {response.BusinessUnitId}");
        Console.WriteLine($"  OrganizationId: {response.OrganizationId}");
        Console.WriteLine();
    }
}
