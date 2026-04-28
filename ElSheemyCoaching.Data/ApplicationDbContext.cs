using ElSheemyCoaching.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ElSheemyCoaching.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<WorkoutProgram> Programs => Set<WorkoutProgram>();
    public DbSet<ProgramVariant> ProgramVariants => Set<ProgramVariant>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<PaymentProof> PaymentProofs => Set<PaymentProof>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<DownloadToken> DownloadTokens => Set<DownloadToken>();
    public DbSet<Transformation> Transformations => Set<Transformation>();
    public DbSet<InAppNotification> InAppNotifications => Set<InAppNotification>();
    public DbSet<UserProgress> UserProgresses => Set<UserProgress>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ───────────────── ApplicationUser ─────────────────
        builder.Entity<ApplicationUser>(e =>
        {
            e.Property(u => u.FullName).HasMaxLength(200);
            e.Property(u => u.TotalSpent).HasPrecision(18, 2);
        });

        // ───────────────── WorkoutProgram ─────────────────
        builder.Entity<WorkoutProgram>(e =>
        {
            e.HasQueryFilter(p => !p.IsDeleted);
            e.HasIndex(p => p.Slug).IsUnique();
            e.Property(p => p.Slug).HasMaxLength(200);
            e.Property(p => p.TitleAr).HasMaxLength(300);
            e.Property(p => p.TitleEn).HasMaxLength(300);
            e.Property(p => p.Price).HasPrecision(18, 2);
            e.Property(p => p.CompareAtPrice).HasPrecision(18, 2);
            e.Property(p => p.PrivateFilePath).HasMaxLength(500);
            e.Property(p => p.CoverImageUrl).HasMaxLength(500);
        });

        // ───────────────── ProgramVariant ─────────────────
        builder.Entity<ProgramVariant>(e =>
        {
            e.Property(v => v.Price).HasPrecision(18, 2);
            e.Property(v => v.NameAr).HasMaxLength(200);
            e.Property(v => v.NameEn).HasMaxLength(200);
            e.Property(v => v.FilePath).HasMaxLength(500);

            e.HasOne(v => v.Program)
             .WithMany(p => p.Variants)
             .HasForeignKey(v => v.ProgramId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasQueryFilter(v => !v.Program.IsDeleted);
        });

        // ───────────────── Order ─────────────────
        builder.Entity<Order>(e =>
        {
            e.HasIndex(o => o.OrderNumber).IsUnique();
            e.Property(o => o.OrderNumber).HasMaxLength(50);
            e.Property(o => o.Total).HasPrecision(18, 2);

            e.HasOne(o => o.User)
             .WithMany(u => u.Orders)
             .HasForeignKey(o => o.UserId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(o => o.Coupon)
             .WithMany(c => c.Orders)
             .HasForeignKey(o => o.CouponId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ───────────────── OrderItem ─────────────────
        builder.Entity<OrderItem>(e =>
        {
            e.Property(i => i.Price).HasPrecision(18, 2);

            e.HasOne(i => i.Order)
             .WithMany(o => o.Items)
             .HasForeignKey(i => i.OrderId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(i => i.Program)
             .WithMany(p => p.OrderItems)
             .HasForeignKey(i => i.ProgramId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasQueryFilter(i => !i.Program.IsDeleted);

            e.HasOne(i => i.ProgramVariant)
             .WithMany(v => v.OrderItems)
             .HasForeignKey(i => i.ProgramVariantId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ───────────────── PaymentProof ─────────────────
        builder.Entity<PaymentProof>(e =>
        {
            e.Property(p => p.Amount).HasPrecision(18, 2);
            e.Property(p => p.ProofImagePath).HasMaxLength(500);
            e.Property(p => p.TransactionRef).HasMaxLength(200);

            e.HasOne(p => p.Order)
             .WithOne(o => o.PaymentProof)
             .HasForeignKey<PaymentProof>(p => p.OrderId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(p => p.ReviewedBy)
             .WithMany()
             .HasForeignKey(p => p.ReviewedByUserId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ───────────────── Coupon ─────────────────
        builder.Entity<Coupon>(e =>
        {
            e.HasIndex(c => c.Code).IsUnique();
            e.Property(c => c.Code).HasMaxLength(50);
        });

        // ───────────────── DownloadToken ─────────────────
        builder.Entity<DownloadToken>(e =>
        {
            e.HasIndex(d => d.Token).IsUnique();
            e.Property(d => d.Token).HasMaxLength(64);

            e.HasOne(d => d.Order)
             .WithMany(o => o.DownloadTokens)
             .HasForeignKey(d => d.OrderId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(d => d.Program)
             .WithMany()
             .HasForeignKey(d => d.ProgramId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasQueryFilter(d => !d.Program.IsDeleted);

            e.HasOne(d => d.User)
             .WithMany()
             .HasForeignKey(d => d.UserId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ───────────────── Transformation ─────────────────
        builder.Entity<Transformation>(e =>
        {
            e.Property(t => t.TitleAr).HasMaxLength(300);
            e.Property(t => t.TitleEn).HasMaxLength(300);
            e.Property(t => t.BeforeImageUrl).HasMaxLength(500);
            e.Property(t => t.AfterImageUrl).HasMaxLength(500);
        });

        // ───────────────── InAppNotification ─────────────────
        builder.Entity<InAppNotification>(e =>
        {
            e.Property(n => n.TitleAr).HasMaxLength(200);
            e.Property(n => n.TitleEn).HasMaxLength(200);
            e.Property(n => n.ActionUrl).HasMaxLength(500);

            e.HasOne(n => n.User)
             .WithMany()
             .HasForeignKey(n => n.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
