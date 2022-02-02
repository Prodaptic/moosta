using System;
using Microsoft.Azure.Cosmos;
using AzureFunctions.OidcAuthentication;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Moosta.Functions.Platform.Configurations;

[assembly: FunctionsStartup(typeof(Moosta.Functions.Platform.Startup))]
namespace Moosta.Functions.Platform
{
    class Startup : FunctionsStartup
    {
        private static IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Environment.CurrentDirectory)
            .AddEnvironmentVariables()
            .Build();

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddOidcApiAuthorization();

            builder.Services.AddSingleton(
                new OpenAI_API.OpenAIAPI(
                    apiKeys: new OpenAI_API.APIAuthentication(configuration["OpenAIKey"]),
                    engine: configuration["OpenAIEngine"]));

            builder.Services.AddSingleton(
                new CosmosClient(
                    configuration["CosmosEndPointUrl"],
                    configuration["CosmosAccountKey"]));
        }
    }
}
