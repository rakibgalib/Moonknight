namespace MoonKnight.Auth.Domain.Entities
{
    public class PasswordResetToken
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
        public DateTime Expires { get; set; }
        public DateTime Created { get; set; }
        public bool Used { get; set; }
    }
}
