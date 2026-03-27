using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ElSheemyCoaching.Core.DTOs;

public class InstaPayUploadViewModel
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public decimal Total { get; set; }

    /// <summary>
    /// The InstaPay handle/address to transfer to
    /// </summary>
    public string InstaPayHandle { get; set; } = string.Empty;

    [Display(Name = "صورة إثبات الدفع")]
    public IFormFile? ProofImage { get; set; }

    [Display(Name = "رقم العملية (اختياري)")]
    [MaxLength(200)]
    public string? TransactionRef { get; set; }
}
