using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Moosta.Web;
using MudBlazor.Services;
using System.Net.Http;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Moosta.Web.Clients;
using Moosta.Web.Models;
using Microsoft.Extensions.DependencyInjection;
using Moosta.Shared.Platform;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<MoostaAuthorizationMessageHandler>();

builder.Services.AddHttpClient<PlatformServiceClient>("PlatformServiceClient",
    client => client.BaseAddress = new Uri(builder.Configuration["PlatformService:Url"]))
.AddHttpMessageHandler<MoostaAuthorizationMessageHandler>();

builder.Services.AddTransient(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("PlatformServiceClient"));

builder.Services.AddMsalAuthentication<RemoteAuthenticationState, MoostaUserAccount>(options =>
{
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
    options.ProviderOptions.DefaultAccessTokenScopes.Add("openid");
    options.ProviderOptions.DefaultAccessTokenScopes.Add("offline_access");
    options.ProviderOptions.DefaultAccessTokenScopes.Add(builder.Configuration["PlatformService:Scope"]);
    options.ProviderOptions.LoginMode = "redirect";
    options.UserOptions.RoleClaim = "roles";
}).AddAccountClaimsPrincipalFactory<RemoteAuthenticationState, MoostaUserAccount, MoostaAccountFactory>();

builder.Services.AddMudServices();

await builder.Build().RunAsync();
