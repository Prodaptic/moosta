using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;
using Moosta.Shared.Platform;
using Moosta.Shared.Platform.Models;
using Moosta.Web.Clients;

namespace Moosta.Web.Models
{
    public class MoostaAccountFactory : AccountClaimsPrincipalFactory<MoostaUserAccount>
    {
        public IHttpClientFactory factory { get; set; }
        public MoostaAccountFactory(NavigationManager navigationManager,
            IHttpClientFactory factory,
            IAccessTokenProviderAccessor accessor) : base(accessor)
        {
            this.factory = factory;
        }

        public override async ValueTask<ClaimsPrincipal> CreateUserAsync(
            MoostaUserAccount account, RemoteAuthenticationUserOptions options)
        {
            var initialUser = await base.CreateUserAsync(account, options);

            if (initialUser.Identity.IsAuthenticated)
            {
                var client = factory.CreateClient("PlatformServiceClient");
                var response = await client.GetAsync("/user/me");

                var moostaUser = new MoostaUser();
                if(response.IsSuccessStatusCode)
                    moostaUser = await response.Content.ReadFromJsonAsync<MoostaUser>();

                if (string.IsNullOrEmpty(moostaUser.Id)) //if we don't have an id its a new user
                { 
                    response = await client.PostAsync("/user", null);
                    moostaUser = await response.Content.ReadFromJsonAsync<MoostaUser>();
                }

                ((ClaimsIdentity)initialUser.Identity)
                    .AddClaim(new Claim("roles", moostaUser.Roles));
            }

            return initialUser;
        }
    }
}