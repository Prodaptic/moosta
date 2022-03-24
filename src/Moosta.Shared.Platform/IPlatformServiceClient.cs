using Moosta.Shared.Platform.Models;
using System.Collections.Generic;

namespace Moosta.Shared.Platform
{
    public interface IPlatformServiceClient
    {
        #region User

        public Task<MoostaUser> GetMeAsync();

        public Task<MoostaUser> CreateUserAsync();

        #endregion

        #region Idea

        public Task<MoostaIdea> GetIdeaAsync(string id);

        public Task<IEnumerable<MoostaIdea>> GetIdeasAsync();

        public Task<MoostaIdea> CreateIdeaAsync();

        public Task<MoostaIdea> UpdateIdeaAsync(string id, MoostaIdea idea);

        #endregion

        public Task<MoostaCompletion> GetCompletionAsync(MoostaCompletionRequest request);
    }
}
