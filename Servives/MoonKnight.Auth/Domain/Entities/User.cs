namespace MoonKnight.Auth.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }  // e.g., Admin, Technician
        public Guid TenantId { get; set; }
    }
}
