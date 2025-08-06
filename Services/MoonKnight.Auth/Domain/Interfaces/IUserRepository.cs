using MoonKnight.Auth.Domain.Entities;

namespace MoonKnight.Auth.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetByEmailAsync(string email);
        Task AddAsync(User user);
    }
}
