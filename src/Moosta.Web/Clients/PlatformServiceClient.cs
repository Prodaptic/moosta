using Moosta.Shared.Platform;
using Moosta.Shared.Platform.Models;
using System.Net.Http.Json;

namespace Moosta.Web.Clients
{
    public class PlatformServiceClient : IPlatformServiceClient
    {
        private readonly HttpClient _client;

        public PlatformServiceClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<MoostaCompletion> GetCompletionAsync(MoostaCompletionRequest request)
        {
            var result = await _client.GetFromJsonAsync<MoostaCompletion>($"/completion?prompt={request.Prompt}");
            if (result == null)
                throw new HttpRequestException("Unable to execute completion");
            return result;
        }

        #region User
        public async Task<MoostaUser> CreateUserAsync()
        {
            var result = await _client.PostAsync($"/user", null);

            var user = await result.Content.ReadFromJsonAsync<MoostaUser>();
            if (user == null)
                throw new HttpRequestException("Failed to create the user");

            return user;
        }

        public async Task<MoostaUser> GetMeAsync()
        {
            var result = await _client.GetFromJsonAsync<MoostaUser>($"/user/me");
            if (result == null)
                throw new HttpRequestException("User Not Found");
            return result;
        }

        #endregion

        #region Idea
        public async Task<MoostaIdea> CreateIdeaAsync()
        {
            var result = await _client.PostAsync($"/idea", null);

            var idea = await result.Content.ReadFromJsonAsync<MoostaIdea>();
            if (idea == null)
                throw new HttpRequestException("Failed to create the idea");

            return idea;
        }

        public async Task<MoostaIdea> GetIdeaAsync(string id)
        {
            var result = await _client.GetFromJsonAsync<MoostaIdea>($"/idea/{id}");
            if (result == null)
                throw new HttpRequestException("Idea Not Found");
            return result;
        }

        public async Task<IEnumerable<MoostaIdea>> GetIdeasAsync()
        {
            var result = await _client.GetFromJsonAsync<IEnumerable<MoostaIdea>>($"/idea");
            if (result == null)
                throw new HttpRequestException("Could not retrieve ideas");
            return result;
        }

        public async Task<MoostaIdea> UpdateIdeaAsync(string id, MoostaIdea idea)
        {
            var result = await _client.PutAsJsonAsync($"/idea/{id}", idea);

            var ideaResult = await result.Content.ReadFromJsonAsync<MoostaIdea>();
            if (ideaResult == null)
                throw new HttpRequestException("Failed to create the idea");

            return ideaResult;
        }

        #endregion
    }
}
