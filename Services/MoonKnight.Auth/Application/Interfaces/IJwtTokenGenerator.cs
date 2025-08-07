using MoonKnight.Auth.Domain.Entities;

namespace MoonKnight.Auth.Application.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(User user);
        RefreshToken GenerateRefreshToken();
    }
}
