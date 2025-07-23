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
using MoonKnight.Auth.Infrastructures.Services;
using AutoMapper;

namespace MoonKnight.Auth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly MoonKnightDbContext _context;
        private readonly JwtTokenGenerator _jwtTokenGenerator;
        private readonly IMapper _mapper;
        public AuthController(MoonKnightDbContext context, JwtTokenGenerator jwtTokenGenerator,IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
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
                return BadRequest(new ApiResponse<string>(false, "Invalid credentials"));
            }

            // Verify the password using BCrypt
            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return BadRequest(new ApiResponse<string>(false, "Invalid credentials"));
            }
            var token = _jwtTokenGenerator.GenerateToken(user);
            return Ok(new ApiResponse<string>(true, "Login successful", token));

        }

        // Utility to hash the password (you can replace this with a more secure method like BCrypt in production)
       
    }
}
