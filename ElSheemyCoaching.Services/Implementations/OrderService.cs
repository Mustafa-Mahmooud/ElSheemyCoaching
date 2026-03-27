using ElSheemyCoaching.Data;
using ElSheemyCoaching.Core.Entities;
using ElSheemyCoaching.Core.Enums;
using Microsoft.EntityFrameworkCore;
using ElSheemyCoaching.Core.Interfaces;

namespace ElSheemyCoaching.Services.Implementations;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;

    public OrderService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Order> CreateOrderAsync(string userId, int programId, int? variantId, string? couponCode)
    {
        var program = await _context.Programs.FindAsync(programId)
            ?? throw new InvalidOperationException("البرنامج غير موجود");

        decimal price = program.Price;

        // If a variant is selected, use its price
        if (variantId.HasValue)
        {
            var variant = await _context.ProgramVariants.FindAsync(variantId.Value)
                ?? throw new InvalidOperationException("نوع البرنامج غير موجود");
            price = variant.Price;
        }

        decimal total = price;
        int? couponId = null;

        // Apply coupon if provided
        if (!string.IsNullOrWhiteSpace(couponCode))
        {
            var coupon = await _context.Coupons
                .FirstOrDefaultAsync(c => c.Code == couponCode && c.IsActive && c.CurrentUses < c.MaxUses);

            if (coupon != null && (coupon.ExpiresAt == null || coupon.ExpiresAt > DateTime.UtcNow))
            {
                total = price - (price * coupon.DiscountPercent / 100m);
                couponId = coupon.Id;
                coupon.CurrentUses++;
            }
        }

        var order = new Order
        {
            OrderNumber = GenerateOrderNumber(),
            UserId = userId,
            Status = OrderStatus.AwaitingVerification,
            Total = total,
            CouponId = couponId
        };

        order.Items.Add(new OrderItem
        {
            ProgramId = programId,
            ProgramVariantId = variantId,
            Price = price,
            Quantity = 1
        });

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return order;
    }

    public string GenerateOrderNumber()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
        var random = Random.Shared.Next(1000, 9999);
        return $"ESC-{timestamp}-{random}";
    }

    public async Task<DownloadToken> GenerateDownloadTokenAsync(int orderId, int programId, string userId)
    {
        var token = new DownloadToken
        {
            OrderId = orderId,
            ProgramId = programId,
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _context.DownloadTokens.Add(token);
        await _context.SaveChangesAsync();

        return token;
    }
}
