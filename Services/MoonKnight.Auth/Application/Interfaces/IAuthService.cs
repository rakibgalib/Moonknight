using MoonKnight.Auth.Application.Dtos;

namespace MoonKnight.Auth.Application.Interfaces
{
    public interface IAuthService
    {
        //Task<string> RegisterAsync(RegisterDto dto);
        //Task LoginAsync(LoginDto dto);
        //Task LogoutAsync(string refreshToken);
        //Task RefreshTokenAsync(string token);
        //Task VerifyEmailAsync(string token);
        //Task ForgotPasswordAsync(ForgotPasswordDto dto);
        //Task ResetPasswordAsync(ResetPasswordDto dto);

        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task ForgotPasswordAsync(ForgotPasswordDto dto);
        Task ResetPasswordAsync(ResetPasswordDto dto);
        Task VerifyEmailAsync(string token);
        Task LogoutAsync(string refreshToken);
        Task<AuthResponseDto> RefreshTokenAsync(string oldRefreshToken);
    }
}
