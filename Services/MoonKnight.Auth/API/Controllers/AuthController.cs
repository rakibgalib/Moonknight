// File: MoonKnight.Auth.Controllers/AuthController.cs
using AutoMapper;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using MoonKnight.Auth.Application.Dtos;
using MoonKnight.Auth.Application.Interfaces;
using MoonKnight.Auth.Domain.Entities;
using MoonKnight.Auth.Infrastructures.DbContexts;
using MoonKnight.Auth.Infrastructures.Services;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace MoonKnight.Auth.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly MoonKnightDbContext _context;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IMapper _mapper;
        private readonly IEmailServices _emailService;
        public AuthController(MoonKnightDbContext context, IJwtTokenGenerator jwtTokenGenerator,IMapper mapper,IEmailServices emailService)
        {
            _mapper = mapper;
            _context = context;
            _emailService = emailService;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        // Registration endpoint
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (registerDto.Password != registerDto.ConfirmPassword)
                return BadRequest("Passwords do not match");

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == registerDto.Email);

            if (existingUser != null)
                return Conflict("User already exists");

            // Map RegisterDto to User (ignoring PasswordHash and TenantId)
            var user = _mapper.Map<User>(registerDto);

            // Hash the password manually
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            // Assign TenantId manually (if applicable)
            user.TenantId = Guid.NewGuid(); // Or assign proper tenant id here
            user.EmailVerificationToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            user.EmailVerificationTokenExpires = DateTime.UtcNow.AddHours(24);
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            var verifyLink = $"https://localhost:5152/api/auth/verify-email?token={user.EmailVerificationToken}";
            var message = $"Please click the following link to verify your email: <a href=\"{verifyLink}\">Verify</a>";

            await _emailService.SendAsync(user.Email, "Email Verification", message);

            return Ok(new { message = "Registration successful" });
        }

        [EnableRateLimiting("login-policy")]
        // Login endpoint
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null)
                return Unauthorized("Invalid credentials");
            if (!user.EmailConfirmed)
                return Unauthorized("Email not confirmed");
            // Verify password using BCrypt
            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                return Unauthorized("Invalid credentials");
            var existingTokens = _context.RefreshTokens
                .Where(rt => rt.UserId == user.Id && rt.Revoked == null && rt.Expires > DateTime.UtcNow);

            foreach (var token in existingTokens)
            {
                token.Revoked = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();

            // Generate JWT token (assuming you have a JwtTokenGenerator service)
            var accessToken = _jwtTokenGenerator.GenerateToken(user);
            var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();
            refreshToken.UserId = user.Id;
            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();
            return Ok(new
            {
                message = "Login successful",
                accessToken,
                refreshToken = refreshToken.Token
            });
           
        }
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] string token)
        {
            var oldToken = await _context.RefreshTokens.Include(r => r.User)
                    .FirstOrDefaultAsync(r => r.Token == token);

            if (oldToken == null || !oldToken.IsActive)
                return Unauthorized("Invalid refresh token");

            oldToken.Revoked = DateTime.UtcNow;

            var newRefresh = _jwtTokenGenerator.GenerateRefreshToken();
            newRefresh.UserId = oldToken.UserId;
            var newAccessToken = _jwtTokenGenerator.GenerateToken(oldToken.User);

            await _context.RefreshTokens.AddAsync(newRefresh);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                accessToken = newAccessToken,
                refreshToken = newRefresh.Token
            });
        }
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] string token)
        {
            var refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == token);
            if (refreshToken == null) return Ok();

            refreshToken.Revoked = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Logged out" });
        }

        [HttpPost("forgot")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == dto.Email);
            if (user == null)
                return Ok(new { message = "If an account exists, a reset link has been sent." });

            // generate token
            var resetToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            // create entity using AutoMapper (optional) or manual:
            var resetEntry = _mapper.Map<PasswordResetToken>(dto);
            resetEntry.Token = resetToken;
            resetEntry.UserId = user.Id;
            resetEntry.Created = DateTime.UtcNow;
            resetEntry.Expires = DateTime.UtcNow.AddHours(1);

            await _context.PasswordResetTokens.AddAsync(resetEntry);
            await _context.SaveChangesAsync();

            // TODO: send resetEntry.Token to email
            return Ok(new { message = "Reset link sent" });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (dto.NewPassword != dto.ConfirmPassword)
                return BadRequest("Passwords do not match");

            var tokenEntry = await _context.PasswordResetTokens
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Token == dto.ResetToken);

            if (tokenEntry == null || tokenEntry.Used || tokenEntry.Expires < DateTime.UtcNow)
                return BadRequest("Token expired/invalid");

            // hash new password
            tokenEntry.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            tokenEntry.Used = true;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Password reset successfully" });
        }


        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] string token)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.EmailVerificationToken == token);

            if (user == null || user.EmailVerificationTokenExpires < DateTime.UtcNow)
                return BadRequest("Invalid or expired token");

            user.EmailConfirmed = true;
            user.EmailVerificationToken = null;
            user.EmailVerificationTokenExpires = null;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Email verified successfully" });
        }
    }
}
