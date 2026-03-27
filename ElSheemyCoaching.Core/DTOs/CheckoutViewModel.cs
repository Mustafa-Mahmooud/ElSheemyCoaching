namespace ElSheemyCoaching.Core.DTOs;

public class CheckoutViewModel
{
    public int ProgramId { get; set; }
    public string ProgramTitle { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int? VariantId { get; set; }
    public string? VariantName { get; set; }
    public string? CouponCode { get; set; }
}
