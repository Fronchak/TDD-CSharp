using ClientTDDApi.Data;
using ClientTDDApi.DTOs.Auth;
using ClientTDDApi.Entities;
using ClientTDDApi.Tests.Builders.Auth;
using ClientTDDApi.Tests.Builders.Users;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using ClientTDDApi.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace ClientTDDApi.Tests.Controllers
{
    public class AuthControllerIT : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly DataContext _context;

        public AuthControllerIT(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder((builder) =>
            {
                builder.ConfigureServices((services) =>
                {
                    var dbContextDescriptor = services.SingleOrDefault((d) => d.ServiceType == typeof(DbContextOptions<DataContext>));
                    services.Remove(dbContextDescriptor);
                    var dbConnectionDescriptor = services.SingleOrDefault((d) => d.ServiceType == typeof(DbConnection));
                    services.Remove(dbConnectionDescriptor);
                    services.AddDbContextPool<DataContext>((options) =>
                    {
                        options.UseInMemoryDatabase("auth_controller_it");
                    });
                });
            });
            _client = _factory.CreateClient();
            var scope = _factory.Services.CreateScope();
            _context = scope.ServiceProvider.GetRequiredService<DataContext>();
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
        }

        [Fact]
        public async Task RegisterShouldReturnBadRequestWhenEmailIsAlreadyUsed()
        {
            User user = UserBuilder.AUser().Build();
            user.UserRoles = new List<UserRole>();
            string registerEmail = user.Email;
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            UserRegisterDTO userRegisterDTO = UserRegisterDTOBuilder.AUserRegisterDTO().WithEmail(registerEmail).Build();

            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/auth/register", userRegisterDTO);
            ValidationProblemDetails validationResponse = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            _context.Users.Count().Should().Be(1);
            validationResponse.Should().NotBeNull();
            validationResponse.Errors.Keys.Should().Contain("Email");
        }

        [Fact]
        public async Task RegisterShouldReturnNoContentWhenDataIsValid()
        {
            UserRegisterDTO userRegisterDTO = UserRegisterDTOBuilder.AUserRegisterDTO().Build();

            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/auth/register", userRegisterDTO);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            _context.Users.Count().Should().Be(1);
            User user = _context.Users.First();
            (user.Password.Equals(userRegisterDTO.Password)).Should().BeFalse();
        }

        [Fact]
        public async Task LoginShouldReturnUnhauthorizedWhenPasswordIsIncorrect()
        {
            UserRegisterDTO userRegisterDTO = UserRegisterDTOBuilder.AUserRegisterDTO().Build();
            string email = userRegisterDTO.Email;
            string password = userRegisterDTO.Password;
            string wrongPassword = password + ".";
            LoginDTO loginDTO = new LoginDTO() { Email = email, Password = wrongPassword };

            await _client.PostAsJsonAsync("/api/auth/register", userRegisterDTO);
            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/auth/login", loginDTO);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task LoginShouldReturnTokenWhenPasswordIsCorrect()
        {
            UserRegisterDTO userRegisterDTO = UserRegisterDTOBuilder.AUserRegisterDTO().Build();
            string email = userRegisterDTO.Email;
            string password = userRegisterDTO.Password;
            LoginDTO loginDTO = new LoginDTO() { Email = email, Password = password };

            await _client.PostAsJsonAsync("/api/auth/register", userRegisterDTO);
            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/auth/login", loginDTO);
            TokenDTO? tokenDTO = await response.Content.ReadFromJsonAsync<TokenDTO>();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            tokenDTO.Should().NotBeNull();
            tokenDTO.Access_Token.Length.Should().BeGreaterThan(20);
        }
    }
}
