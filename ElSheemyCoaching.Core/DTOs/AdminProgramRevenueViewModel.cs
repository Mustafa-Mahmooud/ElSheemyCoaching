namespace ElSheemyCoaching.Core.DTOs;

public class AdminProgramRevenueViewModel
{
    public string TitleAr { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int Purchases { get; set; }
}
