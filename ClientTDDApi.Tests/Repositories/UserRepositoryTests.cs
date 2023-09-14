using ClientTDDApi.Data;
using ClientTDDApi.Entities;
using ClientTDDApi.Repositories;
using ClientTDDApi.Tests.Builders.Users;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientTDDApi.Tests.Repositories
{
    public class UserRepositoryTests
    {
        private readonly DataContext _context;
        private readonly UserRepository _userRepository;

        public UserRepositoryTests()
        {
            DbContextOptions<DataContext> options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase("user_repository_tests")
                .Options;
            _context = new DataContext(options);
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
            _userRepository = new UserRepository(_context);
        }

        [Fact]
        public void FindByEmailShouldReturnNullWhenEmailDoesNotExist()
        {
            string nonExistingEmail = "aaa@gmail.com";
            User user = UserBuilder.AUser().Build();
            user.UserRoles = new List<UserRole>();
            _context.Users.Add(user);
            _context.SaveChanges(); 

            User? result = _userRepository.FindByEmail(nonExistingEmail);

            result.Should().BeNull();
        }

        [Fact]
        public void FindByEmailShouldReturnUserWhenIdExists()
        {
            User user = UserBuilder.AUser().Build();
            user.UserRoles = new List<UserRole>();
            _context.Users.Add(user);
            _context.SaveChanges();

            User? result = _userRepository.FindByEmail(user.Email);

            result.Should().NotBeNull();
            result.Id.Should().Be(user.Id);
        }

        [Fact]
        public async Task FindByIdShouldReturnUserWithRolesWhenIdExists()
        {
            User user = new User()
            {
                Id = 1,
                Email = "user@gmail.com",
                Password = "123456"
            };
            Role admin = new Role()
            {
                Name = "admin"
            };
            Role worker = new Role()
            {
                Name = "worker"
            };
            IEnumerable<UserRole> userRoles = new List<UserRole>()
            {
                new UserRole()
                {
                    Role = admin,
                    User = user
                },
                new UserRole()
                {
                    Role = worker,
                    User = user
                }
            };
            _context.UserRoles.AddRange(userRoles);
            await _context.SaveChangesAsync();

            User? result = await _userRepository.FindByIdAsync(1);

            result.Should().NotBeNull();
            IEnumerable<UserRole> userRolesResult = result.UserRoles;
            userRolesResult.Should().HaveCount(2);
            userRolesResult.Should().Contain((userRole) => userRole.Role.Name == "admin");
            userRolesResult.Should().Contain((userRole) => userRole.Role.Name == "worker");
        }
    }
}
