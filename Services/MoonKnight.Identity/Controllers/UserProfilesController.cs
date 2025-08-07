using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoonKnight.Identity.Domain.Entities;
using MoonKnight.Identity.Dtos.User;
using MoonKnight.Identity.Infrastructures.DbContexts;

namespace MoonKnight.Identity.Controllers
{
    [Authorize]   //
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
        [Authorize(Roles = "Admin")]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] UserCreateDto dto)
        {
            var entity = _mapper.Map<UserProfile>(dto);
            await _context.UserProfiles.AddAsync(entity);
            await _context.SaveChangesAsync();
            return Ok(new { message = "User created successfully" });
        }
   
       
        [HttpPost("list")]
        public async Task<IActionResult> List([FromBody] UserListRequestDto dto)
        {
            var query = _context.UserProfiles.AsQueryable();

            if (dto.TenantId.HasValue)
                query = query.Where(x => x.TenantId == dto.TenantId && x.IsActive);

            var paged = await query
                .Skip((dto.Page - 1) * dto.PageSize)
                .Take(dto.PageSize)
                .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return Ok(paged);
        }

        // GET BY ID
        [HttpPost("get")]
        public async Task<IActionResult> Get([FromBody] Guid id)
        {
            var user = await _context.UserProfiles.FindAsync(id);
            if (user == null) return NotFound();

            var dto = _mapper.Map<UserDto>(user);
            return Ok(dto);
        }
        [Authorize(Roles = "Admin")]
        // UPDATE
        [HttpPost("update")]
        public async Task<IActionResult> Update([FromBody] UserUpdateDto dto)
        {
            var user = await _context.UserProfiles.FindAsync(dto.Id);
            if (user == null) return NotFound();

            if (dto.FullName != null)
                user.FullName = dto.FullName;
            if (dto.Phone != null)
                user.Phone = dto.Phone;
            if (dto.Role != null)
                user.Role = dto.Role;

            await _context.SaveChangesAsync();
            return Ok(new { message = "User updated" });
        }
        [Authorize(Roles = "Admin")]
        // DELETE (Soft delete)
        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody] Guid id)
        {
            var user = await _context.UserProfiles.FindAsync(id);
            if (user == null) return NotFound();

            user.IsActive = false;
            await _context.SaveChangesAsync();
            return Ok(new { message = "User deactivated" });
        }
    }
}
