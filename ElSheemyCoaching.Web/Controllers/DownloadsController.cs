using ElSheemyCoaching.Data;
using ElSheemyCoaching.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElSheemyCoaching.Controllers;

[Authorize]
public class DownloadsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _env;

    public DownloadsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IWebHostEnvironment env)
    {
        _context = context;
        _userManager = userManager;
        _env = env;
    }

    // GET: /Downloads/Download/{token}
    [HttpGet("Downloads/{token}")]
    public async Task<IActionResult> Download(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return BadRequest("رابط التحميل غير صالح");

        var userId = _userManager.GetUserId(User)!;

        var downloadToken = await _context.DownloadTokens
            .Include(d => d.Program)
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(d => d.Token == token);

        if (downloadToken is null)
            return NotFound("رابط التحميل غير موجود");

        // Security: verify ownership
        if (downloadToken.UserId != userId)
            return Forbid();

        // Check expiry
        if (downloadToken.ExpiresAt < DateTime.UtcNow)
            return BadRequest("رابط التحميل منتهي الصلاحية");

        // Resolve the file path
        var filePath = Path.Combine(_env.ContentRootPath, downloadToken.Program.PrivateFilePath);

        if (!System.IO.File.Exists(filePath))
            return NotFound("الملف غير موجود");

        // Mark as used (but don't prevent re-download within expiry)
        downloadToken.IsUsed = true;
        await _context.SaveChangesAsync();

        // Stream the file — never expose the URL
        var fileName = $"{downloadToken.Program.TitleEn}.pdf";
        return PhysicalFile(filePath, "application/pdf", fileName);
    }
}
