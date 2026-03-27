using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ElSheemyCoaching.Core.DTOs;

public class ProgramFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "العنوان بالعربية مطلوب")]
    [Display(Name = "العنوان بالعربية")]
    [MaxLength(300)]
    public string TitleAr { get; set; } = string.Empty;

    [Required(ErrorMessage = "العنوان بالإنجليزية مطلوب")]
    [Display(Name = "العنوان بالإنجليزية")]
    [MaxLength(300)]
    public string TitleEn { get; set; } = string.Empty;

    [Required(ErrorMessage = "الرابط المختصر مطلوب")]
    [Display(Name = "Slug (رابط مختصر)")]
    [MaxLength(200)]
    //[RegularExpression(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", ErrorMessage = "الرابط يجب أن يكون حروف إنجليزية صغيرة وأرقام وشرطات فقط")]
    public string Slug { get; set; } = string.Empty;

    [Display(Name = "الوصف بالعربية")]
    public string DescriptionAr { get; set; } = string.Empty;

    [Display(Name = "الوصف بالإنجليزية")]
    public string DescriptionEn { get; set; } = string.Empty;

    [Required(ErrorMessage = "السعر مطلوب")]
    [Display(Name = "السعر (ج.م)")]
    [Range(0.01, 100000, ErrorMessage = "السعر يجب أن يكون أكبر من صفر")]
    public decimal Price { get; set; }

    [Display(Name = "السعر قبل الخصم (ج.م)")]
    public decimal? CompareAtPrice { get; set; }

    [Display(Name = "برنامج مميز")]
    public bool IsFeatured { get; set; }

    [Display(Name = "نشط")]
    public bool IsActive { get; set; } = true;

    // ─── File Uploads ───
    [Display(Name = "صورة الغلاف")]
    public IFormFile? CoverImage { get; set; }

    [Display(Name = "ملف PDF للبرنامج")]
    public IFormFile? PdfFile { get; set; }

    // ─── Read-only for Edit view ───
    public string? ExistingCoverImageUrl { get; set; }
    public string? ExistingPdfFileName { get; set; }
}
