using ClientTDDApi.DTOs.Role;
using ClientTDDApi.Entities;
using ClientTDDApi.Interfaces;

namespace ClientTDDApi.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IRoleMapper _roleMapper;

        public RoleService(IRoleRepository roleRepository, IRoleMapper roleMapper)
        {
            _roleRepository = roleRepository;
            _roleMapper = roleMapper;
        }

        public async Task<IEnumerable<RoleDTO>> FindAllAsync()
        {
            IEnumerable<Role> roles = await _roleRepository.FindAllAsync();
            IEnumerable<RoleDTO> dtos = _roleMapper.MapToRoleDTOs(roles);
            return dtos;
        } 
    }
}
