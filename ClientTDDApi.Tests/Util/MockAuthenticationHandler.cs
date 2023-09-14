using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace ClientTDDApi.Tests.Util
{
    public class MockAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public MockAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock
        ) : base(options, logger, encoder, clock) { }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new Claim[] {
                new Claim(ClaimTypes.Role, "worker"),
                new Claim(ClaimTypes.Role, "admin")
            };
            var identity = new ClaimsIdentity( claims, "Test" );
            var principal = new ClaimsPrincipal(identity);
            var ticked = new AuthenticationTicket(principal, "Test");
            return await Task.FromResult( AuthenticateResult.Success(ticked));
        }
    }
}
