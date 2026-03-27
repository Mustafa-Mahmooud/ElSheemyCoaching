using ElSheemyCoaching.Core.Entities;

namespace ElSheemyCoaching.Core.Interfaces;

public interface IOrderService
{
    Task<Order> CreateOrderAsync(string userId, int programId, int? variantId, string? couponCode);
    string GenerateOrderNumber();
    Task<DownloadToken> GenerateDownloadTokenAsync(int orderId, int programId, string userId);
}
