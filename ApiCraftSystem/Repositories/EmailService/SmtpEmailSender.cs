using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;

namespace ApiCraftSystem.Repositories.EmailService
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly IConfiguration _config;

        public SmtpEmailSender(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpHost = _config["Email:Host"];
            var smtpPort = int.Parse(_config["Email:Port"]);
            var smtpUser = _config["Email:Username"];
            var smtpPass = _config["Email:Password"];
            var fromEmail = _config["Email:FromEmail"];

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            using var message = new MailMessage(fromEmail, toEmail, subject, body)
            {
                IsBodyHtml = true
            };

            await client.SendMailAsync(message);
        }
    }
}
