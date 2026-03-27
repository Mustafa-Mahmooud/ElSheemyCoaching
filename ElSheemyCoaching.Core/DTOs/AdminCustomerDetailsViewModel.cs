namespace ElSheemyCoaching.Core.DTOs;

public class AdminCustomerDetailsViewModel
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public decimal TotalSpent { get; set; }
    public int OrderCount { get; set; }
    public DateTime RegisteredAt { get; set; }

    public List<AdminCustomerOrderHistoryViewModel> Orders { get; set; } = new();
}

public class AdminCustomerOrderHistoryViewModel
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
    public Enums.OrderStatus Status { get; set; }
    public List<string> Items { get; set; } = new();
}
