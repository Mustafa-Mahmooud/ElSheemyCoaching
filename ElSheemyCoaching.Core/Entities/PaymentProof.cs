using ElSheemyCoaching.Core.Enums;

namespace ElSheemyCoaching.Core.Entities;

public class PaymentProof
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    /// <summary>
    /// Path to the uploaded InstaPay screenshot (stored outside wwwroot)
    /// </summary>
    public string ProofImagePath { get; set; } = string.Empty;

    public string? TransactionRef { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    public decimal Amount { get; set; }

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ReviewedAt { get; set; }

    public string? ReviewedByUserId { get; set; }

    // Navigation
    public Order Order { get; set; } = null!;
    public ApplicationUser? ReviewedBy { get; set; }
}
