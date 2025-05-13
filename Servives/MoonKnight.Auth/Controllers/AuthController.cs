// File: MoonKnight.Auth.Controllers/AuthController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoonKnight.Auth.Dtos;
using MoonKnight.Auth.Infrastructures.DbContexts;
using MoonKnight.Auth.Domain.Entities;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using MoonKnight.Auth.Dtos;

namespace MoonKnight.Auth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly MoonKnightDbContext _context;

        public AuthController(MoonKnightDbContext context)
        {
            _context = context;
        }

        // Registration endpoint
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            // Validate password
            if (registerDto.Password != registerDto.ConfirmPassword)
            {
                return BadRequest("Passwords do not match");
            }

            // Check if user already exists
            var existingUser = await _context.Users
                .Where(u => u.Email == registerDto.Email)
                .FirstOrDefaultAsync();

            if (existingUser != null)
            {
                return Conflict("User already exists");
            }

            // Hash the password using BCrypt
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            // Create the user entity
            var user = new User
            {
                Email = registerDto.Email,
                PasswordHash = hashedPassword, // Store the hashed password
                Role = registerDto.Role ?? "User" // Default to 'User' if no role is provided
            };

            // Add the user to the database
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Registration successful" });
        }

        // Login endpoint
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _context.Users
                .Where(u => u.Email == loginDto.Email)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return Unauthorized("Invalid credentials");
            }

            // Verify the password using BCrypt
            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid credentials");
            }

            // Generate JWT or session here (optional step)
            return Ok(new { message = "Login successful" });
        }

        // Utility to hash the password (you can replace this with a more secure method like BCrypt in production)
       
    }
}
