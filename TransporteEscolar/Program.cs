using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using TransporteEscolar.Data;
using TransporteEscolar.Repositories;
using TransporteEscolar.Repositories.Interfaces;
using TransporteEscolar.Services;
using TransporteEscolar.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ── MVC ──────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

// ── Entity Framework Core + SQL Server ───────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure(3)));

// ── Repositorios y Servicios (DI) ─────────────────────────────
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<TransporteEscolar.Services.ExportService>();

builder.Services.AddScoped<TransporteEscolar.Services.ImportExportService>();
builder.Services.AddScoped<TransporteEscolar.Services.NotificacionService>();

// ── Autenticación con Cookies ─────────────────────────────────
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccesoDenegado";
        options.Cookie.Name = "TE_Auth";
        options.Cookie.HttpOnly = true;          // No accesible desde JS
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Solo HTTPS
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

builder.Services.AddAuthorization();

// ── Anti-forgery ──────────────────────────────────────────────
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10MB máximo
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10MB
});

var app = builder.Build();

// ── Pipeline HTTP ─────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();  // ← Siempre ANTES de Authorization
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();