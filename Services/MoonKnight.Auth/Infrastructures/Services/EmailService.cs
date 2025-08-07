using MoonKnight.Auth.Application.Interfaces;
using System.Net;
using System.Net.Mail;

namespace MoonKnight.Auth.Infrastructures.Services
{
    public class EmailService:IEmailServices
    {
        private readonly SmtpClient _smtp;
        private readonly string _fromAddress;
        public EmailService(IConfiguration config)
        {
            _fromAddress = config["Email:From"] ?? "7a9f93001@smtp-brevo.com";

            _smtp = new SmtpClient(config["Email:Smtp"], int.Parse(config["Email:Port"]))
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(config["Email:Username"], config["Email:Password"]),
                EnableSsl = true
            };
        }


        public async Task SendAsync(string to, string subject, string body)
        {
            var mail = new MailMessage(_fromAddress, to, subject, body)
            {
                IsBodyHtml = true
            };
            await _smtp.SendMailAsync(mail);
        }
    }
}
