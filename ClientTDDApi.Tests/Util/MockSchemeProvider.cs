using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientTDDApi.Tests.Util
{
    public class MockSchemeProvider : AuthenticationSchemeProvider
    {
        public MockSchemeProvider(IOptions<AuthenticationOptions> options) : base(options)
        {
        }

        protected MockSchemeProvider(
            IOptions<AuthenticationOptions> options,
            IDictionary<string, AuthenticationScheme> schemes
        ) : base(options, schemes) {}

        public override Task<AuthenticationScheme> GetSchemeAsync(string name)
        {
            if (name == "Test")
            {
                var scheme = new AuthenticationScheme(
                    "Test",
                    "Test",
                    typeof(MockAuthenticationHandler)
                );
                return Task.FromResult(scheme);
            }

            return base.GetSchemeAsync(name);
        }
    }
}
