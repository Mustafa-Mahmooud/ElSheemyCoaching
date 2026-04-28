using ElSheemyCoaching.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Localization;

namespace ElSheemyCoaching.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SearchController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SearchController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("live")]
    public async Task<IActionResult> LiveSearch(string query)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return Ok(new { programs = new List<object>(), transformations = new List<object>() });

        var requestCulture = HttpContext.Features.Get<IRequestCultureFeature>();
        var isArabic = requestCulture?.RequestCulture.Culture.Name.StartsWith("ar") ?? true;

        query = query.ToLower().Trim();

        var programs = await _context.Programs
            .Where(p => p.IsActive && 
                       (p.TitleAr.ToLower().Contains(query) || 
                        p.TitleEn.ToLower().Contains(query) || 
                        p.DescriptionAr.ToLower().Contains(query) || 
                        p.DescriptionEn.ToLower().Contains(query)))
            .Take(5)
            .Select(p => new
            {
                title = isArabic ? p.TitleAr : p.TitleEn,
                url = "/Programs/Details/" + p.Slug,
                type = isArabic ? "برنامج" : "Program",
                image = p.CoverImageUrl
            })
            .ToListAsync();

        var transformations = await _context.Transformations
            .Where(t => t.IsVisible && 
                       (t.TitleAr.ToLower().Contains(query) || 
                        t.TitleEn.ToLower().Contains(query) || 
                        t.DescriptionAr.ToLower().Contains(query) || 
                        t.DescriptionEn.ToLower().Contains(query)))
            .Take(5)
            .Select(t => new
            {
                title = isArabic ? t.TitleAr : t.TitleEn,
                url = "/#transformations", // Scroll to transformations section
                type = isArabic ? "قصة نجاح" : "Transformation",
                image = t.AfterImageUrl
            })
            .ToListAsync();

        return Ok(new { programs, transformations });
    }
}
