using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TransporteEscolar.Services;

namespace TransporteEscolar.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ImportExportController : Controller
    {
        private readonly ImportExportService _service;

        public ImportExportController(ImportExportService service)
            => _service = service;

        // ── Página principal ─────────────────────────────────
        public IActionResult Index(string tab = "Alumnos")
        {
            ViewBag.TabActiva = tab;
            return View();
        }

        // ── Exportar ─────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Exportar(string entidad)
        {
            var bytes = entidad switch
            {
                "Alumnos" => await _service.ExportarAlumnosAsync(),
                "Autobuses" => await _service.ExportarAutobusesAsync(),
                "Choferes" => await _service.ExportarChoferesAsync(),
                "Azafatas" => await _service.ExportarAzafatasAsync(),
                _ => null
            };

            if (bytes is null) return BadRequest();

            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"{entidad}_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        // ── Descargar plantilla ───────────────────────────────
        [HttpGet]
        public IActionResult Plantilla(string entidad)
        {
            var bytes = _service.GenerarPlantilla(entidad);
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Plantilla_{entidad}.xlsx");
        }

        // ── Preview importación ───────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Preview(IFormFile archivo, string entidad)
        {
            if (archivo is null || archivo.Length == 0)
            {
                TempData["Error"] = "Debes seleccionar un archivo Excel.";
                return RedirectToAction(nameof(Index), new { tab = entidad });
            }

            var preview = _service.GenerarPreview(archivo, entidad);
            return View(preview);
        }

        // ── Confirmar importación ─────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirmar(string datosJson, string entidad)
        {
            try
            {
                var datos = JsonSerializer.Deserialize<ImportDatos>(datosJson)
                    ?? throw new Exception("Datos inválidos.");

                var (creados, _, errores) =
                    await _service.ImportarAsync(entidad, datos.Filas);

                TempData["Exito"] = $"{creados} registro(s) importados correctamente.";

                if (errores.Any())
                    TempData["Error"] = string.Join(" | ", errores.Take(5));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al importar: {ex.Message}";
            }

            return RedirectToAction(nameof(Index), new { tab = entidad });
        }
    }

    public class ImportDatos
    {
        public string Entidad { get; set; } = string.Empty;
        public List<List<string>> Filas { get; set; } = new();
    }
}