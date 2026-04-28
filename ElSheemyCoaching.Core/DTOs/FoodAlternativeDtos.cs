using System.Collections.Generic;

namespace ElSheemyCoaching.Core.DTOs;

public class FoodAlternativeData
{
    public List<FoodCategoryDto> Categories { get; set; } = new();
}

public class FoodCategoryDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<FoodItemDto> Items { get; set; } = new();
}

public class FoodItemDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<string> Aliases { get; set; } = new();
    public decimal ReferenceAmount { get; set; }
    public string Unit { get; set; } = string.Empty;
    public FoodMacrosDto? Macros { get; set; }
}

public class FoodMacrosDto
{
    public decimal Protein { get; set; }
    public decimal Carbs { get; set; }
    public decimal Fats { get; set; }
    public decimal Calories { get; set; }
}

public class FoodAlternativeResultDto
{
    public string FoodName { get; set; } = string.Empty;
    public decimal AdjustedQuantity { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
}
