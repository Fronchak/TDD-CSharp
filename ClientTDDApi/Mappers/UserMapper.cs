using ClientTDDApi.DTOs.Role;
using ClientTDDApi.DTOs.User;
using ClientTDDApi.Entities;
using ClientTDDApi.Interfaces;

namespace ClientTDDApi.Mappers
{
    public class UserMapper
    {
        private readonly IRoleMapper _roleMapper;

        public UserMapper(IRoleMapper roleMapper)
        {
            _roleMapper = roleMapper;
        }

        public UserDTO MapToUserDTO(User user)
        {
            UserDTO dto = new UserDTO(user.Id, user.Email);
            IEnumerable<Role> roles = user.UserRoles.Select((r) => r.Role);
            IEnumerable<RoleDTO> roleDTOs = _roleMapper.MapToRoleDTOs(roles);
            dto.Roles = roleDTOs;
            return dto;
        }

        public IEnumerable<UserSimpleDTO> MapToUserSimpleDTOs(IEnumerable<User> users)
        {
            return users.Select((u) => new UserSimpleDTO(u.Id, u.Email));
        }
    }
}
