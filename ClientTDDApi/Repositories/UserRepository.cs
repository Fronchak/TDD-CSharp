using ClientTDDApi.Data;
using ClientTDDApi.Entities;
using ClientTDDApi.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClientTDDApi.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;

        public UserRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> FindAllAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public User? FindByEmail(string email)
        {
            return _context.Users.FirstOrDefault(x => x.Email == email);    
        }

        public async Task<User?> FindByIdAsync(int id)
        {
            return await _context.Users
                .Where((user) => user.Id == id)
                .Include((user) => user.UserRoles)
                .ThenInclude((userRole) => userRole.Role)
                .FirstOrDefaultAsync();
        }

        public User Save(User user)
        {
            _context.Add(user);
            return user;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
