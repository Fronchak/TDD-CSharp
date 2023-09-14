using ClientTDDApi.DTOs.Role;
using ClientTDDApi.DTOs.User;
using ClientTDDApi.Entities;
using ClientTDDApi.Interfaces;
using ClientTDDApi.Mappers;
using ClientTDDApi.Tests.Builders.RoleBuilders;
using ClientTDDApi.Tests.Builders.Users;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientTDDApi.Tests.Mappers
{
    public class UserMapperTest
    {
        private readonly Mock<IRoleMapper> _roleMapper;
        private readonly UserMapper _userMapper;

        public UserMapperTest()
        {
            _roleMapper = new Mock<IRoleMapper>();
            _userMapper = new UserMapper(_roleMapper.Object);
        }

        [Fact]
        public void MapToUserDTOShouldMapCorrectly()
        {
            User user = UserBuilder.AUser().Build();
            IEnumerable<RoleDTO> roleDTOs = RoleDTOBuilder.BuildRoleDTOs();
            _roleMapper.Setup((m) => m.MapToRoleDTOs(It.IsAny<IEnumerable<Role>>())).Returns(roleDTOs);

            UserDTO result = _userMapper.MapToUserDTO(user);

            result.Id.Should().Be(user.Id);
            result.Email.Should().Be(user.Email);
            result.Roles.Count().Should().Be(roleDTOs.Count());
            result.Roles.Should().Contain((r) => r.Id == RoleDTOBuilder.Id && r.Name == RoleDTOBuilder.Name);
            result.Roles.Should().Contain((r) => r.Id == RoleDTOBuilder.SecondaryId && r.Name == RoleDTOBuilder.SecondaryName);
            _roleMapper.Verify((m) => m.MapToRoleDTOs(It.IsAny<IEnumerable<Role>>()), Times.Once());
        }

        [Fact]
        public void MapToUserSimpleDTOsShouldMapCorrectly()
        {
            IEnumerable<User> users = UserBuilder.BuildUsers();

            IEnumerable<UserSimpleDTO> result = _userMapper.MapToUserSimpleDTOs(users);

            result.Should().HaveCount(users.Count());
            result.Should().Contain((u) => u.Id == UserBuilder.Id && u.Email == UserBuilder.Email);
            result.Should().Contain((u) => u.Id == UserBuilder.SecondaryId && u.Email == UserBuilder.SecondaryEmail);
        }
    }
}
