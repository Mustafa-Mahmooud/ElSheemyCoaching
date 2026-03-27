using ElSheemyCoaching.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace ElSheemyCoaching.Services.Implementations;

/// <summary>
/// Placeholder email service that logs to console.
/// Replace with SMTP/SendGrid implementation when ready.
/// </summary>
public class ConsoleEmailService : IEmailService
{
    private readonly ILogger<ConsoleEmailService> _logger;

    public ConsoleEmailService(ILogger<ConsoleEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string toEmail, string subject, string body)
    {
        _logger.LogInformation(
            "📧 [EMAIL] → To: {Email}, Subject: {Subject}, Body: {Body}",
            toEmail, subject, body);
        return Task.CompletedTask;
    }

    public Task SendOrderApprovedEmailAsync(string toEmail, string customerName, string orderNumber, string downloadUrl)
    {
        _logger.LogInformation(
            "📧 [EMAIL] Order Approved → To: {Email}, Customer: {Name}, Order: {OrderNumber}, Download: {Url}",
            toEmail, customerName, orderNumber, downloadUrl);
        return Task.CompletedTask;
    }

    public Task SendOrderRejectedEmailAsync(string toEmail, string customerName, string orderNumber)
    {
        _logger.LogInformation(
            "📧 [EMAIL] Order Rejected → To: {Email}, Customer: {Name}, Order: {OrderNumber}",
            toEmail, customerName, orderNumber);
        return Task.CompletedTask;
    }
}
