using ClientTDDApi.Entities;

namespace ClientTDDApi.Interfaces
{
    public interface IRoleRepository
    {
        Task<IEnumerable<Role>> FindAllAsync();
    }
}
