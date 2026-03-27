using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ElSheemyCoaching.Core.DTOs;

public class TransformationFormViewModel
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

    [Display(Name = "الوصف بالعربية")]
    public string DescriptionAr { get; set; } = string.Empty;

    [Display(Name = "الوصف بالإنجليزية")]
    public string DescriptionEn { get; set; } = string.Empty;

    [Display(Name = "صورة قبل (Before)")]
    public IFormFile? BeforeImage { get; set; }

    [Display(Name = "صورة بعد (After)")]
    public IFormFile? AfterImage { get; set; }

    [Display(Name = "إظهار للعملاء")]
    public bool IsVisible { get; set; } = true;

    [Required(ErrorMessage = "ترتيب العرض مطلوب")]
    [Display(Name = "ترتيب العرض")]
    public int DisplayOrder { get; set; }

    // Read-only strings to hold paths when editing
    public string? ExistingBeforeImageUrl { get; set; }
    public string? ExistingAfterImageUrl { get; set; }
}
