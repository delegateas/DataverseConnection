using System;
using System.Threading.Tasks;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Extensions.DependencyInjection;
using DataverseConnection;

class Program
{
    static async Task<int> Main(string[] args)
    {
        string dataverseUrl = args.Length > 0
            ? args[0]
            : Environment.GetEnvironmentVariable("DATAVERSE_URL");

        if (string.IsNullOrWhiteSpace(dataverseUrl))
        {
            Console.Error.WriteLine("Error: Dataverse URL must be provided as the first argument or via the DATAVERSE_URL environment variable.");
            Console.Error.WriteLine("Usage: dotnet run -- <DataverseUrl>");
            return 1;
        }

        try
        {
            // Setup DI and register ServiceClient using AddDataverse
            var services = new ServiceCollection();
            services.AddDataverse(options =>
            {
                options.DataverseUrl = dataverseUrl;
                // options.TokenCredential = ... // Optional: customize credential if needed
            });

            using var serviceProvider = services.BuildServiceProvider();
            var serviceClient = serviceProvider.GetRequiredService<ServiceClient>();

            var whoAmIRequest = new WhoAmIRequest();
            var response = (WhoAmIResponse)serviceClient.Execute(whoAmIRequest);

            Console.WriteLine("WhoAmIRequest Result:");
            Console.WriteLine($"  UserId:         {response.UserId}");
            Console.WriteLine($"  BusinessUnitId: {response.BusinessUnitId}");
            Console.WriteLine($"  OrganizationId: {response.OrganizationId}");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("An error occurred while connecting to Dataverse or executing WhoAmIRequest:");
            Console.Error.WriteLine(ex.Message);
            return 99;
        }
    }
}
