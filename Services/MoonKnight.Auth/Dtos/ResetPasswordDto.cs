namespace MoonKnight.Auth.Dtos
{
    public class ResetPasswordDto
    {
        public string ResetToken { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
