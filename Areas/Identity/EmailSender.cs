using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

public class EmailSender : IEmailSender
{
    private readonly IConfiguration _config;

    public EmailSender(IConfiguration config)
    {
        _config = config;
    }

    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var smtpClient = new SmtpClient(_config["Smtp:Host"])
        {
            Port = int.Parse(_config["Smtp:Port"]),
            Credentials = new NetworkCredential(
                _config["Smtp:User"],
                _config["Smtp:Pass"]
            ),
            EnableSsl = true
        };

        return smtpClient.SendMailAsync(
            new MailMessage(_config["Smtp:User"], email, subject, htmlMessage)
            {
                IsBodyHtml = true
            }
        );
    }
}

