using ElSheemyCoaching.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ElSheemyCoaching.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Ensure database is created
        await context.Database.MigrateAsync();

        // ───────── Seed Roles ─────────
        string[] roles = ["Admin", "Coach", "Client"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // ───────── Seed Admin User ─────────
        const string adminEmail = "admin@elsheemy.com";
        if (await userManager.FindByEmailAsync(adminEmail) is null)
        {
            var admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "كابتن الشيمي",
                PhoneNumber = "+201000000000",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
                await userManager.AddToRoleAsync(admin, "Coach");
            }
        }

        // ───────── Seed Sample Programs ─────────
        if (!await context.Programs.IgnoreQueryFilters().AnyAsync())
        {
            var programs = new List<WorkoutProgram>
            {
                new()
                {
                    Slug = "beginner-body-transformation",
                    TitleAr = "برنامج تحول الجسم للمبتدئين",
                    TitleEn = "Beginner Body Transformation",
                    DescriptionAr = "برنامج تدريبي شامل مدته 12 أسبوع مصمم خصيصاً للمبتدئين. يتضمن خطة تغذية كاملة وتمارين مصورة بالفيديو.",
                    DescriptionEn = "A comprehensive 12-week training program designed for beginners. Includes a complete nutrition plan and video-illustrated exercises.",
                    Price = 499.00m,
                    CompareAtPrice = 799.00m,
                    CoverImageUrl = "/images/programs/beginner.jpg",
                    PrivateFilePath = "SecureFiles/beginner-body-transformation.pdf",
                    IsActive = true,
                    IsFeatured = true
                },
                new()
                {
                    Slug = "advanced-muscle-building",
                    TitleAr = "برنامج بناء العضلات المتقدم",
                    TitleEn = "Advanced Muscle Building",
                    DescriptionAr = "برنامج متقدم لبناء العضلات مدته 16 أسبوع. يشمل تقنيات التدريب المتقدمة وخطط التغذية المخصصة لزيادة الكتلة العضلية.",
                    DescriptionEn = "An advanced 16-week muscle building program. Includes advanced training techniques and custom nutrition plans for muscle mass gain.",
                    Price = 799.00m,
                    CompareAtPrice = 1199.00m,
                    CoverImageUrl = "/images/programs/advanced.jpg",
                    PrivateFilePath = "SecureFiles/advanced-muscle-building.pdf",
                    IsActive = true,
                    IsFeatured = true
                },
                new()
                {
                    Slug = "home-workout-essentials",
                    TitleAr = "أساسيات التمرين المنزلي",
                    TitleEn = "Home Workout Essentials",
                    DescriptionAr = "برنامج تدريب منزلي بدون معدات مدته 8 أسابيع. مثالي لمن لا يستطيع الذهاب للجيم. يتضمن تمارين بوزن الجسم وخطة غذائية.",
                    DescriptionEn = "An 8-week no-equipment home workout program. Perfect for those who can't go to the gym. Includes bodyweight exercises and a meal plan.",
                    Price = 299.00m,
                    CompareAtPrice = 499.00m,
                    CoverImageUrl = "/images/programs/home.jpg",
                    PrivateFilePath = "SecureFiles/home-workout-essentials.pdf",
                    IsActive = true,
                    IsFeatured = false
                }
            };

            context.Programs.AddRange(programs);
            await context.SaveChangesAsync();
        }
    }
}
