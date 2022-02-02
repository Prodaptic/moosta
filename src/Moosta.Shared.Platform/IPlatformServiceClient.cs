using Moosta.Shared.Platform.Models;

namespace Moosta.Shared.Platform
{
    public interface IPlatformServiceClient
    {
        public MoostaUser GetUser(string id);

        public Task<MoostaUser> GetMeAsync();

        public MoostaUser UpdateUser(MoostaUser user);

        public Task<MoostaUser> CreateUserAsync();

        public Task<MoostaCompletion> GetCompletionAsync(MoostaCompletionRequest request);
    }
}
