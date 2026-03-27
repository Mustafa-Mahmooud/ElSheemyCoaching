namespace ElSheemyCoaching.Core.Entities;

public class ProgramVariant
{
    public int Id { get; set; }

    public int ProgramId { get; set; }

    public string NameAr { get; set; } = string.Empty;

    public string NameEn { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string FilePath { get; set; } = string.Empty;

    // Navigation
    public WorkoutProgram Program { get; set; } = null!;
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
