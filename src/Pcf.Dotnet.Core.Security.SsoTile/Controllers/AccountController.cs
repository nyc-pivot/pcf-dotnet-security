using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Pcf.Dotnet.Core.Security.SsoTile.Controllers
{
    public class AccountController : Controller
    {
        private readonly IOptions<CloudFoundryServicesOptions> cloudFoundryServicesOptions;
        private readonly OpenIdConnectPostConfigureOptions openIdConnectPostConfigureOptions;
        private readonly IOptionsMonitorCache<OpenIdConnectOptions> optionsCache;

        public AccountController(
            IOptions<CloudFoundryServicesOptions> cloudFoundryServicesOptions,
            OpenIdConnectPostConfigureOptions openIdConnectPostConfigureOptions,
            IOptionsMonitorCache<OpenIdConnectOptions> optionsCache)
        {
            this.cloudFoundryServicesOptions = cloudFoundryServicesOptions;
            this.openIdConnectPostConfigureOptions = openIdConnectPostConfigureOptions;
            this.optionsCache = optionsCache;
        }

        public async Task Login(string returnUrl = "/")
        {
            var servicesList = cloudFoundryServicesOptions.Value.ServicesList;
            var ssoService = servicesList.SingleOrDefault(s => s.Label == "p-identity");

            var domain = ssoService.Credentials["auth_domain"].Value;
            var clientId = ssoService.Credentials["client_id"].Value;
            var clientSecret = ssoService.Credentials["client_secret"].Value;

            var options = new OpenIdConnectOptions()
            {
                Authority = $"https://{domain}",
                ClientId = clientId,
                ClientSecret = clientSecret,
                ResponseType = "code",
                CallbackPath = new PathString("/callback"),
                ClaimsIssuer = "Auth0"
            };

            options.Events = new OpenIdConnectEvents
            {
                OnRedirectToIdentityProviderForSignOut = (context) =>
                {
                    var logoutUri = $"https://{domain}/v2/logout?client_id={clientId}";

                    var postLogoutUri = context.Properties.RedirectUri;
                    if (!string.IsNullOrEmpty(postLogoutUri))
                    {
                        if (postLogoutUri.StartsWith("/"))
                        {
                            // transform to absolute
                            var request = context.Request;
                            postLogoutUri = request.Scheme + "://" + request.Host + request.PathBase + postLogoutUri;
                        }
                        logoutUri += $"&returnTo={ Uri.EscapeDataString(postLogoutUri)}";
                    }

                    context.Response.Redirect(logoutUri);
                    context.HandleResponse();

                    return Task.CompletedTask;
                }
            };

            openIdConnectPostConfigureOptions.PostConfigure("Auth0", options);
            optionsCache.TryAdd("Auth0", options);

            await HttpContext.ChallengeAsync("Auth0", new AuthenticationProperties() { RedirectUri = returnUrl });
        }

        [Authorize]
        public async Task Logout()
        {
            await HttpContext.SignOutAsync("Auth0", new AuthenticationProperties
            {
                // Indicate here where Auth0 should redirect the user after a logout.
                // Note that the resulting absolute Uri must be whitelisted in the
                // **Allowed Logout URLs** settings for the app.
                RedirectUri = Url.Action("Index", "Home")
            });
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}