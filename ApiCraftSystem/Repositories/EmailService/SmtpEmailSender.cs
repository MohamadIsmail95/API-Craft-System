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

        // using auth SMTP
        //public async Task SendEmailAsync(string toEmail, string subject, string body)
        //{
        //    var smtpHost = _config["EmailSettings:Host"];
        //    var smtpPort = int.Parse(_config["EmailSettings:Port"]);
        //    var smtpUser = _config["EmailSettings:Username"];
        //    var smtpPass = _config["EmailSettings:Password"];
        //    var fromEmail = _config["EmailSettings:FromEmail"];

        //    using var client = new SmtpClient(smtpHost, smtpPort)
        //    {
        //        Credentials = new NetworkCredential(smtpUser, smtpPass),
        //        EnableSsl = true,
        //        UseDefaultCredentials = false

        //    };

        //    using var message = new MailMessage(fromEmail, toEmail, subject, body)
        //    {
        //        IsBodyHtml = true
        //    };

        //    await client.SendMailAsync(message);
        //}

        //use SMTP relay without auth

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpHost = _config["EmailSettings:Host"];
            var smtpPort = int.Parse(_config["EmailSettings:Port"]);
            var fromEmail = _config["EmailSettings:FromEmail"];

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = false, // or true if your server still supports SSL without auth
                UseDefaultCredentials = false // crucial to keep this false
            };

            // No credentials set

            using var message = new MailMessage(fromEmail, toEmail, subject, body)
            {
                IsBodyHtml = true
            };

            await client.SendMailAsync(message);
        }

    }
}
