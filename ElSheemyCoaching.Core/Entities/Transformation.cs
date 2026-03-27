namespace ElSheemyCoaching.Core.Entities;

public class Transformation
{
    public int Id { get; set; }

    public string TitleAr { get; set; } = string.Empty;

    public string TitleEn { get; set; } = string.Empty;

    public string DescriptionAr { get; set; } = string.Empty;

    public string DescriptionEn { get; set; } = string.Empty;

    public string BeforeImageUrl { get; set; } = string.Empty;

    public string AfterImageUrl { get; set; } = string.Empty;

    public bool IsVisible { get; set; } = true;

    public int DisplayOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
