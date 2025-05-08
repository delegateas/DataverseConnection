using System;
using System.Threading.Tasks;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Crm.Sdk.Messages;
using Azure.Identity;
using Azure.Core;

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
            var credential = new DefaultAzureCredential();
            // The resource scope for Dataverse
            string resource = $"{new Uri(dataverseUrl).GetLeftPart(UriPartial.Authority)}/.default";

            // Token provider function for ServiceClient
            async Task<string> TokenProvider(string url)
            {
                var tokenRequestContext = new TokenRequestContext(new[] { resource });
                var token = await credential.GetTokenAsync(tokenRequestContext, default);
                return token.Token;
            }

            using var serviceClient = new ServiceClient(
                new Uri(dataverseUrl),
                tokenProviderFunction: TokenProvider);

            if (!serviceClient.IsReady)
            {
                Console.Error.WriteLine("Error: ServiceClient is not ready. Check your credentials and Dataverse URL.");
                return 2;
            }

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
