using System.ComponentModel.DataAnnotations;

namespace ElSheemyCoaching.Core.DTOs;

public class VerifyOtpViewModel
{
    public string UserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "يرجى إدخال رمز التحقق")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "رمز التحقق يجب أن يكون 6 أرقام")]
    public string Otp { get; set; } = string.Empty;
}
