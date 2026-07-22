using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransporteEscolar.Models;
using TransporteEscolar.Repositories.Interfaces;
using TransporteEscolar.ViewModels;

namespace TransporteEscolar.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ChoferController : Controller
    {
        private readonly IUsuarioRepository _repo;
        private readonly IWebHostEnvironment _env;

        public ChoferController(IUsuarioRepository repo, IWebHostEnvironment env)
        {
            _repo = repo;
            _env = env;
        }

        // ── Listar ───────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var choferes = await _repo.ObtenerChoferesAsync();
            return View(choferes);
        }

        // ── Crear — GET ──────────────────────────────────────
        [HttpGet]
        public IActionResult Crear()
            => View("Formulario", new ChoferFormViewModel());

        // ── Crear — POST ─────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(ChoferFormViewModel modelo)
        {
            if (!ModelState.IsValid)
                return View("Formulario", modelo);

            if (await _repo.ExisteDUIAsync(modelo.DUI))
            {
                ModelState.AddModelError("DUI", "Ese DUI ya está registrado.");
                return View("Formulario", modelo);
            }

            var fotoUrl = await GuardarFotoAsync(modelo.FotoArchivo);

            await _repo.CrearChoferAsync(new Chofer
            {
                NombreCompleto = modelo.NombreCompleto.Trim(),
                Telefono = modelo.Telefono?.Trim(),
                DUI = modelo.DUI.Trim(),
                Licencia = modelo.Licencia.Trim(),
                FotoUrl = fotoUrl,
                Activo = modelo.Activo,
                FechaAlta = DateTime.UtcNow
            });

            TempData["Exito"] = $"Chofer '{modelo.NombreCompleto}' creado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ── Editar — GET ─────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var chofer = await _repo.ObtenerChoferPorIdAsync(id);
            if (chofer is null) return NotFound();

            return View("Formulario", new ChoferFormViewModel
            {
                ChoferId = chofer.ChoferId,
                NombreCompleto = chofer.NombreCompleto,
                Telefono = chofer.Telefono,
                DUI = chofer.DUI,
                Licencia = chofer.Licencia,
                FotoUrlActual = chofer.FotoUrl,
                Activo = chofer.Activo
            });
        }

        // ── Editar — POST ────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(ChoferFormViewModel modelo)
        {
            if (!ModelState.IsValid)
                return View("Formulario", modelo);

            if (await _repo.ExisteDUIAsync(modelo.DUI, modelo.ChoferId))
            {
                ModelState.AddModelError("DUI", "Ese DUI ya está en uso.");
                return View("Formulario", modelo);
            }

            var chofer = await _repo.ObtenerChoferPorIdAsync(modelo.ChoferId);
            if (chofer is null) return NotFound();

            // Si subió nueva foto, guardarla y borrar la anterior
            if (modelo.FotoArchivo is not null)
            {
                BorrarFoto(chofer.FotoUrl);
                chofer.FotoUrl = await GuardarFotoAsync(modelo.FotoArchivo);
            }

            chofer.NombreCompleto = modelo.NombreCompleto.Trim();
            chofer.Telefono = modelo.Telefono?.Trim();
            chofer.DUI = modelo.DUI.Trim();
            chofer.Licencia = modelo.Licencia.Trim();
            chofer.Activo = modelo.Activo;

            await _repo.ActualizarChoferAsync(chofer);
            TempData["Exito"] = $"Chofer '{chofer.NombreCompleto}' actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ── Eliminar — POST ──────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var chofer = await _repo.ObtenerChoferPorIdAsync(id);
            if (chofer is null) return NotFound();

            if (chofer.Jornadas.Any(j => j.Activa))
            {
                TempData["Error"] = $"No se puede eliminar '{chofer.NombreCompleto}' porque tiene una jornada activa.";
                return RedirectToAction(nameof(Index));
            }

            BorrarFoto(chofer.FotoUrl);
            await _repo.EliminarChoferAsync(id);
            TempData["Exito"] = $"Chofer '{chofer.NombreCompleto}' eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ── Toggle Activo ────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActivo(int id)
        {
            var chofer = await _repo.ObtenerChoferPorIdAsync(id);
            if (chofer is null) return NotFound();

            chofer.Activo = !chofer.Activo;
            await _repo.ActualizarChoferAsync(chofer);

            TempData["Exito"] = chofer.Activo
                ? $"Chofer '{chofer.NombreCompleto}' activado."
                : $"Chofer '{chofer.NombreCompleto}' desactivado.";

            return RedirectToAction(nameof(Index));
        }

        // ── Helpers de foto ──────────────────────────────────
        private async Task<string?> GuardarFotoAsync(IFormFile? archivo)
        {
            if (archivo is null || archivo.Length == 0) return null;

            var carpeta = Path.Combine(_env.WebRootPath, "images", "choferes");
            Directory.CreateDirectory(carpeta);

            var nombreArchivo = $"{Guid.NewGuid()}{Path.GetExtension(archivo.FileName)}";
            var rutaCompleta = Path.Combine(carpeta, nombreArchivo);

            using var stream = new FileStream(rutaCompleta, FileMode.Create);
            await archivo.CopyToAsync(stream);

            return $"/images/choferes/{nombreArchivo}";
        }

        private void BorrarFoto(string? fotoUrl)
        {
            if (string.IsNullOrEmpty(fotoUrl)) return;
            var ruta = Path.Combine(_env.WebRootPath, fotoUrl.TrimStart('/'));
            if (System.IO.File.Exists(ruta))
                System.IO.File.Delete(ruta);
        }
    }
}