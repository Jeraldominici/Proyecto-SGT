using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransporteEscolar.Models;
using TransporteEscolar.Repositories.Interfaces;
using TransporteEscolar.ViewModels;
using QRCoder;

namespace TransporteEscolar.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AlumnoController : Controller
    {
        private readonly IUsuarioRepository _repo;
        private readonly IWebHostEnvironment _env;

        public AlumnoController(IUsuarioRepository repo, IWebHostEnvironment env)
        {
            _repo = repo;
            _env = env;
        }

        // ── Listar ───────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var alumnos = await _repo.ObtenerTodosAlumnosAsync();
            return View(alumnos);
        }

        // ── Crear — GET ──────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            return View("Formulario", new AlumnoFormViewModel
            {
                Autobuses = await _repo.ObtenerAutobusesActivosAsync()
            });
        }

        // ── Crear — POST ─────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(AlumnoFormViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                modelo.Autobuses = await _repo.ObtenerAutobusesActivosAsync();
                return View("Formulario", modelo);
            }

            var fotoUrl = await GuardarFotoAsync(modelo.FotoArchivo);

            await _repo.CrearAlumnoAsync(new Alumno
            {
                NombreCompleto = modelo.NombreCompleto.Trim(),
                FechaNacimiento = modelo.FechaNacimiento,
                GradoEscolar = modelo.GradoEscolar?.Trim(),
                NombreTutor = modelo.NombreTutor?.Trim(),
                TelefonoEmergencia = modelo.TelefonoEmergencia?.Trim(),
                FotoUrl = fotoUrl,
                DireccionRecogida = modelo.DireccionRecogida?.Trim(),
                DireccionEntrega = modelo.DireccionEntrega?.Trim(),
                AutobusId = modelo.AutobusId,
                Activo = modelo.Activo,
                CodigoQR = Guid.NewGuid().ToString(), // ← agrega esto
                FechaAlta = DateTime.UtcNow
            });

            await _repo.CrearAlumnoAsync(new Alumno
            {
                NombreCompleto = modelo.NombreCompleto.Trim(),
                FechaNacimiento = modelo.FechaNacimiento,
                GradoEscolar = modelo.GradoEscolar?.Trim(),
                NombreTutor = modelo.NombreTutor?.Trim(),
                TelefonoEmergencia = modelo.TelefonoEmergencia?.Trim(),
                FotoUrl = fotoUrl,
                DireccionRecogida = modelo.DireccionRecogida?.Trim(),
                DireccionEntrega = modelo.DireccionEntrega?.Trim(),
                AutobusId = modelo.AutobusId,
                Activo = modelo.Activo,
                FechaAlta = DateTime.UtcNow
            });

            TempData["Exito"] = $"Alumno '{modelo.NombreCompleto}' creado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ── Editar — GET ─────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var alumno = await _repo.ObtenerAlumnoPorIdAsync(id);
            if (alumno is null) return NotFound();

            return View("Formulario", new AlumnoFormViewModel
            {
                AlumnoId = alumno.AlumnoId,
                NombreCompleto = alumno.NombreCompleto,
                FechaNacimiento = alumno.FechaNacimiento,
                GradoEscolar = alumno.GradoEscolar,
                NombreTutor = alumno.NombreTutor,
                TelefonoEmergencia = alumno.TelefonoEmergencia,
                FotoUrlActual = alumno.FotoUrl,
                DireccionRecogida = alumno.DireccionRecogida,
                DireccionEntrega = alumno.DireccionEntrega,
                AutobusId = alumno.AutobusId,
                Activo = alumno.Activo,
                Autobuses = await _repo.ObtenerAutobusesActivosAsync()
            });
        }

        // ── Editar — POST ────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(AlumnoFormViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                modelo.Autobuses = await _repo.ObtenerAutobusesActivosAsync();
                return View("Formulario", modelo);
            }

            var alumno = await _repo.ObtenerAlumnoPorIdAsync(modelo.AlumnoId);
            if (alumno is null) return NotFound();

            if (modelo.FotoArchivo is not null)
            {
                BorrarFoto(alumno.FotoUrl);
                alumno.FotoUrl = await GuardarFotoAsync(modelo.FotoArchivo);
            }

            alumno.NombreCompleto = modelo.NombreCompleto.Trim();
            alumno.FechaNacimiento = modelo.FechaNacimiento;
            alumno.GradoEscolar = modelo.GradoEscolar?.Trim();
            alumno.NombreTutor = modelo.NombreTutor?.Trim();
            alumno.TelefonoEmergencia = modelo.TelefonoEmergencia?.Trim();
            alumno.DireccionRecogida = modelo.DireccionRecogida?.Trim();
            alumno.DireccionEntrega = modelo.DireccionEntrega?.Trim();
            alumno.AutobusId = modelo.AutobusId;
            alumno.Activo = modelo.Activo;

            await _repo.ActualizarAlumnoAsync(alumno);
            TempData["Exito"] = $"Alumno '{alumno.NombreCompleto}' actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ── Toggle Activo ─────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActivo(int id)
        {
            var alumno = await _repo.ObtenerAlumnoPorIdAsync(id);
            if (alumno is null) return NotFound();

            alumno.Activo = !alumno.Activo;
            await _repo.ActualizarAlumnoAsync(alumno);

            TempData["Exito"] = alumno.Activo
                ? $"Alumno '{alumno.NombreCompleto}' activado."
                : $"Alumno '{alumno.NombreCompleto}' desactivado.";

            return RedirectToAction(nameof(Index));
        }

        // ── Eliminar — POST ──────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var alumno = await _repo.ObtenerAlumnoPorIdAsync(id);
            if (alumno is null) return NotFound();

            if (await _repo.ExisteAlumnoEnJornadaAsync(id))
            {
                TempData["Error"] = $"No se puede eliminar '{alumno.NombreCompleto}' porque tiene registros de asistencia.";
                return RedirectToAction(nameof(Index));
            }

            BorrarFoto(alumno.FotoUrl);
            await _repo.EliminarAlumnoAsync(id);
            TempData["Exito"] = $"Alumno '{alumno.NombreCompleto}' eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ── Helpers foto ──────────────────────────────────────
        private async Task<string?> GuardarFotoAsync(IFormFile? archivo)
        {
            if (archivo is null || archivo.Length == 0) return null;
            var carpeta = Path.Combine(_env.WebRootPath, "images", "alumnos");
            Directory.CreateDirectory(carpeta);
            var nombre = $"{Guid.NewGuid()}{Path.GetExtension(archivo.FileName)}";
            using var stream = new FileStream(Path.Combine(carpeta, nombre), FileMode.Create);
            await archivo.CopyToAsync(stream);
            return $"/images/alumnos/{nombre}";
        }

        private void BorrarFoto(string? fotoUrl)
        {
            if (string.IsNullOrEmpty(fotoUrl)) return;
            var ruta = Path.Combine(_env.WebRootPath, fotoUrl.TrimStart('/'));
            if (System.IO.File.Exists(ruta)) System.IO.File.Delete(ruta);
        }





        // ── Ver QR (devuelve imagen PNG para modal) ───────────────
        [HttpGet]
        public async Task<IActionResult> QR(int id)
        {
            var alumno = await _repo.ObtenerAlumnoPorIdAsync(id);
            if (alumno is null) return NotFound();

            var qrGenerator = new QRCodeGenerator();
            var qrData = qrGenerator.CreateQrCode(alumno.CodigoQR, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrData);
            var qrBytes = qrCode.GetGraphic(10);

            // Devolver JSON con imagen base64 y datos del alumno
            return Json(new
            {
                nombre = alumno.NombreCompleto,
                grado = alumno.GradoEscolar ?? "—",
                codigoQR = alumno.CodigoQR,
                imagenB64 = Convert.ToBase64String(qrBytes)
            });
        }

        // ── Descargar QR como PNG ─────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> DescargarQR(int id)
        {
            var alumno = await _repo.ObtenerAlumnoPorIdAsync(id);
            if (alumno is null) return NotFound();

            var qrGenerator = new QRCodeGenerator();
            var qrData = qrGenerator.CreateQrCode(alumno.CodigoQR, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrData);
            var qrBytes = qrCode.GetGraphic(10);

            return File(qrBytes, "image/png",
                $"QR_{alumno.NombreCompleto.Replace(" ", "_")}.png");
        }
    }
}
