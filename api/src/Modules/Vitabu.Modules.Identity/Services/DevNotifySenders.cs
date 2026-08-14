using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Vitabu.Modules.Identity.Services;

public sealed class LoggingSmsSender(ILogger<LoggingSmsSender> logger) : ISmsSender
{
    public Task SendAsync(string phoneE164, string message, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("SMS to {Phone}: {Message}", phoneE164, message);
        return Task.CompletedTask;
    }
}

public sealed class SmtpEmailSender(IConfiguration configuration, ILogger<SmtpEmailSender> logger) : IEmailSender
{
    public async Task SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default)
    {
        var host = configuration["Smtp:Host"] ?? "localhost";
        var port = int.TryParse(configuration["Smtp:Port"], out var p) ? p : 1025;
        var from = configuration["Smtp:From"] ?? "noreply@vitabu.local";

        try
        {
            using var client = new SmtpClient(host, port)
            {
                DeliveryMethod = SmtpDeliveryMethod.Network,
                EnableSsl = false
            };
            using var message = new MailMessage(from, toEmail, subject, body);
            await client.SendMailAsync(message, cancellationToken);
            logger.LogInformation("Email sent to {Email}: {Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            // Local Mailpit may be down — log and continue so forgot-password still returns 202.
            logger.LogWarning(ex, "Failed to send email to {Email}; token still created", toEmail);
        }
    }
}
