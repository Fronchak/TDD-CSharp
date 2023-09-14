using ClientTDDApi.DTOs.Auth;
using ClientTDDApi.Entities;
using ClientTDDApi.Exceptions;
using ClientTDDApi.Interfaces;
using ClientTDDApi.Services;
using ClientTDDApi.Tests.Builders.Users;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientTDDApi.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepository;
        private readonly Mock<IPasswordEncoder> _passwordEncoder;
        private readonly Mock<ITokenService> _tokenService;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _userRepository = new Mock<IUserRepository>();
            _passwordEncoder = new Mock<IPasswordEncoder>();
            _tokenService = new Mock<ITokenService>();
            _authService = new AuthService(_userRepository.Object, _passwordEncoder.Object, _tokenService.Object);
        }

        [Fact]
        public async Task RegisterShouldSaveNewUserInTheDatabase()
        {
            string email = "mail@gmail.com";
            string password = "123";
            string salt = "456";
            string hashed = "789";
            UserRegisterDTO userRegisterDTO = new UserRegisterDTO()
            {
                Email = email,
                Password = password
            };
            _passwordEncoder.Setup((p) => p.GenerateSalt()).Returns(salt);
            _passwordEncoder.Setup((p) => p.HashPassword(password, salt)).Returns(hashed);

            await _authService.Register(userRegisterDTO);

            _userRepository.Verify((r) => r.Save(It.Is<User>((user) => user.Email == email && user.Password == hashed)), Times.Once());
            _userRepository.Verify((r) => r.SaveChangesAsync(), Times.Once());
        }

        [Fact]
        public async Task LoginShouldReturnUnauthorizedExceptionWhenEmailDoesntExists()
        {
            string nonExistingEmail = "mail@gmail.com";
            LoginDTO loginDTO = new LoginDTO() { Email = nonExistingEmail };
            User? userNull = null;
            _userRepository.Setup((r) => r.FindByEmail(nonExistingEmail)).Returns(userNull);

            Func<Task> action = async () => await _authService.Login(loginDTO);

            await action.Should().ThrowAsync<UnauthorizedException>();
            _userRepository.Verify((r) => r.FindByEmail(nonExistingEmail), Times.Once());
        }

        [Fact]
        public async Task LoginShouldReturnUnauthorizedExceptionWhenPasswordIsInvalid()
        {
            string existingEmail = "mail@gmail.com";
            string password = "123";
            User user = UserBuilder.AUser().Build();
            string hashed = user.Password;
            _userRepository.Setup((r) => r.FindByEmail(existingEmail)).Returns(user);
            _passwordEncoder.Setup((p) => p.VerifyPassword(password, hashed)).Returns(false);
            LoginDTO loginDTO = new LoginDTO()
            {
                Email = existingEmail,
                Password = password,
            };

            Func<Task> action = async () => await _authService.Login(loginDTO);

            await action.Should().ThrowAsync<UnauthorizedException>();
            _userRepository.Verify((r) => r.FindByEmail(existingEmail), Times.Once());
            _passwordEncoder.Verify((p) => p.VerifyPassword(password, hashed), Times.Once());
        }

        [Fact]
        public async Task LoginShouldReturnTokenWhenPasswordIsValid()
        {
            string existingEmail = "mail@gmail.com";
            string password = "123";
            User user = UserBuilder.AUser().Build();
            string hashed = user.Password;
            _userRepository.Setup((r) => r.FindByEmail(existingEmail)).Returns(user);
            _userRepository.Setup((r) => r.FindByIdAsync(user.Id)).ReturnsAsync(user);
            _passwordEncoder.Setup((p) => p.VerifyPassword(password, hashed)).Returns(true);
            LoginDTO loginDTO = new LoginDTO()
            {
                Email = existingEmail,
                Password = password,
            };
            string token = "token";
            _tokenService.Setup((s) => s.GenerateToken(user)).Returns(token);

            TokenDTO result = await _authService.Login(loginDTO);

            result.Access_Token.Should().Be(token);
            _userRepository.Verify((r) => r.FindByEmail(existingEmail), Times.Once());
            _passwordEncoder.Verify((p) => p.VerifyPassword(password, hashed), Times.Once());
        }
    }
}
