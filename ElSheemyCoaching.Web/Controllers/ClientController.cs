using ElSheemyCoaching.Data;
using ElSheemyCoaching.Core.Entities;
using ElSheemyCoaching.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElSheemyCoaching.Controllers;

[Authorize]
public class ClientController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ClientController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: /Client/MyPrograms
    [HttpGet("Client/MyPrograms")]
    public async Task<IActionResult> MyPrograms()
    {
        var userId = _userManager.GetUserId(User)!;

        // Fetch all PAID orders for this user, including their OrderItems and associated Programs.
        // We use IgnoreQueryFilters() to ensure we load soft-deleted programs that were purchased previously.
        var orders = await _context.Orders
            .Include(o => o.Items)
                .ThenInclude(oi => oi.Program)
            .Include(o => o.DownloadTokens)
            .IgnoreQueryFilters()
            .Where(o => o.UserId == userId && o.Status == OrderStatus.Paid)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return View(orders);
    }

    // GET: /Client/Progress
    [HttpGet("Client/Progress")]
    public async Task<IActionResult> Progress()
    {
        var userId = _userManager.GetUserId(User)!;

        var progressHistory = await _context.UserProgresses
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.Date)
            .ToListAsync();

        return View(progressHistory);
    }

    // POST: /Client/Progress
    [HttpPost("Client/Progress")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LogProgress([FromForm] decimal weight, [FromForm] string? notes)
    {
        var userId = _userManager.GetUserId(User)!;

        // Check if there's an entry for today
        var today = DateTime.UtcNow.Date;
        var existingEntry = await _context.UserProgresses
            .FirstOrDefaultAsync(p => p.UserId == userId && p.Date == today);

        if (existingEntry != null)
        {
            existingEntry.Weight = weight;
            existingEntry.Notes = notes;
        }
        else
        {
            _context.UserProgresses.Add(new UserProgress
            {
                UserId = userId,
                Date = today,
                Weight = weight,
                Notes = notes
            });
        }

        await _context.SaveChangesAsync();

        TempData["Success"] = "تم حفظ تحديث تقدمك بنجاح";
        return RedirectToAction(nameof(Progress));
    }

    // GET: /Client/Calculator
    [HttpGet("Client/Calculator")]
    public IActionResult Calculator()
    {
        return View();
    }
}
