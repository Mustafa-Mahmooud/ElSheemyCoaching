using ElSheemyCoaching.Data;
using ElSheemyCoaching.Core.Entities;
using ElSheemyCoaching.Core.Interfaces;
using ElSheemyCoaching.Services.Implementations;
using ElSheemyCoaching.Core.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElSheemyCoaching.Controllers;

[Authorize]
public class CheckoutController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IOrderService _orderService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _env;

    public CheckoutController(
        ApplicationDbContext context,
        IOrderService orderService,
        UserManager<ApplicationUser> userManager,
        IWebHostEnvironment env)
    {
        _context = context;
        _orderService = orderService;
        _userManager = userManager;
        _env = env;
    }

    // POST: /Checkout/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int programId, int? variantId, string? couponCode)
    {
        var userId = _userManager.GetUserId(User)!;

        try
        {
            var order = await _orderService.CreateOrderAsync(userId, programId, variantId, couponCode);
            return RedirectToAction(nameof(InstaPayUpload), new { orderId = order.Id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("Index", "Programs");
        }
    }

    // GET: /Checkout/InstaPayUpload?orderId=X
    [HttpGet]
    public async Task<IActionResult> InstaPayUpload(int orderId)
    {
        var userId = _userManager.GetUserId(User);
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        if (order is null)
            return NotFound();

        var model = new InstaPayUploadViewModel
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            Total = order.Total,
            InstaPayHandle = "https://ipn.eg/S/elsheemmyy/instapay/4omfpu" 
        };

        return View(model);
    }

    // POST: /Checkout/SubmitProof
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitProof(InstaPayUploadViewModel model)
    {
        var userId = _userManager.GetUserId(User);
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == model.OrderId && o.UserId == userId);

        if (order is null)
            return NotFound();

        if (model.ProofImage is null || model.ProofImage.Length == 0)
        {
            ModelState.AddModelError("ProofImage", "يرجى رفع صورة إثبات الدفع");
            model.OrderNumber = order.OrderNumber;
            model.Total = order.Total;
            model.InstaPayHandle = "elsheemy_coaching";
            return View(nameof(InstaPayUpload), model);
        }

        // Save proof image to Uploads/PaymentProofs/
        var uploadsPath = Path.Combine(_env.ContentRootPath, "Uploads", "PaymentProofs");
        Directory.CreateDirectory(uploadsPath);

        var fileName = $"{order.OrderNumber}_{Guid.NewGuid():N}{Path.GetExtension(model.ProofImage.FileName)}";
        var filePath = Path.Combine(uploadsPath, fileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await model.ProofImage.CopyToAsync(stream);
        }

        var paymentProof = new PaymentProof
        {
            OrderId = order.Id,
            ProofImagePath = filePath,
            TransactionRef = model.TransactionRef,
            Amount = order.Total
        };

        _context.PaymentProofs.Add(paymentProof);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Success), new { orderId = order.Id });
    }

    // GET: /Checkout/Success?orderId=X
    [HttpGet]
    public async Task<IActionResult> Success(int orderId)
    {
        var userId = _userManager.GetUserId(User);
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        if (order is null)
            return NotFound();

        return View(order);
    }

    // POST: /Checkout/Cancel
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int orderId)
    {
        var userId = _userManager.GetUserId(User);
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId && o.PaymentProof == null);

        if (order != null)
        {
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Index", "Programs");
    }
}
