namespace ElSheemyCoaching.Core.Entities;

public class InAppNotification
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string TitleAr { get; set; } = string.Empty;

    public string TitleEn { get; set; } = string.Empty;

    public string MessageAr { get; set; } = string.Empty;

    public string MessageEn { get; set; } = string.Empty;

    public string? ActionUrl { get; set; }

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ApplicationUser User { get; set; } = null!;
}
