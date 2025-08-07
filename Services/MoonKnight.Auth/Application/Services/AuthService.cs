using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MoonKnight.Auth.Application.Dtos;
using MoonKnight.Auth.Application.Interfaces;

using MoonKnight.Auth.Domain.Entities;
using MoonKnight.Auth.Infrastructures.DbContexts;
using System.Security.Cryptography;

namespace MoonKnight.Auth.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly MoonKnightDbContext _context;
        private readonly IMapper _mapper;
        private readonly IEmailServices _emailService;
        private readonly IJwtTokenGenerator _jwtService;

        public AuthService(
            MoonKnightDbContext context,
            IMapper mapper,
            IEmailServices emailService,
            IJwtTokenGenerator jwtService)
        {
            _context = context;
            _mapper = mapper;
            _emailService = emailService;
            _jwtService = jwtService;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            if (dto.Password != dto.ConfirmPassword)
                throw new ArgumentException("Passwords do not match");

            if (await _context.Users.AnyAsync(x => x.Email == dto.Email))
                throw new InvalidOperationException("User already exists");

            var user = _mapper.Map<User>(dto);
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            user.TenantId = Guid.NewGuid(); // Adjust as needed
            user.EmailVerificationToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            user.EmailVerificationTokenExpires = DateTime.UtcNow.AddHours(24);

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var verifyLink = $"https://your-api.com/api/auth/verify-email?token={user.EmailVerificationToken}";
            var message = $"Click here to verify your email: <a href=\"{verifyLink}\">Verify</a>";
            await _emailService.SendAsync(user.Email, "Verify your email", message);

            return new AuthResponseDto
            {
                Email = user.Email,
                UserId = user.Id.ToString(),
                Message = "Registration successful. Please check your email to verify your account."
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid credentials");

            if (!user.EmailConfirmed)
                throw new UnauthorizedAccessException("Email not verified");

            var accessToken = _jwtService.GenerateToken(user);
            var refreshToken = GenerateAndSaveRefreshToken(user);

            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                Email = user.Email,
                UserId = user.Id.ToString(),
                Message = "Login successful"
            };
        }

        public async Task ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == dto.Email);
            if (user == null) return;

            var resetToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var tokenEntry = new PasswordResetToken
            {
                Token = resetToken,
                UserId = user.Id,
                Created = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddHours(1)
            };

            await _context.PasswordResetTokens.AddAsync(tokenEntry);
            await _context.SaveChangesAsync();

            var resetLink = $"https://your-api.com/api/auth/reset-password?token={resetToken}";
            var message = $"Click here to reset your password: <a href=\"{resetLink}\">Reset Password</a>";
            await _emailService.SendAsync(user.Email, "Reset your password", message);
        }

        public async Task ResetPasswordAsync(ResetPasswordDto dto)
        {
            if (dto.NewPassword != dto.ConfirmPassword)
                throw new ArgumentException("Passwords do not match");

            var tokenEntry = await _context.PasswordResetTokens
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Token == dto.ResetToken);

            if (tokenEntry == null || tokenEntry.Used || tokenEntry.Expires < DateTime.UtcNow)
                throw new InvalidOperationException("Invalid or expired token");

            tokenEntry.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            tokenEntry.Used = true;

            await _context.SaveChangesAsync();
        }

        public async Task VerifyEmailAsync(string token)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.EmailVerificationToken == token);
            if (user == null || user.EmailVerificationTokenExpires < DateTime.UtcNow)
                throw new InvalidOperationException("Invalid or expired verification link");

            user.EmailConfirmed = true;
            user.EmailVerificationToken = null;
            user.EmailVerificationTokenExpires = null;

            await _context.SaveChangesAsync();
        }

        public async Task LogoutAsync(string refreshToken)
        {
            var token = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == refreshToken);
            if (token != null)
            {
                token.Revoked = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(string oldRefreshToken)
        {
            var token = await _context.RefreshTokens
                .Include(r => r.User)
                .FirstOrDefaultAsync(x => x.Token == oldRefreshToken);

            if (token == null || token.Revoked != null || token.Expires < DateTime.UtcNow)
                throw new UnauthorizedAccessException("Invalid refresh token");

            token.Revoked = DateTime.UtcNow;

            var newAccessToken = _jwtService.GenerateToken(token.User);
            var newRefreshToken = GenerateAndSaveRefreshToken(token.User);

            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken.Token,
                Email = token.User.Email,
                UserId = token.User.Id.ToString(),
                Message = "Token refreshed successfully"
            };
        }

        // Helper to generate and save a new refresh token
        private RefreshToken GenerateAndSaveRefreshToken(User user)
        {
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                UserId = user.Id,
                Created = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(7) // or your preferred expiry
            };
            _context.RefreshTokens.Add(refreshToken);
            return refreshToken;
        }
    }
}
