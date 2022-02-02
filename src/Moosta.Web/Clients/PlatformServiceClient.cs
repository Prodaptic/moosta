using Microsoft.AspNetCore.WebUtilities;
using Moosta.Shared.Platform;
using Moosta.Shared.Platform.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace Moosta.Web.Clients
{
    public class PlatformServiceClient : IPlatformServiceClient
    {
        private readonly HttpClient _client;

        public PlatformServiceClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<MoostaUser> CreateUserAsync()
        {
            var result = await _client.PostAsync($"/user", null);

            var user = await result.Content.ReadFromJsonAsync<MoostaUser>();
            if (user == null)
                throw new HttpRequestException("Failed to create the user");

            return user;
        }

        public async Task<MoostaCompletion> GetCompletionAsync(MoostaCompletionRequest request)
        {
            var result = await _client.GetFromJsonAsync<MoostaCompletion>($"/completion?prompt={request.Prompt}");
            if (result == null)
                throw new HttpRequestException("Unable to execute completion");
            return result;
        }

        public MoostaUser GetUser(string id)
        {
            throw new NotImplementedException();
        }

        public async Task<MoostaUser> GetMeAsync()
        {
            var result = await _client.GetFromJsonAsync<MoostaUser>($"/user/me");
            if (result == null)
                throw new HttpRequestException("User Not Found");
            return result;
        }

        public MoostaUser UpdateUser(MoostaUser user)
        {
            throw new NotImplementedException();
        }
    }
}
