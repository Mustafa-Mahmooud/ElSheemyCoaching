namespace ElSheemyCoaching.Core.Entities;

public class DownloadToken
{
    public int Id { get; set; }

    public string Token { get; set; } = Guid.NewGuid().ToString("N");

    public int OrderId { get; set; }

    public int ProgramId { get; set; }

    public string UserId { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public bool IsUsed { get; set; }

    // Navigation
    public Order Order { get; set; } = null!;
    public WorkoutProgram Program { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}
