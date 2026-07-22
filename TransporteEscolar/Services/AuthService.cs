using System.Security.Claims;
using TransporteEscolar.Helpers;
using TransporteEscolar.Models;
using TransporteEscolar.Repositories.Interfaces;
using TransporteEscolar.Services.Interfaces;
using TransporteEscolar.ViewModels;

namespace TransporteEscolar.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUsuarioRepository _usuarioRepo;

        public AuthService(IUsuarioRepository usuarioRepo)
            => _usuarioRepo = usuarioRepo;

        public async Task<(bool Exitoso, string? Error, ClaimsPrincipal? Principal)>
            ValidarLoginAsync(LoginViewModel modelo, string ip, string userAgent)
        {
            var usuario = await _usuarioRepo.ObtenerPorNombreUsuarioAsync(modelo.NombreUsuario);

            if (usuario is null)
            {
                await RegistrarBitacora(null, modelo.NombreUsuario, false, ip, userAgent,
                    "Usuario no encontrado");
                return (false, "Credenciales incorrectas.", null);
            }

            if (usuario.Bloqueado)
            {
                await RegistrarBitacora(usuario.UsuarioId, modelo.NombreUsuario, false, ip,
                    userAgent, "Cuenta bloqueada");
                return (false, "Tu cuenta está bloqueada. Contacta al administrador.", null);
            }

            if (!PasswordHelper.VerifyPassword(modelo.Password, usuario.PasswordHash))
            {
                await _usuarioRepo.IncrementarIntentosFallidosAsync(usuario.UsuarioId);
                await RegistrarBitacora(usuario.UsuarioId, modelo.NombreUsuario, false, ip,
                    userAgent, "Contraseña incorrecta");
                return (false, "Credenciales incorrectas.", null);
            }

            // ── Bloque eliminado: validación de FichaAutobus ──
            // La azafata selecciona su autobús después del login

            await _usuarioRepo.ActualizarUltimoAccesoAsync(usuario.UsuarioId);
            await RegistrarBitacora(usuario.UsuarioId, modelo.NombreUsuario, true, ip,
                userAgent, "Acceso exitoso");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioId.ToString()),
                new Claim(ClaimTypes.Name,           usuario.NombreUsuario),
                new Claim(ClaimTypes.GivenName,      usuario.NombreCompleto),
                new Claim(ClaimTypes.Role,           usuario.Rol.Nombre),
            };

            if (usuario.Autobus is not null)
                claims.Add(new Claim("AutobusId", usuario.AutobusId.ToString()!));

            if (!string.IsNullOrEmpty(usuario.FotoUrl))
                claims.Add(new Claim("FotoUrl", usuario.FotoUrl));

            var identity = new ClaimsIdentity(claims, "CookieAuth");
            var principal = new ClaimsPrincipal(identity);

            return (true, null, principal);
        }

        private async Task RegistrarBitacora(
            int? usuarioId, string nombreUsuario, bool exitoso,
            string ip, string userAgent, string detalle)
        {
            await _usuarioRepo.RegistrarBitacoraAsync(new BitacoraAcceso
            {
                UsuarioId = usuarioId,
                NombreUsuario = nombreUsuario,
                FechaHora = DateTime.UtcNow,
                Exitoso = exitoso,
                DireccionIP = ip,
                UserAgent = userAgent,
                Detalle = detalle
            });
        }
    }
}