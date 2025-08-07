namespace MoonKnight.Identity.Dtos.User
{
    public class UserUpdateDto
    {
        public Guid Id { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Role { get; set; }
    }
}
