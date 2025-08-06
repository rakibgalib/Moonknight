using MoonKnight.Auth.Domain.Entities;

namespace MoonKnight.Auth.Infrastructures.Interfaces.Services
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(User user);
        RefreshToken GenerateRefreshToken();
    }
}
