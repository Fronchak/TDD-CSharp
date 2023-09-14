using ClientTDDApi.Entities;

namespace ClientTDDApi.Interfaces
{
    public interface IUserRepository
    {
        User Save(User user);

        Task<User?> FindByIdAsync(int id);

        Task<IEnumerable<User>> FindAllAsync();

        User? FindByEmail(string email);

        Task SaveChangesAsync();
    }
}
