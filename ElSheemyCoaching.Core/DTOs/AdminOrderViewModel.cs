using ElSheemyCoaching.Core.Enums;

namespace ElSheemyCoaching.Core.DTOs;

public class AdminOrderViewModel
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? PaymentProofId { get; set; }
    public string? ProofImagePath { get; set; }
    public string? TransactionRef { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }
    public List<AdminOrderItemViewModel> Items { get; set; } = new();
}

public class AdminOrderItemViewModel
{
    public string ProgramTitle { get; set; } = string.Empty;
    public string? VariantName { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}
