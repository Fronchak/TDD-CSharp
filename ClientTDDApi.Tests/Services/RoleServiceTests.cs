using ClientTDDApi.DTOs.Role;
using ClientTDDApi.Entities;
using ClientTDDApi.Interfaces;
using ClientTDDApi.Services;
using ClientTDDApi.Tests.Builders.RoleBuilders;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientTDDApi.Tests.Services
{
    public class RoleServiceTests
    {
        private Mock<IRoleRepository> _roleRepository;
        private Mock<IRoleMapper> _roleMapper;
        private readonly RoleService _roleService;

        public RoleServiceTests()
        {
            _roleRepository = new Mock<IRoleRepository>();
            _roleMapper = new Mock<IRoleMapper>();
            _roleService = new RoleService(_roleRepository.Object, _roleMapper.Object);
        }

        [Fact]
        public async Task FindAllShouldReturnRoleDTOList()
        {
            IEnumerable<Role> roles = RoleBuilder.BuildRoles();
            IEnumerable<RoleDTO> roleDTOs = RoleDTOBuilder.BuildRoleDTOs(); 
            _roleRepository.Setup((r) => r.FindAllAsync()).ReturnsAsync(roles);
            _roleMapper.Setup((m) => m.MapToRoleDTOs(roles)).Returns(roleDTOs);

            IEnumerable<RoleDTO> result = await _roleService.FindAllAsync();

            result.Should().HaveCount(roleDTOs.Count());
            _roleRepository.Verify((r) => r.FindAllAsync(), Times.Once());
            _roleMapper.Verify((m) => m.MapToRoleDTOs(roles), Times.Once());
        }
    }
}
