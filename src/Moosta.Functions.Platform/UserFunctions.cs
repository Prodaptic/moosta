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

namespace Moosta.Functions.Platform
{
    public class UserFunctions
    {
        private static readonly string databaseId = "moosta";
        private static readonly string containerId = "user";

        //Reusable instance of ItemClient which represents the connection to a Cosmos endpoint
        private static Container container = null;
        private readonly IApiAuthentication apiAuthentication;

        public UserFunctions(CosmosClient cosmosClient, IApiAuthentication apiAuthentication)
        {
            container = cosmosClient.GetContainer(databaseId, containerId);
            this.apiAuthentication = apiAuthentication;
        }

        [OpenApiOperation(operationId: "GetUser", tags: new[] { "user" }, Summary = "Get User", Description = "This returns a Moosta user", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("Bearer", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT")]
        [OpenApiParameter("id", Summary = "The requested user's id", Type = typeof(string), In = ParameterLocation.Query, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(MoostaUser), Summary = "The response", Description = "This returns the response")]

        [FunctionName("GetUser")]
        public async Task<IActionResult> GetUser(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "user/{id}")] HttpRequest req,
            ILogger log, string id)
        {
            log.LogInformation($"Get user request received for {id}");

            // Authenticate the user
            var authResult = await apiAuthentication.AuthenticateAsync(req.Headers);
            if (authResult.Failed)
                return new ForbidResult(authenticationScheme: "Bearer");

            try
            {
                var response = await container.ReadItemAsync<MoostaUser>(
                    id: id,
                    partitionKey: new PartitionKey(id)
                    );
                return new OkObjectResult(response.Resource);

            }
            catch (CosmosException cosmosException)
            {
                log.LogError(cosmosException, "Failed to retrieve user");
                return new BadRequestObjectResult($"Failed to retrieve user. Cosmos Status Code {cosmosException.StatusCode}, Error Code {cosmosException.SubStatusCode}: {cosmosException.Message}.");
            }
        }

        [OpenApiOperation(operationId: "GetMe", tags: new[] { "user" }, Summary = "Get Me", Description = "This returns the current authenticated user", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("Bearer", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(MoostaUser), Summary = "The response", Description = "This returns the response")]

        [FunctionName("GetMe")]
        public async Task<IActionResult> GetMe(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = "user/me")] HttpRequest req,
           ILogger log)
        {
            log.LogInformation($"Get user me request received");

            // Authenticate the user
            var authResult = await apiAuthentication.AuthenticateAsync(req.Headers);
            if (authResult.Failed)
                return new ForbidResult(authenticationScheme: "Bearer");

            //find our object id claim
            var authId = authResult.User.Claims
                .FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier").Value;

            try
            {
                var query = new QueryDefinition("SELECT * FROM user u WHERE u.authid = @authid")
                    .WithParameter("@authid", authId);

                //only expecting one result
                var iterator = container.GetItemQueryIterator<MoostaUser>(query,
                    requestOptions: new QueryRequestOptions
                    {
                        MaxItemCount = 1
                    });

                while (iterator.HasMoreResults)
                {
                    var currentResultSet = await iterator.ReadNextAsync();
                    foreach (var user in currentResultSet)
                    {
                        if (user == null)
                            return new NotFoundObjectResult(string.Empty);
                        else
                            return new OkObjectResult(user);
                    }
                }

                return new NotFoundObjectResult(string.Empty);
            }
            catch (CosmosException cosmosException)
            {
                log.LogError(cosmosException, "Failed to retrieve current user");
                return new BadRequestObjectResult($"Failed to retrieve user. Cosmos Status Code {cosmosException.StatusCode}, Error Code {cosmosException.SubStatusCode}: {cosmosException.Message}.");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Failed to get the current user");
                return new BadRequestObjectResult("Failed to get the current user");
            }
        }

        [OpenApiOperation(operationId: "UpdateUser", tags: new[] { "user" }, Summary = "Put User", Description = "This updates the current user", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("Bearer", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT")]
        [OpenApiParameter("user", Summary = "A Moosta User", Type = typeof(MoostaUser), In = ParameterLocation.Query, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Summary = "The response", Description = "This returns the user's id")]
        [FunctionName("PutUser")]
        public async Task<IActionResult> PutUser(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = "user")] HttpRequest req,
           ILogger log)
        {
            log.LogInformation($"Put user request received");

            // Authenticate the user
            var authResult = await apiAuthentication.AuthenticateAsync(req.Headers);
            if (authResult.Failed)
                return new ForbidResult(authenticationScheme: "Bearer");

            var requestBody = new StreamReader(req.Body).ReadToEnd();
            var user = JsonSerializer.Deserialize<MoostaUser>(requestBody);

            //find our object id claim
            var oid = authResult.User.Claims
                .FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

            //you can only edit your own profile right now
            if (user.AuthId != oid)
                new BadRequestObjectResult("Unauthorized");

            var email = authResult.User.Claims.FirstOrDefault(c => c.Type == "emails")?.Value;

            //we are not currently allowing the editing of the email address either
            if (email != user.Email)
                return new BadRequestObjectResult("Cannot edit email");


            try
            {
                var result = await container.ReplaceItemAsync(user, user.Id, new PartitionKey(user.Id));
                return new OkObjectResult(result.Resource);
            }
            catch (CosmosException cosmosException)
            {
                log.LogError(cosmosException, "Failed to update the user");
                return new BadRequestObjectResult($"Failed to update user {user.Id}. Cosmos Status Code {cosmosException.StatusCode}, Error Code {cosmosException.SubStatusCode}: {cosmosException.Message}.");
            }
        }

        [OpenApiOperation(operationId: "CreateUser", tags: new[] { "user" }, Summary = "Create User", Description = "This verifies the current user and creates a DB record if necessary", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("Bearer", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Summary = "The response", Description = "This returns the user's id")]
        [FunctionName("CreateUser")]
        public async Task<IActionResult> CreateUser(
       [HttpTrigger(AuthorizationLevel.Function, "post", Route = "user")] HttpRequest req,
       ILogger log)
        {
            log.LogInformation($"Post user register request received");

            // Authenticate the profile
            var authResult = await apiAuthentication.AuthenticateAsync(req.Headers);
            if (authResult.Failed)
                return new ForbidResult(authenticationScheme: "Bearer");

            //find our object id claim
            var oid = authResult.User.Claims
                .FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

            //find our email
            var email = authResult.User.Claims
             .FirstOrDefault(c => c.Type == "emails")?.Value;

            var user = new MoostaUser
            {
                Id = IdentifierTools.GenerateId(),
                RegisteredDate = DateTime.Now.ToEpoch(),
                AuthId = oid,
                Name = authResult.User.Identity.Name,
                Email = email,
                Roles = "Member" //default to this role
            };

            try
            {
                var result = await container.CreateItemAsync(user, new PartitionKey(user.Id.ToString()));
                return new OkObjectResult(result.Resource);
            }
            catch (CosmosException cosmosException)
            {
                log.LogError(cosmosException, "Failed to create the user");
                return new BadRequestObjectResult($"Failed to create user. Cosmos Status Code {cosmosException.StatusCode}, Error Code {cosmosException.SubStatusCode}: {cosmosException.Message}.");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Failed to create the user");
                return new BadRequestObjectResult("Failed to create the user");
            }
        }
    }
}
