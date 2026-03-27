using ElSheemyCoaching.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElSheemyCoaching.Services.Implementations
{
    public class AbandonedOrderCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AbandonedOrderCleanupService> _logger;

        public AbandonedOrderCleanupService(
            IServiceScopeFactory scopeFactory,
            ILogger<AbandonedOrderCleanupService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var abandoned = await context.Orders
                        .Where(o => o.PaymentProof == null &&
                                    o.CreatedAt < DateTime.UtcNow.AddHours(-24))
                        .ToListAsync(stoppingToken);

                    if (abandoned.Any())
                    {
                        context.Orders.RemoveRange(abandoned);
                        await context.SaveChangesAsync(stoppingToken);
                        _logger.LogInformation("Deleted {Count} abandoned orders.", abandoned.Count);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during abandoned order cleanup.");
                }

                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}
