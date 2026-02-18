using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Radzen;
using TicketSystem.Components;
using TicketSystem.Components.Account;
using TicketSystem.Data;
using TicketSystem.Hubs;
using TicketSystem.Model;
using TicketSystem.Services;
using TicketSystem.Services.Export;
using TicketSystem.Services.WPX;
using QuestPDF.Infrastructure;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// ---------------------- Logging ----------------------
builder.Logging.ClearProviders(); // إزالة كل الـ Providers
builder.Logging.SetMinimumLevel(LogLevel.None); // لا يظهر أي Log نهائيًا

// ---------------------- Services ----------------------
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddRadzenComponents();
builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<OrderFixedService>();
builder.Services.AddScoped<SectionService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<CurrentUserModel>();
builder.Services.AddScoped<RoleService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor()
       .AddCircuitOptions(options => options.DetailedErrors = false); // منع ظهور الأخطاء في الصفحة
builder.Services.AddAuthentication();
builder.Services.AddBlazorBootstrap();
builder.Services.AddScoped<NotificationService1>();
builder.Services.AddScoped<UserContextService>();
builder.Services.AddScoped<ExportService>();
builder.Services.AddScoped<RateService>();
builder.Services.AddScoped<RawDataService>();
builder.Services.AddScoped<PermissionService>();
builder.Services.AddSignalR();

QuestPDF.Settings.License = LicenseType.Community;

// ---------------------- DB Context ----------------------
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           .LogTo(_ => { }, LogLevel.None); // منع أي تحذيرات EF Core
});

// ---------------------- Identity ----------------------
builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 10;
    options.Lockout.AllowedForNewUsers = true;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddSignInManager()
.AddErrorDescriber<CustomIdentityErrorDescriber>()
.AddDefaultTokenProviders();

// ---------------------- Cookies ----------------------
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
    options.LoginPath = "/Account/Login";
});

// ---------------------- Authentication & Authorization ----------------------
builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme).AddIdentityCookies();
builder.Services.AddAuthorization();

// ---------------------- QuickGrid & Others ----------------------
builder.Services.AddQuickGridEntityFrameworkAdapter();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

// ---------------------- HSTS ----------------------
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});

// ---------------------- App ----------------------
var app = builder.Build();

// ---------------------- Middleware ----------------------
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseHsts();
app.UseAntiforgery();

// Security Headers
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    await next();
});

// Logout endpoint
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/logout")
    {
        await context.SignOutAsync(IdentityConstants.ApplicationScheme);
        foreach (var cookie in context.Request.Cookies.Keys)
            context.Response.Cookies.Delete(cookie);
        context.Response.Redirect("/");
        return;
    }
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

// ---------------------- SignalR ----------------------
app.MapHub<NotificationHub>("/notificationHub").RequireAuthorization();

// ---------------------- Seed Admin User ----------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    var roles = new[] { "Admin", "Customer" };
    foreach (var role in roles)
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));

    var adminEmail = "admin@system.com";
    var adminPassword = "Admin@123";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        await userManager.CreateAsync(adminUser, adminPassword);
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }

}

// ---------------------- Pipeline ----------------------
//app.UseExceptionHandler("/Error"); // منع ظهور الأخطاء في الصفحة

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.MapAdditionalIdentityEndpoints();

app.Run();