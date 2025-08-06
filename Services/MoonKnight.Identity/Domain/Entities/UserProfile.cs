namespace MoonKnight.Identity.Domain.Entities
{
    public class UserProfile
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; } = "User";
        public Guid TenantId { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
