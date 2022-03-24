using AzureFunctions.OidcAuthentication;
using Moosta.Core;
using Moosta.Shared.Platform.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using System.Collections.Generic;

namespace Moosta.Functions.Platform
{
    public class IdeaFunctions
    {
        private static readonly string databaseId = "moosta";
        private static readonly string containerId = "idea";

        //Reusable instance of ItemClient which represents the connection to a Cosmos endpoint
        private static Container container = null;
        private readonly IApiAuthentication apiAuthentication;

        public IdeaFunctions(CosmosClient cosmosClient, IApiAuthentication apiAuthentication)
        {
            container = cosmosClient.GetContainer(databaseId, containerId);
            this.apiAuthentication = apiAuthentication;
        }

        [OpenApiOperation(operationId: "GetIdea", tags: new[] { "idea" }, Summary = "Get Idea", Description = "This returns a Moosta idea", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("Bearer", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT")]
        [OpenApiParameter("id", Summary = "The requested idea's id", Type = typeof(string), In = ParameterLocation.Path, Required = true, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(MoostaIdea), Summary = "The response", Description = "This returns the response")]

        [FunctionName("GetIdea")]
        public async Task<IActionResult> GetIdea(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "idea/{id}")] HttpRequest req,
            ILogger log, string id)
        {
            log.LogInformation($"Get idea request received for {id}");

            // Authenticate the user
            var authResult = await apiAuthentication.AuthenticateAsync(req.Headers);
            if (authResult.Failed)
                return new ForbidResult(authenticationScheme: "Bearer");

            try
            {
                var response = await container.ReadItemAsync<MoostaIdea>(
                    id: id,
                    partitionKey: new PartitionKey(id)
                    );
                return new OkObjectResult(response.Resource);

            }
            catch (CosmosException cosmosException)
            {
                log.LogError(cosmosException, "Failed to retrieve idea");
                return new BadRequestObjectResult($"Failed to retrieve idea. Cosmos Status Code {cosmosException.StatusCode}, Error Code {cosmosException.SubStatusCode}: {cosmosException.Message}.");
            }
        }

        [OpenApiOperation(operationId: "GetIdeas", tags: new[] { "idea" }, Summary = "Get Ideas", Description = "This returns all the user's Moosta Ideas", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("Bearer", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(IEnumerable<MoostaIdea>), Summary = "The response", Description = "This returns the response")]

        [FunctionName("GetIdeas")]
        public async Task<IActionResult> GetIdeas(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "idea")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation($"Get ideas request received");

            // Authenticate the user
            var authResult = await apiAuthentication.AuthenticateAsync(req.Headers);
            if (authResult.Failed)
                return new ForbidResult(authenticationScheme: "Bearer");

            //find our object id claim
            var oid = authResult.User.Claims
                .FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

            try
            {
                var query = new QueryDefinition(
                    $"select * from idea i where i.createdby = '{oid}'");

                var allIdeas = new List<MoostaIdea>();
                using (FeedIterator<MoostaIdea> resultSet = container.GetItemQueryIterator<MoostaIdea>(
                    query))
                {
                    while (resultSet.HasMoreResults)
                    {
                        FeedResponse<MoostaIdea> response = await resultSet.ReadNextAsync();
                        MoostaIdea sale = response.First();

                        allIdeas.AddRange(response);
                    }
                }
                return new OkObjectResult(allIdeas);

            }
            catch (CosmosException cosmosException)
            {
                log.LogError(cosmosException, "Failed to retrieve ideas");
                return new BadRequestObjectResult($"Failed to retrieve ideas. Cosmos Status Code {cosmosException.StatusCode}, Error Code {cosmosException.SubStatusCode}: {cosmosException.Message}.");
            }
        }

        [OpenApiOperation(operationId: "UpdateIdea", tags: new[] { "user" }, Summary = "Put User", Description = "This updates the idea")]
        [OpenApiSecurity("Bearer", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT")]
        [OpenApiParameter("id", Summary = "The idea's id to update", Type = typeof(string), In = ParameterLocation.Path, Required = true, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Summary = "The response", Description = "This returns the user's id")]
        [FunctionName("UpdateIdea")]
        public async Task<IActionResult> PutIdea(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = "idea/{id}")] HttpRequest req,
           ILogger log, string id)
        {
            log.LogInformation($"Put idea request received for id: {id}");

            // Authenticate the user
            var authResult = await apiAuthentication.AuthenticateAsync(req.Headers);
            if (authResult.Failed)
                return new ForbidResult(authenticationScheme: "Bearer");

            var requestBody = new StreamReader(req.Body).ReadToEnd();
            var idea = JsonSerializer.Deserialize<MoostaIdea>(requestBody);

            //find our object id claim
            var oid = authResult.User.Claims
                .FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

            try
            {
                var result = await container.ReplaceItemAsync(idea, idea.Id, new PartitionKey(idea.Id));
                return new OkObjectResult(result.Resource);
            }
            catch (CosmosException cosmosException)
            {
                log.LogError(cosmosException, "Failed to update the idea");
                return new BadRequestObjectResult($"Failed to update idea {idea.Id}. Cosmos Status Code {cosmosException.StatusCode}, Error Code {cosmosException.SubStatusCode}: {cosmosException.Message}.");
            }
        }

        [OpenApiOperation(operationId: "CreateIdea", tags: new[] { "idea" }, Summary = "Create User", Description = "This creates a new idea", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("Bearer", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Summary = "The response", Description = "This returns the idea's id")]
        [FunctionName("CreateIdea")]
        public async Task<IActionResult> CreateIdea(
       [HttpTrigger(AuthorizationLevel.Function, "post", Route = "idea")] HttpRequest req,
       ILogger log)
        {
            log.LogInformation($"Post idea request received");

            // Authenticate the profile
            var authResult = await apiAuthentication.AuthenticateAsync(req.Headers);
            if (authResult.Failed)
                return new ForbidResult(authenticationScheme: "Bearer");

            string requestBody = String.Empty;
            using (StreamReader streamReader = new StreamReader(req.Body))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }

            var idea = new MoostaIdea();

            if(!string.IsNullOrEmpty(requestBody))
                idea = JsonSerializer.Deserialize<MoostaIdea>(requestBody);

            var authId = authResult.User.Claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier").Value;

            idea.Id = IdentifierTools.GenerateId();
            idea.CreatedDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            idea.CreatedByUserId = authId;

            //set some defaults
            idea.IsPublic = false;
            idea.Title = $"Untitled - {DateTime.Today.ToShortDateString}";
            idea.Description = "The start of an idea";

            try
            {
                var result = await container.CreateItemAsync(idea, new PartitionKey(idea.Id.ToString()));
                return new OkObjectResult(result.Resource);
            }
            catch (CosmosException cosmosException)
            {
                log.LogError(cosmosException, "Failed to create the idea");
                return new BadRequestObjectResult($"Failed to create idea. Cosmos Status Code {cosmosException.StatusCode}, Error Code {cosmosException.SubStatusCode}: {cosmosException.Message}.");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Failed to create the idea");
                return new BadRequestObjectResult("Failed to create the idea");
            }
        }
    }
}
