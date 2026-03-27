using ElSheemyCoaching.Data;
using ElSheemyCoaching.Core.Entities;
using ElSheemyCoaching.Core.Enums;
using ElSheemyCoaching.Core.Interfaces;
using ElSheemyCoaching.Core.DTOs;
using ElSheemyCoaching.Services.Implementations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElSheemyCoaching.Controllers;

[Authorize(Roles = "Admin,Coach")]
[Route("Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IOrderService _orderService;
    private readonly IEmailService _emailService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        ApplicationDbContext context,
        IOrderService orderService,
        IEmailService emailService,
        UserManager<ApplicationUser> userManager,
        IWebHostEnvironment env,
        ILogger<AdminController> logger)
    {
        _context = context;
        _orderService = orderService;
        _emailService = emailService;
        _userManager = userManager;
        _env = env;
        _logger = logger;
    }

    // ══════════════════════════════════════════════════════════
    //  DASHBOARD (ANALYTICS)
    // ══════════════════════════════════════════════════════════

    // GET: /Admin
    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index()
    {
        var totalRevenue = await _context.Orders
            .Where(o => o.Status == OrderStatus.Paid)
            .SumAsync(o => o.Total);

        var totalUsersCount = (await _userManager.GetUsersInRoleAsync("Client")).Count;

        var totalOrders = await _context.Orders.CountAsync();

        var pendingApprovals = await _context.PaymentProofs
            .Where(p => p.Status == PaymentStatus.Pending)
            .CountAsync();

        var programRevenue = await _context.OrderItems
            .Where(i => i.Order.Status == OrderStatus.Paid)
            .GroupBy(i => new { i.Program.TitleAr, i.Program.TitleEn })
            .Select(g => new AdminProgramRevenueViewModel
            {
                TitleAr = g.Key.TitleAr,
                TitleEn = g.Key.TitleEn,
                Revenue = g.Sum(i => i.Price),
                Purchases = g.Count()
            })
            .OrderByDescending(r => r.Revenue)
            .ToListAsync();

        ViewBag.TotalRevenue = totalRevenue;
        ViewBag.TotalUsers = totalUsersCount;
        ViewBag.TotalOrders = totalOrders;
        ViewBag.PendingApprovals = pendingApprovals;
        ViewBag.ProgramRevenue = programRevenue;

        var recentPending = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.PaymentProof)
            .Where(o => o.Status == OrderStatus.AwaitingVerification)
            .OrderByDescending(o => o.CreatedAt)
            .Take(5)
            .ToListAsync();

        return View(recentPending);
    }

    // ══════════════════════════════════════════════════════════
    //  ORDERS
    // ══════════════════════════════════════════════════════════

    // GET: /Admin/Orders
    [HttpGet("Orders")]
    public async Task<IActionResult> Orders(string? status = null)
    {
        var query = _context.Orders
            .Include(o => o.User)
            .Include(o => o.PaymentProof).Where(p => p.PaymentProof != null)
            .Include(o => o.Items).ThenInclude(i => i.Program)
            .Include(o => o.Items).ThenInclude(i => i.ProgramVariant)
            .AsQueryable();

        if (Enum.TryParse<OrderStatus>(status, out var parsedStatus))
            query = query.Where(o => o.Status == parsedStatus);
        else
            query = query.Where(o => o.Status == OrderStatus.AwaitingVerification);

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new AdminOrderViewModel
            {
                OrderId = o.Id,
                OrderNumber = o.OrderNumber,
                UserId = o.UserId,
                CustomerName = o.User.FullName,
                CustomerEmail = o.User.Email!,
                Total = o.Total,
                Status = o.Status,
                CreatedAt = o.CreatedAt,
                PaymentProofId = o.PaymentProof != null ? o.PaymentProof.Id : null,
                ProofImagePath = o.PaymentProof != null ? o.PaymentProof.ProofImagePath : null,
                TransactionRef = o.PaymentProof != null ? o.PaymentProof.TransactionRef : null,
                PaymentStatus = o.PaymentProof != null ? o.PaymentProof.Status : null,
                Items = o.Items.Select(i => new AdminOrderItemViewModel
                {
                    ProgramTitle = i.Program.TitleAr,
                    VariantName = i.ProgramVariant != null ? i.ProgramVariant.NameAr : null,
                    Price = i.Price,
                    Quantity = i.Quantity
                }).ToList()
            })
            .ToListAsync();

        ViewBag.CurrentStatus = status ?? "AwaitingVerification";
        return View(orders);
    }

    // POST: /Admin/ApprovePayment
    [HttpPost("ApprovePayment")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApprovePayment(int paymentProofId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var proof = await _context.PaymentProofs
                .Include(p => p.Order).ThenInclude(o => o.User)
                .Include(p => p.Order).ThenInclude(o => o.Items).ThenInclude(i => i.Program)
                .FirstOrDefaultAsync(p => p.Id == paymentProofId);

            if (proof is null) return NotFound();

            // Check if already approved to avoid duplicate triggers
            if (proof.Status == PaymentStatus.Approved)
            {
                TempData["Success"] = "هذا الطلب معتمد بالفعل.";
                return RedirectToAction(nameof(Orders));
            }

            var adminUserId = _userManager.GetUserId(User)!;

            // 1. Update Payment and Order Status
            proof.Status = PaymentStatus.Approved;
            proof.ReviewedAt = DateTime.UtcNow;
            proof.ReviewedByUserId = adminUserId;
            proof.Order.Status = OrderStatus.Paid;

            // 2. Update Customer Total Spent
            if (proof.Order.User != null)
            {
                proof.Order.User.TotalSpent += proof.Order.Total;
            }

            // 3. Generate Download Tokens
            // Note: Service calls SaveChangesAsync internally for each token
            foreach (var item in proof.Order.Items)
            {
                await _orderService.GenerateDownloadTokenAsync(
                    proof.Order.Id, item.ProgramId, proof.Order.UserId);
            }

            // Current state of Order/Proof is also saved by the service calls above, 
            // but we call it once more to ensure everything is flushed.
            await _context.SaveChangesAsync();

            // 4. Create In-App Notification
            var downloadToken = await _context.DownloadTokens
                .FirstOrDefaultAsync(d => d.OrderId == proof.Order.Id);

            string downloadUrl = "#";
            string actionUrl = "#";

            if (downloadToken != null)
            {
                downloadUrl = Url.Action("Download", "Downloads", new { token = downloadToken.Token }, Request.Scheme) ?? "#";
                actionUrl = $"/Downloads/{downloadToken.Token}";
            }

            var programTitleAr = proof.Order.Items.FirstOrDefault()?.Program?.TitleAr ?? "البرنامج";
            var programTitleEn = proof.Order.Items.FirstOrDefault()?.Program?.TitleEn ?? "Program";

            var notification = new InAppNotification
            {
                UserId = proof.Order.UserId,
                TitleAr = "تم الموافقة على الدفع!",
                TitleEn = "Payment Approved!",
                MessageAr = $"تم تأكيد الدفع لبرنامج '{programTitleAr}'. يمكنك تحميل البرنامج الآن.",
                MessageEn = $"Payment confirmed for program '{programTitleEn}'. You can download it now.",
                ActionUrl = actionUrl
            };
            
            _context.InAppNotifications.Add(notification);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            // 5. Send Email (Async, non-blocking if possible, but here we wait to ensure logic completion)
            if (proof.Order.User != null && !string.IsNullOrEmpty(proof.Order.User.Email))
            {
                try 
                {
                    await _emailService.SendOrderApprovedEmailAsync(
                        proof.Order.User.Email, proof.Order.User.FullName,
                        proof.Order.OrderNumber, downloadUrl);
                }
                catch (Exception emailEx)
                {
                    _logger.LogWarning(emailEx, "Failed to send approval email to {Email}", proof.Order.User.Email);
                }
            }

            TempData["Success"] = $"تم اعتماد الطلب {proof.Order.OrderNumber} بنجاح";
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error approving payment for proof {Id}", paymentProofId);
            TempData["Error"] = "حدث خطأ أثناء اعتماد الطلب. يرجى التأكد من البيانات والمحاولة مرة أخرى.";
        }
        
        return RedirectToAction(nameof(Orders));
    }

    // POST: /Admin/RejectPayment
    [HttpPost("RejectPayment")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectPayment(int paymentProofId)
    {
        var proof = await _context.PaymentProofs
            .Include(p => p.Order).ThenInclude(o => o.User)
            .FirstOrDefaultAsync(p => p.Id == paymentProofId);

        if (proof is null) return NotFound();

        proof.Status = PaymentStatus.Rejected;
        proof.ReviewedAt = DateTime.UtcNow;
        proof.ReviewedByUserId = _userManager.GetUserId(User)!;
        proof.Order.Status = OrderStatus.Cancelled;

        await _context.SaveChangesAsync();

        await _emailService.SendOrderRejectedEmailAsync(
            proof.Order.User.Email!, proof.Order.User.FullName, proof.Order.OrderNumber);

        TempData["Success"] = $"تم رفض الطلب {proof.Order.OrderNumber}";
        return RedirectToAction(nameof(Orders));
    }

    // GET: /Admin/ViewProofImage
    [HttpGet("ViewProofImage")]
    public IActionResult ViewProofImage(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !System.IO.File.Exists(path))
            return NotFound();

        var contentType = Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };

        return PhysicalFile(path, contentType);
    }

    // ══════════════════════════════════════════════════════════
    //  PROGRAMS CRUD
    // ══════════════════════════════════════════════════════════

    // GET: /Admin/Programs
    [HttpGet("Programs")]
    public async Task<IActionResult> Programs()
    {
        var programs = await _context.Programs
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return View(programs);
    }

    // GET: /Admin/Programs/Create
    [HttpGet("Programs/Create")]
    public IActionResult CreateProgram()
    {
        return View(new ProgramFormViewModel());
    }

    // POST: /Admin/Programs/Create
    [HttpPost("Programs/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateProgram(ProgramFormViewModel model)
    {
        // Check slug uniqueness
        if (await _context.Programs.AnyAsync(p => p.Slug == model.Slug))
        {
            ModelState.AddModelError("Slug", "هذا الرابط المختصر مستخدم بالفعل");
        }

        if (model.PdfFile is null || model.PdfFile.Length == 0)
        {
            ModelState.AddModelError("PdfFile", "ملف PDF مطلوب");
        }

        if (!ModelState.IsValid)
            return View(model);

        // Save PDF to SecureFiles/
        var pdfFileName = $"{model.Slug}_{Guid.NewGuid():N}.pdf";
        var pdfPath = Path.Combine(_env.ContentRootPath, "SecureFiles", pdfFileName);
        Directory.CreateDirectory(Path.GetDirectoryName(pdfPath)!);
        await using (var stream = new FileStream(pdfPath, FileMode.Create))
        {
            await model.PdfFile!.CopyToAsync(stream);
        }

        // Save cover image to wwwroot/images/programs/
        string? coverImageUrl = null;
        if (model.CoverImage is not null && model.CoverImage.Length > 0)
        {
            coverImageUrl = await SaveCoverImageAsync(model.CoverImage, model.Slug);
        }

        var program = new WorkoutProgram
        {
            Slug = model.Slug,
            TitleAr = model.TitleAr,
            TitleEn = model.TitleEn,
            DescriptionAr = model.DescriptionAr,
            DescriptionEn = model.DescriptionEn,
            Price = model.Price,
            CompareAtPrice = model.CompareAtPrice,
            CoverImageUrl = coverImageUrl,
            PrivateFilePath = Path.Combine("SecureFiles", pdfFileName),
            IsActive = model.IsActive,
            IsFeatured = model.IsFeatured
        };

        _context.Programs.Add(program);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"تم إضافة البرنامج \"{model.TitleAr}\" بنجاح";
        return RedirectToAction(nameof(Programs));
    }

    // GET: /Admin/Programs/Edit/{id}
    [HttpGet("Programs/Edit/{id}")]
    public async Task<IActionResult> EditProgram(int id)
    {
        var program = await _context.Programs.FindAsync(id);
        if (program is null) return NotFound();

        var model = new ProgramFormViewModel
        {
            Id = program.Id,
            TitleAr = program.TitleAr,
            TitleEn = program.TitleEn,
            Slug = program.Slug,
            DescriptionAr = program.DescriptionAr,
            DescriptionEn = program.DescriptionEn,
            Price = program.Price,
            CompareAtPrice = program.CompareAtPrice,
            IsFeatured = program.IsFeatured,
            IsActive = program.IsActive,
            ExistingCoverImageUrl = program.CoverImageUrl,
            ExistingPdfFileName = Path.GetFileName(program.PrivateFilePath)
        };

        return View(model);
    }

    // POST: /Admin/Programs/Edit/{id}
    [HttpPost("Programs/Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProgram(int id, ProgramFormViewModel model)
    {
        var program = await _context.Programs.FindAsync(id);
        if (program is null) return NotFound();

        // Check slug uniqueness (excluding self)
        if (await _context.Programs.AnyAsync(p => p.Slug == model.Slug && p.Id != id))
        {
            ModelState.AddModelError("Slug", "هذا الرابط المختصر مستخدم بالفعل");
        }

        if (!ModelState.IsValid)
        {
            model.ExistingCoverImageUrl = program.CoverImageUrl;
            model.ExistingPdfFileName = Path.GetFileName(program.PrivateFilePath);
            return View(model);
        }

        // Update fields
        program.TitleAr = model.TitleAr;
        program.TitleEn = model.TitleEn;
        program.Slug = model.Slug;
        program.DescriptionAr = model.DescriptionAr;
        program.DescriptionEn = model.DescriptionEn;
        program.Price = model.Price;
        program.CompareAtPrice = model.CompareAtPrice;
        program.IsFeatured = model.IsFeatured;
        program.IsActive = model.IsActive;

        // Replace PDF if a new one is uploaded
        if (model.PdfFile is not null && model.PdfFile.Length > 0)
        {
            // Delete old PDF
            var oldPdfPath = Path.Combine(_env.ContentRootPath, program.PrivateFilePath);
            if (System.IO.File.Exists(oldPdfPath))
                System.IO.File.Delete(oldPdfPath);

            var pdfFileName = $"{model.Slug}_{Guid.NewGuid():N}.pdf";
            var pdfPath = Path.Combine(_env.ContentRootPath, "SecureFiles", pdfFileName);
            await using (var stream = new FileStream(pdfPath, FileMode.Create))
            {
                await model.PdfFile.CopyToAsync(stream);
            }
            program.PrivateFilePath = Path.Combine("SecureFiles", pdfFileName);
        }

        // Replace cover image if a new one is uploaded
        if (model.CoverImage is not null && model.CoverImage.Length > 0)
        {
            // Delete old cover
            if (!string.IsNullOrEmpty(program.CoverImageUrl))
            {
                var oldCoverPath = Path.Combine(_env.WebRootPath, program.CoverImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldCoverPath))
                    System.IO.File.Delete(oldCoverPath);
            }
            program.CoverImageUrl = await SaveCoverImageAsync(model.CoverImage, model.Slug);
        }

        await _context.SaveChangesAsync();

        TempData["Success"] = $"تم تحديث البرنامج \"{model.TitleAr}\" بنجاح";
        return RedirectToAction(nameof(Programs));
    }

    // POST: /Admin/Programs/Delete/{id}
    [HttpPost("Programs/Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProgram(int id)
    {
        var program = await _context.Programs.FindAsync(id);
        if (program is null) return NotFound();

        // Soft Delete
        program.IsDeleted = true;
        program.IsActive = false;

        await _context.SaveChangesAsync();

        TempData["Success"] = $"تم حذف البرنامج \"{program.TitleAr}\" بنجاح";
        return RedirectToAction(nameof(Programs));
    }

    // ══════════════════════════════════════════════════════════
    //  TRANSFORMATIONS CRUD
    // ══════════════════════════════════════════════════════════

    // GET: /Admin/Transformations
    [HttpGet("Transformations")]
    public async Task<IActionResult> Transformations()
    {
        var transformations = await _context.Transformations
            .OrderBy(t => t.DisplayOrder)
            .ToListAsync();

        return View(transformations);
    }

    // GET: /Admin/Transformations/Create
    [HttpGet("Transformations/Create")]
    public IActionResult CreateTransformation()
    {
        return View(new TransformationFormViewModel { DisplayOrder = 1, IsVisible = true });
    }

    // POST: /Admin/Transformations/Create
    [HttpPost("Transformations/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTransformation(TransformationFormViewModel model)
    {
        if (model.BeforeImage == null || model.BeforeImage.Length == 0)
            ModelState.AddModelError("BeforeImage", "صورة 'قبل' مطلوبة");

        if (model.AfterImage == null || model.AfterImage.Length == 0)
            ModelState.AddModelError("AfterImage", "صورة 'بعد' مطلوبة");

        if (!ModelState.IsValid)
            return View(model);

        var beforeUrl = await SaveTransformationImageAsync(model.BeforeImage!, "before");
        var afterUrl = await SaveTransformationImageAsync(model.AfterImage!, "after");

        var transformation = new Transformation
        {
            TitleAr = model.TitleAr,
            TitleEn = model.TitleEn,
            DescriptionAr = model.DescriptionAr,
            DescriptionEn = model.DescriptionEn,
            BeforeImageUrl = beforeUrl,
            AfterImageUrl = afterUrl,
            IsVisible = model.IsVisible,
            DisplayOrder = model.DisplayOrder
        };

        _context.Transformations.Add(transformation);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"تم إضافة التحول {model.TitleAr} بنجاح";
        return RedirectToAction(nameof(Transformations));
    }

    // GET: /Admin/Transformations/Edit/{id}
    [HttpGet("Transformations/Edit/{id}")]
    public async Task<IActionResult> EditTransformation(int id)
    {
        var t = await _context.Transformations.FindAsync(id);
        if (t == null) return NotFound();

        var model = new TransformationFormViewModel
        {
            Id = t.Id,
            TitleAr = t.TitleAr,
            TitleEn = t.TitleEn,
            DescriptionAr = t.DescriptionAr,
            DescriptionEn = t.DescriptionEn,
            IsVisible = t.IsVisible,
            DisplayOrder = t.DisplayOrder,
            ExistingBeforeImageUrl = t.BeforeImageUrl,
            ExistingAfterImageUrl = t.AfterImageUrl
        };

        return View(model);
    }

    // POST: /Admin/Transformations/Edit/{id}
    [HttpPost("Transformations/Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditTransformation(int id, TransformationFormViewModel model)
    {
        var t = await _context.Transformations.FindAsync(id);
        if (t == null) return NotFound();

        if (!ModelState.IsValid)
        {
            model.ExistingBeforeImageUrl = t.BeforeImageUrl;
            model.ExistingAfterImageUrl = t.AfterImageUrl;
            return View(model);
        }

        t.TitleAr = model.TitleAr;
        t.TitleEn = model.TitleEn;
        t.DescriptionAr = model.DescriptionAr;
        t.DescriptionEn = model.DescriptionEn;
        t.IsVisible = model.IsVisible;
        t.DisplayOrder = model.DisplayOrder;

        if (model.BeforeImage != null && model.BeforeImage.Length > 0)
        {
            DeleteImageFile(t.BeforeImageUrl);
            t.BeforeImageUrl = await SaveTransformationImageAsync(model.BeforeImage, "before");
        }

        if (model.AfterImage != null && model.AfterImage.Length > 0)
        {
            DeleteImageFile(t.AfterImageUrl);
            t.AfterImageUrl = await SaveTransformationImageAsync(model.AfterImage, "after");
        }

        await _context.SaveChangesAsync();

        TempData["Success"] = $"تم تحديث التحول {model.TitleAr} بنجاح";
        return RedirectToAction(nameof(Transformations));
    }

    // POST: /Admin/Transformations/Delete/{id}
    [HttpPost("Transformations/Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTransformation(int id)
    {
        var t = await _context.Transformations.FindAsync(id);
        if (t == null) return NotFound();

        DeleteImageFile(t.BeforeImageUrl);
        DeleteImageFile(t.AfterImageUrl);

        _context.Transformations.Remove(t);
        await _context.SaveChangesAsync();

        TempData["Success"] = "تم حذف التحول بنجاح";
        return RedirectToAction(nameof(Transformations));
    }

    // ══════════════════════════════════════════════════════════
    //  CUSTOMERS
    // ══════════════════════════════════════════════════════════

    // GET: /Admin/Customers
    [HttpGet("Customers")]
    public async Task<IActionResult> Customers(string? search)
    {
        // Get all users in the 'Client' role
        var clientsInsideRole = await _userManager.GetUsersInRoleAsync("Client");
        var clientIds = clientsInsideRole.Select(u => u.Id).ToList();

        var query = _context.Users
            .Where(u => clientIds.Contains(u.Id))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(u => 
                (u.FullName != null && u.FullName.ToLower().Contains(searchLower)) ||
                (u.Email != null && u.Email.ToLower().Contains(searchLower)) ||
                (u.PhoneNumber != null && u.PhoneNumber.Contains(searchLower)));
        }

        var customers = await query
            .OrderByDescending(u => u.TotalSpent)
            .Select(u => new
            {
                u.Id,
                u.FullName,
                u.Email,
                u.PhoneNumber,
                u.TotalSpent,
                OrderCount = _context.Orders.Count(o => o.UserId == u.Id)
            })
            .ToListAsync();

        ViewBag.SearchQuery = search;
        return View(customers);
    }

    // GET: /Admin/CustomerDetails/{id}
    [HttpGet("CustomerDetails/{id}")]
    public async Task<IActionResult> CustomerDetails(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var orders = await _context.Orders
            .Include(o => o.Items).ThenInclude(i => i.Program)
            .Where(o => o.UserId == id)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        var model = new AdminCustomerDetailsViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email!,
            PhoneNumber = user.PhoneNumber,
            TotalSpent = user.TotalSpent,
            OrderCount = orders.Count,
            // Since we don't track CreatedAt on IdentityUser directly unless configured, 
            // we'll default to the earliest order date or use a placeholder if needed, 
            // but for now we just show OrderCount and TotalSpent.
            Orders = orders.Select(o => new AdminCustomerOrderHistoryViewModel
            {
                OrderId = o.Id,
                OrderNumber = o.OrderNumber,
                Total = o.Total,
                CreatedAt = o.CreatedAt,
                Status = o.Status,
                Items = o.Items.Select(i => i.Program.TitleAr).ToList()
            }).ToList()
        };

        return View(model);
    }

    // ══════════════════════════════════════════════════════════
    //  HELPERS
    // ══════════════════════════════════════════════════════════

    private void DeleteImageFile(string? url)
    {
        if (string.IsNullOrEmpty(url)) return;
        var path = Path.Combine(_env.WebRootPath, url.TrimStart('/'));
        if (System.IO.File.Exists(path))
            System.IO.File.Delete(path);
    }

    private async Task<string> SaveTransformationImageAsync(IFormFile file, string prefix)
    {
        var imagesDir = Path.Combine(_env.WebRootPath, "images", "transformations");
        Directory.CreateDirectory(imagesDir);

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{prefix}_{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(imagesDir, fileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/images/transformations/{fileName}";
    }

    private async Task<string> SaveCoverImageAsync(IFormFile file, string slug)
    {
        var imagesDir = Path.Combine(_env.WebRootPath, "images", "programs");
        Directory.CreateDirectory(imagesDir);

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{slug}_{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(imagesDir, fileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/images/programs/{fileName}";
    }
}
