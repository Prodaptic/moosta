using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace Moosta.Web.Models
{
    public class MoostaAuthorizationMessageHandler : AuthorizationMessageHandler
    {
        IConfiguration _configuration;
        public MoostaAuthorizationMessageHandler(IAccessTokenProvider provider,
            NavigationManager navigationManager,
            IConfiguration configuration)
            : base(provider, navigationManager)
        {
            _configuration = configuration;
            ConfigureHandler(
                authorizedUrls: new[] { _configuration["PlatformService:Url"] },
                scopes: new[] { _configuration["PlatformService:Scope"] });
        }
    }
}
