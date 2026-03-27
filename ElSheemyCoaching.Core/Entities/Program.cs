namespace ElSheemyCoaching.Core.Entities;

public class WorkoutProgram
{
    public int Id { get; set; }

    public string Slug { get; set; } = string.Empty;

    public string TitleAr { get; set; } = string.Empty;

    public string TitleEn { get; set; } = string.Empty;

    public string DescriptionAr { get; set; } = string.Empty;

    public string DescriptionEn { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public decimal? CompareAtPrice { get; set; }

    public string? CoverImageUrl { get; set; }

    /// <summary>
    /// Path to the PDF file stored OUTSIDE wwwroot (e.g., SecureFiles/)
    /// </summary>
    public string PrivateFilePath { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public bool IsFeatured { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<ProgramVariant> Variants { get; set; } = new List<ProgramVariant>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
