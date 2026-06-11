using API_tester.Data;
using API_tester.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Localization;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using System;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});
builder.Services.AddRazorPages();
builder.Services.AddHttpClient<ApiRequestExecutor>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(60);
});

var connectionString = builder.Configuration.GetConnectionString("ApiTesterDb")
    ?? throw new InvalidOperationException("Connection string 'ApiTesterDb' was not found.");

// Use an explicit server version to avoid a network probe during design-time tools.
var serverVersion = new MySqlServerVersion(new Version(8, 0, 33));
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, serverVersion));

// Identity with integer keys
builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
{
    builder.Services.AddAuthentication()
        .AddGoogle(options =>
        {
            options.ClientId = googleClientId;
            options.ClientSecret = googleClientSecret;
        });
}

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest
        : Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // Schema adjustments should be done via EF migrations, not runtime SQL.
    // If you need to make the `OwnerUserId` column nullable, create and apply a migration instead.

    // Seed Identity roles and an initial admin user (dev-friendly: configurable)
    try
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

        // DO NOT perform schema changes at runtime. Use EF migrations to make persistent schema updates.
        // The database was adjusted manually during development to ensure Identity Id columns are
        // AUTO_INCREMENT. Remove any runtime SQL fixes and track schema changes with migrations.

        var roles = new[] { "Admin", "Manager" };
        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                int nextRoleId = 1;
                try
                {
                    if (await roleManager.Roles.AnyAsync())
                    {
                        nextRoleId = await roleManager.Roles.MaxAsync(r => r.Id) + 1;
                    }
                }
                catch
                {
                    // If querying fails, fall back to 1
                    nextRoleId = 1;
                }

                var role = new IdentityRole<int> { Id = nextRoleId, Name = roleName, NormalizedName = roleName.ToUpperInvariant() };
                await roleManager.CreateAsync(role);
            }
        }

        var adminEmail = builder.Configuration["SeedAdminEmail"] ?? "admin@local";
        var adminPassword = builder.Configuration["SeedAdminPassword"] ?? "ChangeMe123!";

        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin == null)
        {
            admin = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                Role = "Admin",
                OIB = "00000000000",
                JMBG = "0000000000000"
            };

            var createResult = await userManager.CreateAsync(admin, adminPassword);
            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
        else if (!await userManager.IsInRoleAsync(admin, "Admin"))
        {
            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
    catch
    {
        // Seeding should not block startup in dev scenarios; continue silently
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

var supportedCultures = new[]
{
    new CultureInfo("hr"),
    new CultureInfo("en-US")
};

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("hr"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

await app.RunAsync();

public partial class Program { }
