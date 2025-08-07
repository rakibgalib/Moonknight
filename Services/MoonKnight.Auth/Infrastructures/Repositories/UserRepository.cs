using Microsoft.EntityFrameworkCore;
using MoonKnight.Auth.Application.Interfaces;
using MoonKnight.Auth.Domain.Entities;
using MoonKnight.Auth.Infrastructures.DbContexts;

namespace MoonKnight.Auth.Infrastructures.Repositories
{
    public class UserRepository: IUserRepository
    {
        private readonly MoonKnightDbContext _context;

        public UserRepository(MoonKnightDbContext context)
        {
            _context = context;
        }

        public async Task<User> GetByEmailAsync(string email) =>
            await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }
    }
}
