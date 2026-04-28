using System.Threading.Tasks;
using ElSheemyCoaching.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ElSheemyCoaching.Web.Controllers;

public class FoodsController : Controller
{
    private readonly IFoodAlternativeService _foodAlternativeService;

    public FoodsController(IFoodAlternativeService foodAlternativeService)
    {
        _foodAlternativeService = foodAlternativeService;
    }

    [HttpGet]
    public IActionResult Alternatives()
    {
        return View();
    }

    [HttpGet]
    [Route("api/foods/alternatives")]
    public async Task<IActionResult> GetAlternatives(string name, decimal grams)
    {
        if (string.IsNullOrWhiteSpace(name) || grams <= 0)
        {
            return BadRequest(new { message = "Invalid input. Please provide a food name and quantity." });
        }

        var alternatives = await _foodAlternativeService.GetAlternativesAsync(name, grams);
        return Ok(alternatives);
    }

    [HttpGet]
    [Route("api/foods/list")]
    public async Task<IActionResult> GetFoodList()
    {
        var names = await _foodAlternativeService.GetAllFoodNamesAsync();
        return Ok(names);
    }
}
