using ClientTDDApi.Data;
using ClientTDDApi.Entities;
using ClientTDDApi.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClientTDDApi.Repositories
{
    public class ClientRepository : IClientRepository
    {
        private readonly DataContext _context;

        public ClientRepository(DataContext context)
        {
            _context = context;
        }

        public void Delete(Client client)
        {
            _context.Remove(client);
        }

        public async Task<IEnumerable<Client>> FindAllAsync()
        {
            return await _context.Clients.ToListAsync();
        }

        public Client? FindByEmail(string email)
        {
            return _context.Clients
                .Where((client) => client.Email.Equals(email))
                .FirstOrDefault();
        }

        public async Task<Client?> FindByIdAsync(int id)
        {
            return await _context.Clients.FindAsync(id);
        }

        public void Save(Client client)
        {
            _context.Clients.Add(client);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void Update(Client client)
        {
            _context.Clients.Update(client);
        }
    }
}
