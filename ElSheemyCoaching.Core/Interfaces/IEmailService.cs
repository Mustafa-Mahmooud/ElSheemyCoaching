namespace ElSheemyCoaching.Core.Interfaces;

public interface IEmailService
{
    Task SendOrderApprovedEmailAsync(string toEmail, string customerName, string orderNumber, string downloadUrl);
    Task SendOrderRejectedEmailAsync(string toEmail, string customerName, string orderNumber);
    Task SendEmailAsync(string toEmail, string subject, string body);
}
