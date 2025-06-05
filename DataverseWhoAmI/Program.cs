using System;
using System.Threading.Tasks;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
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

            // Setup DI and register ServiceClient using AddDataverse
            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddDataverse();

            using var serviceProvider = services.BuildServiceProvider();
            var serviceClient = serviceProvider.GetRequiredService<ServiceClient>();

            var whoAmIRequest = new WhoAmIRequest();
            var response = (WhoAmIResponse) await serviceClient.ExecuteAsync(whoAmIRequest);

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
