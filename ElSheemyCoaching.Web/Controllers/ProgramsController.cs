using ElSheemyCoaching.Data;
using ElSheemyCoaching.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElSheemyCoaching.Controllers;

public class ProgramsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProgramsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /Programs
    public async Task<IActionResult> Index()
    {
        var programs = await _context.Programs
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.IsFeatured)
            .ThenByDescending(p => p.CreatedAt)
            .ToListAsync();

        return View(programs);
    }

    // GET: /Programs/Details/{slug}
    public async Task<IActionResult> Details(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return NotFound();

        var program = await _context.Programs
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Slug == slug && p.IsActive);

        if (program is null)
            return NotFound();

        return View(program);
    }
}
