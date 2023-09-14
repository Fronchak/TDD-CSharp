using ClientTDDApi.DTOs.Auth;
using ClientTDDApi.Entities;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace ClientTDDApi.Tests.Util
{
    public class TokenUtil
    {
        public async static Task<string> GetToken(HttpClient client, string username, string password)
        {
            LoginDTO loginDTO = new LoginDTO()
            {
                Email = username,
                Password = password
            };

            HttpResponseMessage? loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginDTO);
            TokenDTO? tokenDTO = await loginResponse.Content.ReadFromJsonAsync<TokenDTO>();

            tokenDTO.Should().NotBeNull();
            return tokenDTO.Access_Token;
        }
    }
}
