using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;

namespace UserService.Services
{
    public class EmailSender : IEmailSender
    {
        private static readonly string EMAIL_CONFIG = "EmailSender";

        private IConfiguration configuration;

        private EmailConfig emailConf;

        public EmailSender(IConfiguration configuration)
        {
            this.configuration = configuration;
            emailConf = new EmailConfig();
            configuration.Bind(EMAIL_CONFIG, emailConf);
        }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            SmtpClient smtpClient = new SmtpClient();
            smtpClient.Port = emailConf.Port;
            smtpClient.UseDefaultCredentials = emailConf.UseDefaultCredentials;
            smtpClient.Host = emailConf.Host;
            smtpClient.EnableSsl = emailConf.EnableSsl;
            smtpClient.Credentials = new NetworkCredential(emailConf.Credentials.Login, emailConf.Credentials.Password);
            MailMessage mail = new MailMessage(emailConf.From, email);
            mail.Subject = subject;
            mail.Body = message;
            smtpClient.SendAsync(mail, "trse");
            return Task.CompletedTask;
        }
    }

    public class EmailConfig
    {
        public string Host { get; set; }
        public string From { get; set; }
        public int Port { get; set; }
        public bool UseDefaultCredentials { get; set; }
        public bool EnableSsl { get; set; }
        public Credentials Credentials { get; set; }
    }

    public class Credentials
    {
        public string Login { get; set; }
        public string Password { get; set; }
    }
}
