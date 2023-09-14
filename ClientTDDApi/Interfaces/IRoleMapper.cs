using ClientTDDApi.DTOs.Role;
using ClientTDDApi.Entities;

namespace ClientTDDApi.Interfaces
{
    public interface IRoleMapper
    {
        IEnumerable<RoleDTO> MapToRoleDTOs(IEnumerable<Role> roles);
    }
}
