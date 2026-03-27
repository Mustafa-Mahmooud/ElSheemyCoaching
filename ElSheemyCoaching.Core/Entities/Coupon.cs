namespace ElSheemyCoaching.Core.Entities;

public class Coupon
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public int DiscountPercent { get; set; }

    public int MaxUses { get; set; }

    public int CurrentUses { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
