using System.Net;
using System.Net.Mail;
using ElSheemyCoaching.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace ElSheemyCoaching.Services.Implementations;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        try
        {
            var emailSettings = _configuration.GetSection("Email");
            var fromEmail = emailSettings["From"];
            var host = emailSettings["SmtpHost"];
            var port = int.Parse(emailSettings["Port"] ?? "587");
            var username = emailSettings["Username"];
            var password = emailSettings["Password"];

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail!),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("Email sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {Email}", toEmail);
            throw;
        }
    }

    public async Task SendOrderApprovedEmailAsync(string toEmail, string customerName, string orderNumber, string downloadUrl)
    {
        string subject = "تمت الموافقة على طلبك - El Sheemy Coaching";
        string body = $@"
            <h3>مرحباً {customerName}،</h3>
            <p>لقد تمت الموافقة على طلبك رقم <b>{orderNumber}</b> بنجاح.</p>
            <p>يمكنك تحميل الملف الخاص بك من الرابط التالي:</p>
            <a href='{downloadUrl}' style='padding: 10px 20px; background-color: #007bff; color: white; text-decoration: none; border-radius: 5px;'>تحميل الملف</a>
            <p>شكراً لتعاملك معنا!</p>";

        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendOrderRejectedEmailAsync(string toEmail, string customerName, string orderNumber)
    {
        string subject = "حول طلبك - El Sheemy Coaching";
        string body = $@"
            <h3>مرحباً {customerName}،</h3>
            <p>نعتذر منك، لم يتم قبول طلبك رقم <b>{orderNumber}</b>.</p>
            <p>يرجى التأكد من بيانات الدفع أو التواصل معنا للمزيد من التفاصيل.</p>";

        await SendEmailAsync(toEmail, subject, body);
    }
}
