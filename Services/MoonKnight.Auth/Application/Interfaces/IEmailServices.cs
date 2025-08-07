namespace MoonKnight.Auth.Application.Interfaces
{
    public interface IEmailServices
    {
        Task SendAsync(string to, string subject, string body);
    }
}
