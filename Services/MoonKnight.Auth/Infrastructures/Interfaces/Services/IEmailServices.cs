namespace MoonKnight.Auth.Infrastructures.Interfaces.Services
{
    public interface IEmailServices
    {
        Task SendAsync(string to, string subject, string body);
    }
}
