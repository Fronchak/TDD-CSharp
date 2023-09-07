using ClientTDDApi.Entities;

namespace ClientTDDApi.Interfaces
{
    public interface IClientRepository
    {
        Task<Client?> FindByIdAsync(int id);

        Task<IEnumerable<Client>> FindAllAsync();

        void Save(Client client);

        void Update(Client client);

        void Delete(Client client);

        Task SaveChangesAsync();
    }
}
