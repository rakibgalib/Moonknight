using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoonKnight.Identity.Domain.Entities;
using MoonKnight.Identity.Dtos;
using MoonKnight.Identity.Infrastructures.DbContexts;

namespace MoonKnight.Identity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserProfilesController : ControllerBase
    {
        private readonly IdentityDbContext _context;
        private readonly IMapper _mapper;

        public UserProfilesController(IdentityDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] UserCreateDto dto)
        {
            var entity = _mapper.Map<UserProfile>(dto);
            await _context.UserProfiles.AddAsync(entity);
            await _context.SaveChangesAsync();
            return Ok(new { message = "User created successfully" });
        }
    }
}
