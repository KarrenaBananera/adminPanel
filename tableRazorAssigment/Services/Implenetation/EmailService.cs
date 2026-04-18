
using System.Net;
using System.Net.Mail;
using tableRazorAssigment.Configuration;

namespace tableRazorAssigment.Services.Implenetation;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    public EmailService(EmailSettings settings)
    {
        _settings = settings;
    }
    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = false)
    {
        using var message = ConfigureMessage(to, subject, body, isHtml);
        using var client = ConfigureSMTPClient();
        await client.SendMailAsync(message);
    }

    private MailMessage ConfigureMessage(string to, string subject, string body, bool isHtml = false)
    {
        var message = new MailMessage();
        message.From = new MailAddress(_settings.From);
        message.To.Add(to);
        message.Subject = subject;
        message.Body = body;
        message.IsBodyHtml = isHtml;
        return message;
    }

    private SmtpClient ConfigureSMTPClient()
    {
        var client = new SmtpClient(_settings.Host, _settings.Port);
        client.EnableSsl = _settings.EnableSsl;
        client.Credentials = new NetworkCredential(_settings.Username, _settings.Password);
        client.UseDefaultCredentials = false;
        client.DeliveryMethod = SmtpDeliveryMethod.Network;
        return client;
    }
}
