using ClientTDDApi.Data;
using ClientTDDApi.Entities;
using ClientTDDApi.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClientTDDApi.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly DataContext _context;

        public RoleRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Role>> FindAllAsync()
        {
            return await _context.Roles.ToListAsync();
        }
    }
}
