using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransporteEscolar.Models;
using TransporteEscolar.Repositories.Interfaces;
using TransporteEscolar.ViewModels;

namespace TransporteEscolar.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AutobusController : Controller
    {
        private readonly IUsuarioRepository _repo;

        public AutobusController(IUsuarioRepository repo)
            => _repo = repo;

        // ── Listar ───────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var autobuses = await _repo.ObtenerTodosAutobusesAsync();
            return View(autobuses);
        }

        // ── Crear — GET ──────────────────────────────────────
        [HttpGet]
        public IActionResult Crear()
            => View("Formulario", new AutobusFormViewModel());

        // ── Crear — POST ─────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(AutobusFormViewModel modelo)
        {
            if (!ModelState.IsValid)
                return View("Formulario", modelo);

            if (await _repo.ExisteFichaAsync(modelo.Ficha))
            {
                ModelState.AddModelError("Ficha", "Esa ficha ya está registrada.");
                return View("Formulario", modelo);
            }

            if (await _repo.ExistePlacaAsync(modelo.Placa))
            {
                ModelState.AddModelError("Placa", "Esa placa ya está registrada.");
                return View("Formulario", modelo);
            }

            await _repo.CrearAutobusAsync(new Autobus
            {
                Ficha = modelo.Ficha.Trim().ToUpper(),
                Placa = modelo.Placa.Trim().ToUpper(),
                Capacidad = modelo.Capacidad,
                Activo = modelo.Activo,
                FechaAlta = DateTime.UtcNow
            });

            TempData["Exito"] = $"Autobús '{modelo.Ficha}' creado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ── Editar — GET ─────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var autobus = await _repo.ObtenerAutobusPorIdAsync(id);
            if (autobus is null) return NotFound();

            return View("Formulario", new AutobusFormViewModel
            {
                AutobusId = autobus.AutobusId,
                Ficha = autobus.Ficha,
                Placa = autobus.Placa,
                Capacidad = autobus.Capacidad,
                Activo = autobus.Activo
            });
        }

        // ── Editar — POST ────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(AutobusFormViewModel modelo)
        {
            if (!ModelState.IsValid)
                return View("Formulario", modelo);

            if (await _repo.ExisteFichaAsync(modelo.Ficha, modelo.AutobusId))
            {
                ModelState.AddModelError("Ficha", "Esa ficha ya está en uso.");
                return View("Formulario", modelo);
            }

            if (await _repo.ExistePlacaAsync(modelo.Placa, modelo.AutobusId))
            {
                ModelState.AddModelError("Placa", "Esa placa ya está en uso.");
                return View("Formulario", modelo);
            }

            var autobus = await _repo.ObtenerAutobusPorIdAsync(modelo.AutobusId);
            if (autobus is null) return NotFound();

            autobus.Ficha = modelo.Ficha.Trim().ToUpper();
            autobus.Placa = modelo.Placa.Trim().ToUpper();
            autobus.Capacidad = modelo.Capacidad;
            autobus.Activo = modelo.Activo;

            await _repo.ActualizarAutobusAsync(autobus);
            TempData["Exito"] = $"Autobús '{autobus.Ficha}' actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ── Eliminar — POST ──────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var autobus = await _repo.ObtenerAutobusPorIdAsync(id);
            if (autobus is null) return NotFound();

            // Proteger: no eliminar si tiene azafatas asignadas
            if (autobus.Usuarios.Any())
            {
                TempData["Error"] = $"No se puede eliminar '{autobus.Ficha}' porque tiene usuarios asignados.";
                return RedirectToAction(nameof(Index));
            }

            await _repo.EliminarAutobusAsync(id);
            TempData["Exito"] = $"Autobús '{autobus.Ficha}' eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ── Toggle Activo — POST ─────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActivo(int id)
        {
            var autobus = await _repo.ObtenerAutobusPorIdAsync(id);
            if (autobus is null) return NotFound();

            autobus.Activo = !autobus.Activo;
            await _repo.ActualizarAutobusAsync(autobus);

            TempData["Exito"] = autobus.Activo
                ? $"Autobús '{autobus.Ficha}' activado."
                : $"Autobús '{autobus.Ficha}' desactivado.";

            return RedirectToAction(nameof(Index));
        }
    }
}