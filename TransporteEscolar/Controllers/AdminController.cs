using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransporteEscolar.Helpers;
using TransporteEscolar.Models;
using TransporteEscolar.Repositories.Interfaces;
using TransporteEscolar.Services;
using TransporteEscolar.ViewModels;

namespace TransporteEscolar.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly ExportService _exportService;

        public AdminController(IUsuarioRepository usuarioRepo, ExportService exportService)
        {
            _usuarioRepo = usuarioRepo;
            _exportService = exportService;
        }

        // ── Dashboard ────────────────────────────────────────
        public async Task<IActionResult> Dashboard()
        {
            var modelo = await _usuarioRepo.ObtenerDatosDashboardAsync();
            return View(modelo);
        }

        // ── Listar usuarios ──────────────────────────────────
        public async Task<IActionResult> Usuarios()
        {
            var usuarios = await _usuarioRepo.ObtenerTodosAsync();
            return View(usuarios);
        }

        // ── Crear — GET ──────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> CrearUsuario()
        {
            var modelo = new UsuarioFormViewModel
            {
                Roles = await _usuarioRepo.ObtenerRolesAsync(),
                Autobuses = await _usuarioRepo.ObtenerAutobusesActivosAsync()
            };
            return View("FormularioUsuario", modelo);
        }

        // ── Crear — POST ─────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearUsuario(UsuarioFormViewModel modelo)
        {
            if (string.IsNullOrWhiteSpace(modelo.Password))
                ModelState.AddModelError("Password", "La contraseña es obligatoria al crear un usuario.");

            if (!ModelState.IsValid)
            {
                modelo.Roles = await _usuarioRepo.ObtenerRolesAsync();
                modelo.Autobuses = await _usuarioRepo.ObtenerAutobusesActivosAsync();
                return View("FormularioUsuario", modelo);
            }

            if (await _usuarioRepo.ExisteNombreUsuarioAsync(modelo.NombreUsuario))
            {
                ModelState.AddModelError("NombreUsuario", "Ese nombre de usuario ya está en uso.");
                modelo.Roles = await _usuarioRepo.ObtenerRolesAsync();
                modelo.Autobuses = await _usuarioRepo.ObtenerAutobusesActivosAsync();
                return View("FormularioUsuario", modelo);
            }

            var usuario = new Usuario
            {
                NombreUsuario = modelo.NombreUsuario.Trim(),
                PasswordHash = PasswordHelper.HashPassword(modelo.Password!),
                NombreCompleto = modelo.NombreCompleto.Trim(),
                Email = modelo.Email?.Trim(),
                RolId = modelo.RolId,
                AutobusId = modelo.AutobusId,
                Activo = modelo.Activo,
                FechaCreacion = DateTime.UtcNow
            };

            await _usuarioRepo.CrearAsync(usuario);
            TempData["Exito"] = $"Usuario '{usuario.NombreUsuario}' creado correctamente.";
            return RedirectToAction(nameof(Usuarios));
        }

        // ── Editar — GET ─────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> EditarUsuario(int id)
        {
            var usuario = await _usuarioRepo.ObtenerPorIdAsync(id);
            if (usuario is null) return NotFound();

            var modelo = new UsuarioFormViewModel
            {
                UsuarioId = usuario.UsuarioId,
                NombreUsuario = usuario.NombreUsuario,
                NombreCompleto = usuario.NombreCompleto,
                Email = usuario.Email,
                RolId = usuario.RolId,
                AutobusId = usuario.AutobusId,
                Activo = usuario.Activo,
                Roles = await _usuarioRepo.ObtenerRolesAsync(),
                Autobuses = await _usuarioRepo.ObtenerAutobusesActivosAsync()
            };

            return View("FormularioUsuario", modelo);
        }

        // ── Editar — POST ────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarUsuario(UsuarioFormViewModel modelo)
        {
            if (!string.IsNullOrWhiteSpace(modelo.Password) &&
                modelo.Password != modelo.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Las contraseñas no coinciden.");
            }

            ModelState.Remove("Password");
            ModelState.Remove("ConfirmPassword");

            if (!ModelState.IsValid)
            {
                modelo.Roles = await _usuarioRepo.ObtenerRolesAsync();
                modelo.Autobuses = await _usuarioRepo.ObtenerAutobusesActivosAsync();
                return View("FormularioUsuario", modelo);
            }

            var usuario = await _usuarioRepo.ObtenerPorIdAsync(modelo.UsuarioId);
            if (usuario is null) return NotFound();

            if (await _usuarioRepo.ExisteNombreUsuarioAsync(modelo.NombreUsuario, modelo.UsuarioId))
            {
                ModelState.AddModelError("NombreUsuario", "Ese nombre de usuario ya está en uso.");
                modelo.Roles = await _usuarioRepo.ObtenerRolesAsync();
                modelo.Autobuses = await _usuarioRepo.ObtenerAutobusesActivosAsync();
                return View("FormularioUsuario", modelo);
            }

            usuario.NombreUsuario = modelo.NombreUsuario.Trim();
            usuario.NombreCompleto = modelo.NombreCompleto.Trim();
            usuario.Email = modelo.Email?.Trim();
            usuario.RolId = modelo.RolId;
            usuario.AutobusId = modelo.AutobusId;
            usuario.Activo = modelo.Activo;

            if (!string.IsNullOrWhiteSpace(modelo.Password))
                usuario.PasswordHash = PasswordHelper.HashPassword(modelo.Password);

            await _usuarioRepo.ActualizarAsync(usuario);
            TempData["Exito"] = $"Usuario '{usuario.NombreUsuario}' actualizado correctamente.";
            return RedirectToAction(nameof(Usuarios));
        }

        // ── Eliminar — POST ──────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarUsuario(int id)
        {
            var idActual = int.Parse(
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            if (id == idActual)
            {
                TempData["Error"] = "No puedes eliminar tu propio usuario.";
                return RedirectToAction(nameof(Usuarios));
            }

            var usuario = await _usuarioRepo.ObtenerPorIdAsync(id);
            if (usuario is null) return NotFound();

            await _usuarioRepo.EliminarAsync(id);
            TempData["Exito"] = $"Usuario '{usuario.NombreUsuario}' eliminado correctamente.";
            return RedirectToAction(nameof(Usuarios));
        }

        // ── Toggle Bloqueo — POST ────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleBloqueo(int id)
        {
            var usuario = await _usuarioRepo.ObtenerPorIdAsync(id);
            if (usuario is null) return NotFound();

            usuario.Bloqueado = !usuario.Bloqueado;
            usuario.IntentosFallidos = 0;
            await _usuarioRepo.ActualizarAsync(usuario);

            TempData["Exito"] = usuario.Bloqueado
                ? $"Usuario '{usuario.NombreUsuario}' bloqueado."
                : $"Usuario '{usuario.NombreUsuario}' desbloqueado.";

            return RedirectToAction(nameof(Usuarios));
        }

        // ── Auditoría de Accesos ─────────────────────────────
        public async Task<IActionResult> Auditoria(
            int pagina = 1,
            string? usuario = null,
            bool? exitoso = null,
            DateTime? desde = null,
            DateTime? hasta = null)
        {
            const int porPagina = 20;

            List<BitacoraAcceso> registros;
            int total;

            bool hayFiltros = !string.IsNullOrWhiteSpace(usuario)
                || exitoso.HasValue || desde.HasValue || hasta.HasValue;

            if (hayFiltros)
            {
                registros = await _usuarioRepo.FiltrarBitacoraAsync(usuario, exitoso, desde, hasta);
                total = registros.Count;
            }
            else
            {
                total = await _usuarioRepo.ContarBitacoraAsync();
                registros = await _usuarioRepo.ObtenerBitacoraAsync(pagina, porPagina);
            }

            var modelo = new BitacoraViewModel
            {
                Registros = registros,
                PaginaActual = pagina,
                TotalRegistros = total,
                TotalPaginas = (int)Math.Ceiling(total / (double)porPagina),
                PorPagina = porPagina,
                FiltroUsuario = usuario,
                FiltroExitoso = exitoso,
                FiltroDesde = desde,
                FiltroHasta = hasta
            };

            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LimpiarBitacora(int dias = 90)
        {
            await _usuarioRepo.LimpiarBitacoraAsync(dias);
            TempData["Exito"] = $"Registros anteriores a {dias} días eliminados correctamente.";
            return RedirectToAction(nameof(Auditoria));
        }

        // ── Reportes — GET ───────────────────────────────────
        public async Task<IActionResult> Reportes(DateTime? desde = null, DateTime? hasta = null)
        {
            var fechaDesde = desde ?? DateTime.Today.AddDays(-30);
            var fechaHasta = hasta ?? DateTime.Today;

            var datos = await _usuarioRepo.ObtenerDatosReporteAsync(fechaDesde, fechaHasta);
            var modelo = ConstruirModelo(datos, fechaDesde, fechaHasta);

            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExportarExcel(DateTime desde, DateTime hasta)
        {
            var datos = await _usuarioRepo.ObtenerDatosReporteAsync(desde, hasta);
            var modelo = ConstruirModelo(datos, desde, hasta);
            var bytes = _exportService.ExportarExcel(modelo);

            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Reporte_TransporteEscolar_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExportarPdf(DateTime desde, DateTime hasta)
        {
            var datos = await _usuarioRepo.ObtenerDatosReporteAsync(desde, hasta);
            var modelo = ConstruirModelo(datos, desde, hasta);
            var bytes = _exportService.ExportarPdf(modelo);

            return File(bytes, "application/pdf",
                $"Reporte_TransporteEscolar_{DateTime.Now:yyyyMMdd}.pdf");
        }

        private ReporteViewModel ConstruirModelo(ReporteDatos datos, DateTime desde, DateTime hasta)
        {
            return new ReporteViewModel
            {
                FechaDesde = desde,
                FechaHasta = hasta,
                TotalUsuarios = datos.TotalUsuarios,
                UsuariosActivos = datos.UsuariosActivos,
                UsuariosBloqueados = datos.UsuariosBloqueados,
                TotalAutobuses = datos.TotalAutobuses,
                AutobusesActivos = datos.AutobusesActivos,
                TotalAccesos = datos.TotalAccesos,
                AccesosExitosos = datos.AccesosExitosos,
                AccesosFallidos = datos.AccesosFallidos,
                UsuariosBloqueadosList = datos.Usuarios.Where(u => u.Bloqueado).ToList(),

                AccesosPorUsuario = datos.Bitacora
                    .GroupBy(b => b.NombreUsuario)
                    .Select(g => new AccesosPorUsuario
                    {
                        NombreUsuario = g.Key,
                        NombreCompleto = g.First().Usuario?.NombreCompleto ?? g.Key,
                        Rol = g.First().Usuario?.Rol?.Nombre ?? "—",
                        TotalAccesos = g.Count(),
                        Exitosos = g.Count(b => b.Exitoso),
                        Fallidos = g.Count(b => !b.Exitoso),
                        UltimoAcceso = g.Max(b => b.FechaHora)
                    })
                    .OrderByDescending(a => a.TotalAccesos)
                    .ToList(),

                FallidosPorUsuario = datos.Bitacora
                    .Where(b => !b.Exitoso)
                    .GroupBy(b => b.NombreUsuario)
                    .Select(g => new FallidosPorUsuario
                    {
                        NombreUsuario = g.Key,
                        NombreCompleto = g.First().Usuario?.NombreCompleto ?? g.Key,
                        TotalFallidos = g.Count(),
                        Bloqueado = g.First().Usuario?.Bloqueado ?? false,
                        UltimaIP = g.OrderByDescending(b => b.FechaHora).First().DireccionIP
                    })
                    .OrderByDescending(f => f.TotalFallidos)
                    .ToList(),

                ActividadPorFecha = Enumerable
                    .Range(0, (hasta - desde).Days + 1)
                    .Select(i => desde.AddDays(i))
                    .Select(fecha => new ActividadPorFecha
                    {
                        Fecha = fecha,
                        Exitosos = datos.Bitacora.Count(b => b.FechaHora.Date == fecha.Date && b.Exitoso),
                        Fallidos = datos.Bitacora.Count(b => b.FechaHora.Date == fecha.Date && !b.Exitoso)
                    })
                    .ToList()
            };

        }
        // ── Ver hijos de un padre ─────────────────────────────
        [HttpGet]
        public async Task<IActionResult> HijosPadre(int id)
        {
            var padre = await _usuarioRepo.ObtenerPorIdAsync(id);
            if (padre is null) return NotFound();

            var vinculos = await _usuarioRepo.ObtenerVinculosPadreAsync(id);
            var todosAlumnos = await _usuarioRepo.ObtenerTodosAutobusesAsync(); // reutilizamos
            var alumnos = new List<Models.Alumno>();

            // Carga todos los alumnos disponibles para asignar
            foreach (var bus in todosAlumnos)
            {
                var lista = await _usuarioRepo.ObtenerAlumnosPorAutobusAsync(bus.AutobusId);
                alumnos.AddRange(lista);
            }

            ViewBag.Padre = padre;
            ViewBag.Alumnos = alumnos.Where(a => vinculos.All(v => v.AlumnoId != a.AlumnoId)).ToList();
            return View(vinculos);
        }

        // ── Asignar alumno a padre ────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarAlumno(int padreId, int alumnoId)
        {
            await _usuarioRepo.AsignarAlumnoPadreAsync(padreId, alumnoId);
            TempData["Exito"] = "Alumno asignado correctamente.";
            return RedirectToAction(nameof(HijosPadre), new { id = padreId });
        }

        // ── Eliminar vínculo ──────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarVinculo(int padreAlumnoId, int padreId)
        {
            await _usuarioRepo.EliminarVinculoPadreAsync(padreAlumnoId);
            TempData["Exito"] = "Vínculo eliminado correctamente.";
            return RedirectToAction(nameof(HijosPadre), new { id = padreId });
        }
    }
}