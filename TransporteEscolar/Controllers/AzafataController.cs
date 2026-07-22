using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransporteEscolar.Models;
using TransporteEscolar.Repositories.Interfaces;
using TransporteEscolar.Services;
using TransporteEscolar.ViewModels;

namespace TransporteEscolar.Controllers
{
    [Authorize(Roles = "Azafata")]
    public class AzafataController : Controller
    {
        private readonly IUsuarioRepository _repo;

        
        private int GetUsuarioId() => int.Parse(
            User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

        private int GetAutobusId() => int.Parse(
            User.FindFirst("AutobusId")!.Value);

        private async Task<bool> AsistenciaIndividualExisteAsync(
            int alumnoId, DateOnly fecha, string tipoRuta)
            => await _repo.ExisteAsistenciaIndividualAsync(alumnoId, fecha, tipoRuta);

        // ── Dashboard ────────────────────────────────────────
        public async Task<IActionResult> Dashboard()
        {
            var usuarioId = GetUsuarioId();
            var autobusId = GetAutobusId();
            var jornada = await _repo.ObtenerJornadaActivaAsync(usuarioId);
            var alumnos = await _repo.ObtenerAlumnosPorAutobusAsync(autobusId);
            var hoy = DateOnly.FromDateTime(DateTime.Today);

            ViewBag.NombreCompleto = User.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value;
            ViewBag.TotalAlumnos = alumnos.Count;
            ViewBag.AsistenciaIda = await _repo.ExisteAsistenciaAsync(autobusId, hoy, "Ida");
            ViewBag.AsistenciaVuelta = await _repo.ExisteAsistenciaAsync(autobusId, hoy, "Vuelta");
            ViewBag.Jornada = jornada;
            return View();
        }

        // ── Seleccionar Chofer — GET ──────────────────────────
        [HttpGet]
        public async Task<IActionResult> SeleccionarChofer()
        {
            var choferes = await _repo.ObtenerChoferesAsync();
            ViewBag.Choferes = choferes.Where(c => c.Activo).ToList();
            return View();
        }

        // ── Seleccionar Chofer — POST ─────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SeleccionarChofer(int choferId)
        {
            var usuarioId = GetUsuarioId();
            var autobusId = GetAutobusId();

            await _repo.CerrarJornadasAnterioresAsync(usuarioId);
            await _repo.CrearJornadaAsync(new JornadaChofer
            {
                UsuarioId = usuarioId,
                ChoferId = choferId,
                AutobusId = autobusId,
                FechaHora = DateTime.UtcNow,
                Activa = true
            });
            // Notificar a padres del bus
            var autobus = await _repo.ObtenerAutobusPorIdAsync(autobusId);
            if (autobus != null)
                await _notifService.NotificarJornadaAsync(
                    autobusId, autobus.Ficha,
                    $"La azafata ha iniciado la jornada en el autobús {autobus.Ficha}. ¡En camino!");

            return RedirectToAction(nameof(Dashboard));
        }

        // ── Lista de Alumnos ──────────────────────────────────
        public async Task<IActionResult> Alumnos()
        {
            var alumnos = await _repo.ObtenerAlumnosPorAutobusAsync(GetAutobusId());
            return View(alumnos);
        }

        // ── Registrar Asistencia — GET ────────────────────────
        [HttpGet]
        public async Task<IActionResult> Asistencia(string tipoRuta = "Ida")
        {
            var autobusId = GetAutobusId();
            var hoy = DateOnly.FromDateTime(DateTime.Today);

            if (await _repo.ExisteAsistenciaAsync(autobusId, hoy, tipoRuta))
            {
                var registrada = await _repo.ObtenerAsistenciaPorFechaAsync(autobusId, hoy, tipoRuta);
                ViewBag.YaRegistrada = true;
                ViewBag.TipoRuta = tipoRuta;
                return View(registrada.Select(a => new AsistenciaItemViewModel
                {
                    AlumnoId = a.AlumnoId,
                    NombreAlumno = a.Alumno.NombreCompleto,
                    GradoEscolar = a.Alumno.GradoEscolar ?? "—",
                    DireccionRecogida = a.Alumno.DireccionRecogida, // ← nuevo
                    DireccionEntrega = a.Alumno.DireccionEntrega,  // ← nuevo
                    Presente = a.Presente,
                    Observacion = a.Observacion
                }).ToList());
            }

            var alumnos = await _repo.ObtenerAlumnosPorAutobusAsync(autobusId);
            ViewBag.YaRegistrada = false;
            ViewBag.TipoRuta = tipoRuta;

            return View(alumnos.Select(a => new AsistenciaItemViewModel
            {
                AlumnoId = a.AlumnoId,
                NombreAlumno = a.NombreCompleto,
                GradoEscolar = a.GradoEscolar ?? "—",
                DireccionRecogida = a.DireccionRecogida, // ← nuevo
                DireccionEntrega = a.DireccionEntrega,  // ← nuevo
                Presente = true
            }).ToList());
        }

        // ── Registrar Asistencia — POST ───────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Asistencia(
            List<AsistenciaItemViewModel> items, string tipoRuta)
        {
            var usuarioId = GetUsuarioId();
            var autobusId = GetAutobusId();
            var hoy = DateOnly.FromDateTime(DateTime.Today);

            if (await _repo.ExisteAsistenciaAsync(autobusId, hoy, tipoRuta))
            {
                TempData["Error"] = $"La asistencia de {tipoRuta} ya fue registrada hoy.";
                return RedirectToAction(nameof(Dashboard));
            }

            var registros = items.Select(i => new Asistencia
            {
                AlumnoId = i.AlumnoId,
                AutobusId = autobusId,
                UsuarioId = usuarioId,
                Fecha = hoy,
                TipoRuta = tipoRuta,
                Presente = i.Presente,
                Observacion = i.Observacion,
                FechaHora = DateTime.UtcNow
            }).ToList();

            await _repo.GuardarAsistenciaAsync(registros);
            

            // Notificar ausencias
            foreach (var reg in registros.Where(r => !r.Presente))
            {
                var alumno = await _repo.ObtenerAlumnoPorIdAsync(reg.AlumnoId);
                if (alumno != null)
                    await _notifService.NotificarAusenciaAsync(
                        reg.AlumnoId, alumno.NombreCompleto, tipoRuta);
            }
            TempData["Exito"] = $"Asistencia de {tipoRuta} registrada correctamente.";
            return RedirectToAction(nameof(Dashboard));
        }

        // ── Incidencias — GET ─────────────────────────────────
        public async Task<IActionResult> Incidencias()
        {
            var incidencias = await _repo.ObtenerIncidenciasPorAutobusAsync(GetAutobusId());
            return View(incidencias);
        }

        // ── Nueva Incidencia — GET ────────────────────────────
        [HttpGet]
        public async Task<IActionResult> NuevaIncidencia()
        {
            var alumnos = await _repo.ObtenerAlumnosPorAutobusAsync(GetAutobusId());
            ViewBag.Alumnos = alumnos;
            return View(new IncidenciaViewModel());
        }

        // ── Nueva Incidencia — POST ───────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NuevaIncidencia(IncidenciaViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Alumnos = await _repo.ObtenerAlumnosPorAutobusAsync(GetAutobusId());
                return View(modelo);
            }

            await _repo.CrearIncidenciaAsync(new Incidencia
            {
                AutobusId = GetAutobusId(),
                UsuarioId = GetUsuarioId(),
                AlumnoId = modelo.AlumnoId == 0 ? null : modelo.AlumnoId,
                Fecha = DateOnly.FromDateTime(DateTime.Today),
                Titulo = modelo.Titulo.Trim(),
                Descripcion = modelo.Descripcion.Trim(),
                Tipo = modelo.Tipo,
                FechaHora = DateTime.UtcNow
            });
            // Notificar si tiene alumno relacionado
            if (modelo.AlumnoId.HasValue && modelo.AlumnoId > 0)
            {
                var alumno = await _repo.ObtenerAlumnoPorIdAsync(modelo.AlumnoId.Value);
                if (alumno != null)
                    await _notifService.NotificarIncidenciaAsync(
                        alumno.AlumnoId, alumno.NombreCompleto,
                        modelo.Titulo, modelo.Tipo);
            }
            TempData["Exito"] = "Incidencia registrada correctamente.";
            return RedirectToAction(nameof(Incidencias));


        }
        private readonly NotificacionService _notifService;

        public AzafataController(IUsuarioRepository repo, NotificacionService notifService)
        {
            _repo = repo;
            _notifService = notifService;
        }

        // ── Escáner QR — página ───────────────────────────────────
        [HttpGet]
        public IActionResult EscanearQR()
            => View();

        // ── Buscar alumno por QR (AJAX) ───────────────────────────
        [HttpGet]
        public async Task<IActionResult> BuscarPorQR(string codigo)
        {
            var alumno = await _repo.ObtenerAlumnoPorCodigoQRAsync(codigo);
            if (alumno is null)
                return Json(new { encontrado = false });

            var autobusId = GetAutobusId();
            if (alumno.AutobusId != autobusId)
                return Json(new { encontrado = false, error = "Este alumno no pertenece a tu autobús." });

            var hoy = DateOnly.FromDateTime(DateTime.Today);
            var asistIda = await _repo.ExisteAsistenciaAsync(autobusId, hoy, "Ida");
            var asistVuelta = await _repo.ExisteAsistenciaAsync(autobusId, hoy, "Vuelta");

            return Json(new
            {
                encontrado = true,
                alumnoId = alumno.AlumnoId,
                nombre = alumno.NombreCompleto,
                grado = alumno.GradoEscolar ?? "—",
                tutor = alumno.NombreTutor ?? "—",
                fotoUrl = alumno.FotoUrl ?? "",
                asistIda,
                asistVuelta
            });
        }

        // ── Confirmar asistencia individual por QR ────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarQR(int alumnoId, string tipoRuta)
        {
            var autobusId = GetAutobusId();
            var usuarioId = GetUsuarioId();
            var hoy = DateOnly.FromDateTime(DateTime.Today);

            var yaExiste = await _repo.ExisteAsistenciaIndividualAsync(alumnoId, hoy, tipoRuta);
            if (yaExiste)
                return Json(new
                {
                    ok = false,
                    mensaje = "Asistencia ya registrada para este alumno hoy."
                });

            await _repo.GuardarAsistenciaAsync(new List<Models.Asistencia>
    {
        new()
        {
            AlumnoId  = alumnoId,
            AutobusId = autobusId,
            UsuarioId = usuarioId,
            Fecha     = hoy,
            TipoRuta  = tipoRuta,
            Presente  = true,
            FechaHora = DateTime.UtcNow
        }
    });

            var alumno = await _repo.ObtenerAlumnoPorIdAsync(alumnoId);
            if (alumno != null)
                await _notifService.NotificarAusenciaAsync(
                    alumnoId, alumno.NombreCompleto, tipoRuta);

            return Json(new
            {
                ok = true,
                mensaje = $"✅ Asistencia de {tipoRuta} confirmada."
            });
        }
    }
}