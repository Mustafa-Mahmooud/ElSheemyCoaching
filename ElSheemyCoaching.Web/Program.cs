using ElSheemyCoaching.Data;
using ElSheemyCoaching.Core.Entities;
using ElSheemyCoaching.Core.Interfaces;
using ElSheemyCoaching.Services.Implementations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// ═══════════ DATABASE ═══════════
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ═══════════ IDENTITY ═══════════
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;

    // User settings
    options.User.RequireUniqueEmail = true;

    // Sign-in settings
    options.SignIn.RequireConfirmedEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ═══════════ COOKIE AUTH ═══════════
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
});

// ═══════════ LOCALIZATION ═══════════
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

// ═══════════ SERVICES ═══════════
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<IFoodAlternativeService, FoodAlternativeService>();

builder.Services.AddHostedService<AbandonedOrderCleanupService>();

var app = builder.Build();

// ═══════════ MIDDLEWARE ═══════════
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseMiddleware<ElSheemyCoaching.Middleware.GlobalExceptionMiddleware>();

app.UseHsts();
app.UseHttpsRedirection();
app.UseStaticFiles();

// ═══════════ REQUEST LOCALIZATION ═══════════
var supportedCultures = new[]
{
    new CultureInfo("ar-EG"),
    new CultureInfo("en-US")
};

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("ar-EG"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures,
    RequestCultureProviders = new List<IRequestCultureProvider>
    {
        new CookieRequestCultureProvider()
    }
});


app.UseDeveloperExceptionPage(); // add this before app.UseRouting()

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ═══════════ SEED DATA ═══════════
await SeedData.InitializeAsync(app.Services);

app.Run();
