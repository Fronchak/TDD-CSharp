using ClientTDDApi.DTOs.Role;
using ClientTDDApi.Entities;
using ClientTDDApi.Interfaces;

namespace ClientTDDApi.Mappers
{
    public class RoleMapper : IRoleMapper
    {
        public IEnumerable<RoleDTO> MapToRoleDTOs(IEnumerable<Role> roles)
        {
            return roles.Select((role) => new RoleDTO(role.Id, role.Name));
        }
    }
}
