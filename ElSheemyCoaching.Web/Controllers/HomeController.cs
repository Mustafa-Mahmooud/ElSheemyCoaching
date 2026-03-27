using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ElSheemyCoaching.Core.Entities;

using ElSheemyCoaching.Data;
using Microsoft.EntityFrameworkCore;

namespace ElSheemyCoaching.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var transformations = await _context.Transformations
            .Where(t => t.IsVisible)
            .OrderBy(t => t.DisplayOrder)
            .ToListAsync();

        return View(transformations);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
