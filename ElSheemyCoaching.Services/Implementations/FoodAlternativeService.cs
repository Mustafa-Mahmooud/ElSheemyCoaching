using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ElSheemyCoaching.Core.DTOs;
using ElSheemyCoaching.Core.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace ElSheemyCoaching.Services.Implementations;

public class FoodAlternativeService : IFoodAlternativeService
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private FoodAlternativeData? _cachedData;

    public FoodAlternativeService(IWebHostEnvironment webHostEnvironment)
    {
        _webHostEnvironment = webHostEnvironment;
    }

    private async Task<FoodAlternativeData> LoadDataAsync()
    {
        if (_cachedData != null) return _cachedData;

        string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "data", "food-alternatives.json");
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Food alternatives data file not found.", filePath);
        }

        string jsonContent = await File.ReadAllTextAsync(filePath);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        _cachedData = JsonSerializer.Deserialize<FoodAlternativeData>(jsonContent, options) 
                      ?? new FoodAlternativeData();
        
        return _cachedData;
    }

    public async Task<IEnumerable<FoodAlternativeResultDto>> GetAlternativesAsync(string foodName, decimal quantity)
    {
        var data = await LoadDataAsync();
        foodName = foodName.Trim();

        // 1. Find the source food item
        FoodItemDto? sourceItem = null;
        FoodCategoryDto? sourceCategory = null;

        foreach (var category in data.Categories)
        {
            sourceItem = category.Items.FirstOrDefault(i => 
                string.Equals(i.Name, foodName, StringComparison.OrdinalIgnoreCase) ||
                i.Aliases.Any(a => string.Equals(a, foodName, StringComparison.OrdinalIgnoreCase))
            );

            if (sourceItem != null)
            {
                sourceCategory = category;
                break;
            }
        }

        if (sourceItem == null || sourceCategory == null)
        {
            return Enumerable.Empty<FoodAlternativeResultDto>();
        }

        // 2. Calculate scaling ratio
        // ratio = user_quantity / reference_quantity
        decimal ratio = quantity / sourceItem.ReferenceAmount;

        // 3. Generate alternatives from the same category
        var results = sourceCategory.Items
            .Where(i => i.Id != sourceItem.Id)
            .Select(i => new FoodAlternativeResultDto
            {
                FoodName = i.Name,
                AdjustedQuantity = Math.Round(i.ReferenceAmount * ratio, 1),
                Category = sourceCategory.Name,
                Unit = i.Unit
            })
            .ToList();

        return results;
    }

    public async Task<IEnumerable<string>> GetAllFoodNamesAsync()
    {
        var data = await LoadDataAsync();
        return data.Categories
            .SelectMany(c => c.Items)
            .SelectMany(i => new[] { i.Name }.Concat(i.Aliases))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(n => n)
            .ToList();
    }
}
