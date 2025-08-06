using Microsoft.EntityFrameworkCore;
using MoonKnight.Auth.Configuration;
using MoonKnight.Auth.Domain.Interfaces;
using MoonKnight.Auth.Infrastructures.DbContexts;
using MoonKnight.Auth.Infrastructures.Interfaces.Services;
using MoonKnight.Auth.Infrastructures.Repositories;
using MoonKnight.Auth.Infrastructures.Services;
using MoonKnight.Auth.Mappings;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<IEmailServices, EmailService>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<MoonKnightDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddAutoMapper(typeof(AuthMappingProfile));

builder.Services.AddRateLimiter(o =>
{
    o.AddPolicy("login-policy", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "global",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,                    // max attempts
                Window = TimeSpan.FromMinutes(1),   // per 1 minute
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
app.UseRateLimiter();