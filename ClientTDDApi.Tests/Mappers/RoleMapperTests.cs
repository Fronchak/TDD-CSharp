using ClientTDDApi.DTOs.Role;
using ClientTDDApi.Entities;
using ClientTDDApi.Mappers;
using ClientTDDApi.Tests.Builders.RoleBuilders;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientTDDApi.Tests.Mappers
{
    public class RoleMapperTests
    {
        private readonly RoleMapper _roleMapper;

        public RoleMapperTests()
        {
            _roleMapper = new RoleMapper();
        }

        [Fact]
        public void MapToRoleDTOsShouldMapCorrectly()
        {
            IEnumerable<Role> roles = RoleBuilder.BuildRoles();

            IEnumerable<RoleDTO> result = _roleMapper.MapToRoleDTOs(roles);
            List<RoleDTO> list = result.ToList();

            result.Count().Should().Be(roles.Count());
            result.Should().Contain((dto) => dto.Id == RoleBuilder.Id && dto.Name == RoleBuilder.Name);
            result.Should().Contain((dto) => dto.Id == RoleBuilder.SecondaryId && dto.Name == RoleBuilder.SecondaryName);
        }
    }
}
