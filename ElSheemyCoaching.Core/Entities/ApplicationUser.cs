using Microsoft.AspNetCore.Identity;

namespace ElSheemyCoaching.Core.Entities;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;

    // PhoneNumber is already in IdentityUser base class

    public decimal TotalSpent { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? OtpCode { get; set; }
    public DateTime? OtpExpiry { get; set; }

    // Navigation properties
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
