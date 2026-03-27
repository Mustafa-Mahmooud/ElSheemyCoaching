using ElSheemyCoaching.Core.Enums;

namespace ElSheemyCoaching.Core.Entities;

public class Order
{
    public int Id { get; set; }

    public string OrderNumber { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public OrderStatus Status { get; set; } = OrderStatus.AwaitingVerification;

    public decimal Total { get; set; }

    public int? CouponId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ApplicationUser User { get; set; } = null!;
    public Coupon? Coupon { get; set; }
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public PaymentProof? PaymentProof { get; set; }
    public ICollection<DownloadToken> DownloadTokens { get; set; } = new List<DownloadToken>();
}
