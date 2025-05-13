using Microsoft.EntityFrameworkCore;
using MoonKnight.Auth.Domain.Entities;
using System.Collections.Generic;

namespace MoonKnight.Auth.Infrastructures.DbContexts
{
    public class MoonKnightDbContext: DbContext
    {
        public MoonKnightDbContext(DbContextOptions<MoonKnightDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
    }
}
