using TransporteEscolar.Models;
using TransporteEscolar.Repositories.Interfaces;

namespace TransporteEscolar.Services
{
    public class NotificacionService
    {
        private readonly IUsuarioRepository _repo;

        public NotificacionService(IUsuarioRepository repo)
            => _repo = repo;

        // ── Alumno marcado ausente ────────────────────────────
        public async Task NotificarAusenciaAsync(int alumnoId, string nombreAlumno,
            string tipoRuta)
        {
            var padres = await _repo.ObtenerPadresPorAlumnoAsync(alumnoId);
            foreach (var p in padres)
            {
                await _repo.CrearNotificacionAsync(new Notificacion
                {
                    UsuarioId = p.UsuarioId,
                    AlumnoId = alumnoId,
                    Tipo = "Ausencia",
                    Titulo = $"Ausencia registrada — {tipoRuta}",
                    Mensaje = $"{nombreAlumno} fue marcado/a como AUSENTE en la ruta de {tipoRuta} del {DateTime.Today:dd/MM/yyyy}.",
                    FechaHora = DateTime.UtcNow
                });
            }
        }

        // ── Incidencia relacionada a un alumno ────────────────
        public async Task NotificarIncidenciaAsync(int alumnoId, string nombreAlumno,
            string titulo, string tipo)
        {
            var padres = await _repo.ObtenerPadresPorAlumnoAsync(alumnoId);
            foreach (var p in padres)
            {
                await _repo.CrearNotificacionAsync(new Notificacion
                {
                    UsuarioId = p.UsuarioId,
                    AlumnoId = alumnoId,
                    Tipo = "Incidencia",
                    Titulo = $"Nueva incidencia — {tipo}",
                    Mensaje = $"Se registró una incidencia para {nombreAlumno}: \"{titulo}\".",
                    FechaHora = DateTime.UtcNow
                });
            }
        }

        // ── Cambio de jornada (bus salió/llegó) ───────────────
        public async Task NotificarJornadaAsync(int autobusId, string fichaAutobus,
            string mensaje)
        {
            var padres = await _repo.ObtenerPadresPorAutobusAsync(autobusId);
            var usuariosNotificados = new HashSet<int>();

            foreach (var p in padres)
            {
                if (usuariosNotificados.Contains(p.UsuarioId)) continue;
                usuariosNotificados.Add(p.UsuarioId);

                await _repo.CrearNotificacionAsync(new Notificacion
                {
                    UsuarioId = p.UsuarioId,
                    Tipo = "Jornada",
                    Titulo = $"Autobús {fichaAutobus}",
                    Mensaje = mensaje,
                    FechaHora = DateTime.UtcNow
                });
            }
        }

        // ── Mensaje manual del Admin ──────────────────────────
        public async Task NotificarManualAsync(List<int> usuarioIds, string titulo,
            string mensaje)
        {
            foreach (var uid in usuarioIds)
            {
                await _repo.CrearNotificacionAsync(new Notificacion
                {
                    UsuarioId = uid,
                    Tipo = "Manual",
                    Titulo = titulo,
                    Mensaje = mensaje,
                    FechaHora = DateTime.UtcNow
                });
            }
        }
    }
}