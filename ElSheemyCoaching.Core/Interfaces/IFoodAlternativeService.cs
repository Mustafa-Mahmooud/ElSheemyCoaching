using System.Collections.Generic;
using System.Threading.Tasks;
using ElSheemyCoaching.Core.DTOs;

namespace ElSheemyCoaching.Core.Interfaces;

public interface IFoodAlternativeService
{
    Task<IEnumerable<FoodAlternativeResultDto>> GetAlternativesAsync(string foodName, decimal quantity);
    Task<IEnumerable<string>> GetAllFoodNamesAsync();
}
