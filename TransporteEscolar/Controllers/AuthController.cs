using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using TransporteEscolar.Services.Interfaces;
using TransporteEscolar.ViewModels;

namespace TransporteEscolar.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
            => _authService = authService;

        // GET /Auth/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // Si ya está autenticado, redirigir según rol
            if (User.Identity?.IsAuthenticated == true)
                return RedirectSegunRol();

            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        // POST /Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel modelo, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(modelo);

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocido";
            var userAgent = Request.Headers["User-Agent"].ToString();

            var (exitoso, error, principal) = await _authService.ValidarLoginAsync(modelo, ip, userAgent);

            if (!exitoso)
            {
                ModelState.AddModelError(string.Empty, error ?? "Error al iniciar sesión.");
                return View(modelo);
            }

            // Configurar cookie de autenticación
            var propiedades = new AuthenticationProperties
            {
                IsPersistent = modelo.RememberMe,
                ExpiresUtc = modelo.RememberMe
                    ? DateTimeOffset.UtcNow.AddDays(30)
                    : DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal!,
                propiedades);

            // Redirigir según rol
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectSegunRol();
        }

        // POST /Auth/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // GET /Auth/AccesoDenegado
        [HttpGet]
        public IActionResult AccesoDenegado()
            => View();

        private IActionResult RedirectSegunRol()
        {
            var rol = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            return rol switch
            {
                "Admin" => RedirectToAction("Dashboard", "Admin"),
                "Azafata" => RedirectToAction("SeleccionarChofer", "Azafata"),
                "Padre" => RedirectToAction("Dashboard", "Padre"),
                _ => RedirectToAction("Login")
            };
        }
    }
}