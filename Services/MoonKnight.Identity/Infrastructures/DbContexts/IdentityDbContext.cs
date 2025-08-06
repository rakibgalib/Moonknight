using Microsoft.EntityFrameworkCore;
using MoonKnight.Identity.Domain.Entities;

namespace MoonKnight.Identity.Infrastructures.DbContexts
{
    public class IdentityDbContext:DbContext
    {
        public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options) { }

        public DbSet<UserProfile> UserProfiles { get; set; }
    }
}
