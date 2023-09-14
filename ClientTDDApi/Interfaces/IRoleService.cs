using ClientTDDApi.DTOs.Role;

namespace ClientTDDApi.Interfaces
{
    public interface IRoleService
    {
        Task<IEnumerable<RoleDTO>> FindAllAsync();
    }
}
