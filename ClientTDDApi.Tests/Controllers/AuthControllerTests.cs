using ClientTDDApi.DTOs.Auth;
using ClientTDDApi.Interfaces;
using ClientTDDApi.Tests.Builders.Auth;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using ClientTDDApi.Services;
using Microsoft.AspNetCore.Mvc;
using ClientTDDApi.Entities;
using ClientTDDApi.Tests.Builders.Users;
using ClientTDDApi.Exceptions;

namespace ClientTDDApi.Tests.Controllers
{
    public class AuthControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly Mock<IAuthService> _authService;
        private readonly Mock<IUserRepository> _userRepository;
        
        public AuthControllerTests(WebApplicationFactory<Program> factory)
        {
            _authService = new Mock<IAuthService>();
            _userRepository = new Mock<IUserRepository>();
            _factory = factory.WithWebHostBuilder((builder) =>
            {
                builder.ConfigureServices((services) =>
                {
                    services.AddScoped((sp) => _authService.Object);
                    services.AddScoped((sp) => _userRepository.Object);
                });
            });
            _client = _factory.CreateClient();

            string existingEmail = UserRegisterDTOBuilder.RegisteredEmail;
            User? userNull = null;
            _userRepository.Setup((r) => r.FindByEmail(It.Is<string>((email) => email != existingEmail))).Returns(userNull);
            _userRepository.Setup((r) => r.FindByEmail(It.Is<string>((email) => email == existingEmail))).Returns(UserBuilder.AUser().Build());
        }

        [Fact]
        public async Task RegisterShouldReturnBadRequestWhenEmailIsNull()
        {
            UserRegisterDTO userRegisterDTO = UserRegisterDTOBuilder.AUserRegisterDTO().WithANullEmail().Build();

            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/auth/register", userRegisterDTO);
            ValidationProblemDetails validationResponse = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            validationResponse.Should().NotBeNull();
            validationResponse.Errors.Keys.Should().Contain("Email");
            _authService.Verify((s) => s.Register(It.IsAny<UserRegisterDTO>()), Times.Never());
        }

        [Fact]
        public async Task RegisterShouldReturnBadRequestWhenEmailIsBlank()
        {
            UserRegisterDTO userRegisterDTO = UserRegisterDTOBuilder.AUserRegisterDTO().WithABlankEmail().Build();

            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/auth/register", userRegisterDTO);
            ValidationProblemDetails validationResponse = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            validationResponse.Should().NotBeNull();
            validationResponse.Errors.Keys.Should().Contain("Email");
            _authService.Verify((s) => s.Register(It.IsAny<UserRegisterDTO>()), Times.Never());
        }

        [Fact]
        public async Task RegisterShouldReturnBadRequestWhenEmailIsInvalid()
        {
            UserRegisterDTO userRegisterDTO = UserRegisterDTOBuilder.AUserRegisterDTO().WithAnInvalidEmail().Build();

            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/auth/register", userRegisterDTO);
            ValidationProblemDetails validationResponse = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            validationResponse.Should().NotBeNull();
            validationResponse.Errors.Keys.Should().Contain("Email");
            _authService.Verify((s) => s.Register(It.IsAny<UserRegisterDTO>()), Times.Never());
        }

        [Fact]
        public async Task RegisterShouldReturnBadRequestWhenEmailIsAlreadyUsed()
        {
            UserRegisterDTO userRegisterDTO = UserRegisterDTOBuilder.AUserRegisterDTO().WithARegisterEmail().Build();

            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/auth/register", userRegisterDTO);
            ValidationProblemDetails? validationResponse = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            validationResponse.Should().NotBeNull();
            validationResponse.Errors.Keys.Should().Contain("Email");
            _authService.Verify((s) => s.Register(It.IsAny<UserRegisterDTO>()), Times.Never());
        }

        [Fact]
        public async Task RegisterShouldReturnBadRequestWhenPasswordIsNull()
        {
            UserRegisterDTO userRegisterDTO = UserRegisterDTOBuilder.AUserRegisterDTO().WithANullPassword().Build();

            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/auth/register", userRegisterDTO);
            ValidationProblemDetails? validationResponse = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            validationResponse.Should().NotBeNull();
            validationResponse.Errors.Keys.Should().Contain("Password");
            _authService.Verify((s) => s.Register(It.IsAny<UserRegisterDTO>()), Times.Never());
        }

        [Fact]
        public async Task RegisterShouldReturnBadRequestWhenPasswordIsToShort()
        {
            UserRegisterDTO userRegisterDTO = UserRegisterDTOBuilder.AUserRegisterDTO().WithAShortPassword().Build();

            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/auth/register", userRegisterDTO);
            ValidationProblemDetails? validationResponse = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            validationResponse.Should().NotBeNull();
            validationResponse.Errors.Keys.Should().Contain("Password");
            _authService.Verify((s) => s.Register(It.IsAny<UserRegisterDTO>()), Times.Never());
        }

        [Fact]
        public async Task RegisterShouldReturnBadRequestWhenPasswordsAreDifferente()
        {
            UserRegisterDTO userRegisterDTO = UserRegisterDTOBuilder.AUserRegisterDTO().WithDifferentPasswords().Build();

            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/auth/register", userRegisterDTO);
            ValidationProblemDetails? validationResponse = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            validationResponse.Should().NotBeNull();
            validationResponse.Errors.Keys.Should().Contain("ConfirmPassword");
            _authService.Verify((s) => s.Register(It.IsAny<UserRegisterDTO>()), Times.Never());
        }

        [Fact]
        public async Task RegisterShouldReturnNoContentWhenDataIsValid()
        {
            UserRegisterDTO userRegisterDTO = UserRegisterDTOBuilder.AUserRegisterDTO().Build();

            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/auth/register", userRegisterDTO);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            _authService.Verify((s) => s.Register(It.IsAny<UserRegisterDTO>()), Times.Once());
        }

        [Fact]
        public async Task LoginShouldReturnBadRequestWhenEmailIsNull()
        {
            LoginDTO loginDTO = new LoginDTO()
            {
                Password = "password",
                Email = null
            };

            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/auth/login", loginDTO);
            ValidationProblemDetails? validationResponse = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            validationResponse.Should().NotBeNull();
            validationResponse.Errors.Keys.Should().Contain("Email");
            _authService.Verify((s) => s.Login(It.IsAny<LoginDTO>()), Times.Never());
        }

        [Fact]
        public async Task LoginShouldReturnBadRequestWhenPasswordIsNull()
        {
            LoginDTO loginDTO = new LoginDTO()
            {
                Password = null,
                Email = "mail@gmail.com"
            };

            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/auth/login", loginDTO);
            ValidationProblemDetails? validationResponse = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            validationResponse.Should().NotBeNull();
            validationResponse.Errors.Keys.Should().Contain("Password");
            _authService.Verify((s) => s.Login(It.IsAny<LoginDTO>()), Times.Never());
        }

        [Fact]
        public async Task LoginShouldReturnUnhauthorizedWhenPasswordOrEmailAreIncorrect()
        {
            LoginDTO loginDTO = new LoginDTO()
            {
                Email = "mail@gmail.com",
                Password = "123456"
            };
            _authService.Setup((s) => s.Login(It.IsAny<LoginDTO>())).ThrowsAsync(new UnauthorizedException("Wrong"));

            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/auth/login", loginDTO);
            ExceptionResponse? exceptionResponse = await response.Content.ReadFromJsonAsync<ExceptionResponse>();

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            exceptionResponse.Should().NotBeNull();
            exceptionResponse.Message.Should().Be("Wrong");
            _authService.Verify((s) => s.Login(It.IsAny<LoginDTO>()), Times.Once());
        }

        [Fact]
        public async Task LoginShouldReturnTokenWhenPasswordIsCorrect()
        {
            LoginDTO loginDTO = new LoginDTO()
            {
                Email = "mail@gmail.com",
                Password = "123456"
            };
            TokenDTO tokenDTO = new TokenDTO()
            {
                Access_Token = "token"
            };
            _authService.Setup((s) => s.Login(It.IsAny<LoginDTO>())).ReturnsAsync(tokenDTO);

            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/auth/login", loginDTO);
            TokenDTO? content = await response.Content.ReadFromJsonAsync<TokenDTO>();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().NotBeNull();
            content.Access_Token.Should().Be("token");
        }
    }
}
