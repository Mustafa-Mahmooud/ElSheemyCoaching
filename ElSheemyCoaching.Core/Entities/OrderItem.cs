namespace ElSheemyCoaching.Core.Entities;

public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int ProgramId { get; set; }

    public int? ProgramVariantId { get; set; }

    public decimal Price { get; set; }

    public int Quantity { get; set; } = 1;

    // Navigation
    public Order Order { get; set; } = null!;
    public WorkoutProgram Program { get; set; } = null!;
    public ProgramVariant? ProgramVariant { get; set; }
}
