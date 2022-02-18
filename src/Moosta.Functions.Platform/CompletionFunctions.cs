using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Moosta.Shared.Platform.Models;
using AzureFunctions.OidcAuthentication;
using OpenAI_API;
using System;

namespace Moosta.Functions.Platform
{
    public class CompletionFunctions
    {
        private readonly IApiAuthentication _apiAuthentication;
        private readonly OpenAIAPI _openaiClient;

        public CompletionFunctions(IApiAuthentication apiAuthentication, OpenAIAPI openaiClient)
        {
            _apiAuthentication = apiAuthentication;
            _openaiClient = openaiClient;
        }

        [FunctionName("GetCompletion")]
        [OpenApiOperation(operationId: "GetCompletion", tags: new[] { "completion" }, Summary = "Completion", Description = "This returns a completion from the passed in prompt", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("Bearer", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT")]
        [OpenApiParameter("prompt", Summary = "The prompt to return the completion", Type = typeof(string), In = ParameterLocation.Query, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(MoostaCompletion), Summary = "The response", Description = "This returns the response")]
        public async Task<IActionResult> GetCompletion(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "completion")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("A completion request has been received.");

            try
            {
                // Authenticate the user
                var authResult = await _apiAuthentication.AuthenticateAsync(req.Headers);
                if (authResult.Failed)
                    return new ForbidResult(authenticationScheme: "Bearer");

                //set our ai values for this request
                var temp = 0.88;
                var tokens = 200;
                var frequency = 0;
                var presence = 0;

                var prompt = req.Query["prompt"];

                var result = await _openaiClient.Completions.CreateCompletionAsync(prompt,
                                max_tokens: tokens,
                                temperature: temp,
                                frequencyPenalty: frequency,
                                presencePenalty: presence,
                                echo: false,
                                top_p: 1);

                var completion = new MoostaCompletion
                {
                    CompletionText = result.ToString()
                };

                return new OkObjectResult(completion);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Failed to retrieve the completion", req.ToString());
                return new BadRequestObjectResult("Failed to retrieve the completion");
            }
        }
    }
}
