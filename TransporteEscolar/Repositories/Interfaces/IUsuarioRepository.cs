using TransporteEscolar.Models;
using TransporteEscolar.ViewModels;

namespace TransporteEscolar.Repositories.Interfaces
{
    public interface IUsuarioRepository
    {
        // ── Autenticación ────────────────────────────────────
        Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario);
        Task ActualizarUltimoAccesoAsync(int usuarioId);
        Task IncrementarIntentosFallidosAsync(int usuarioId);
        Task ResetearIntentosFallidosAsync(int usuarioId);
        Task RegistrarBitacoraAsync(BitacoraAcceso bitacora);

        // ── CRUD Usuarios ────────────────────────────────────
        Task<List<Usuario>> ObtenerTodosAsync();
        Task<Usuario?> ObtenerPorIdAsync(int id);
        Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario, int excluirId = 0);
        Task<Usuario> CrearAsync(Usuario usuario);
        Task ActualizarAsync(Usuario usuario);
        Task EliminarAsync(int id);

        // ── Auxiliares ───────────────────────────────────────
        Task<List<Rol>> ObtenerRolesAsync();
        Task<List<Autobus>> ObtenerAutobusesActivosAsync();

        // ── CRUD Autobuses ───────────────────────────────────
        Task<List<Autobus>> ObtenerTodosAutobusesAsync();
        Task<Autobus?> ObtenerAutobusPorIdAsync(int id);
        Task<bool> ExisteFichaAsync(string ficha, int excluirId = 0);
        Task<bool> ExistePlacaAsync(string placa, int excluirId = 0);
        Task<Autobus> CrearAutobusAsync(Autobus autobus);
        Task ActualizarAutobusAsync(Autobus autobus);
        Task EliminarAutobusAsync(int id);

        // ── Bitácora / Auditoría ─────────────────────────────
        Task<List<BitacoraAcceso>> ObtenerBitacoraAsync(int pagina, int porPagina);
        Task<int> ContarBitacoraAsync();
        Task<List<BitacoraAcceso>> FiltrarBitacoraAsync(string? usuario, bool? exitoso, DateTime? desde, DateTime? hasta);
        Task LimpiarBitacoraAsync(int diasAntiguedad);

        // ── Reportes ─────────────────────────────────────────
        Task<ReporteDatos> ObtenerDatosReporteAsync(DateTime desde, DateTime hasta);

        // ── Choferes ──────────────────────────────────────────
        Task<List<Chofer>> ObtenerChoferesAsync();
        Task<Chofer?> ObtenerChoferPorIdAsync(int id);
        Task<bool> ExisteDUIAsync(string dui, int excluirId = 0);
        Task<Chofer> CrearChoferAsync(Chofer chofer);
        Task ActualizarChoferAsync(Chofer chofer);
        Task EliminarChoferAsync(int id);

        // ── Jornadas ──────────────────────────────────────────
        Task<JornadaChofer?> ObtenerJornadaActivaAsync(int usuarioId);
        Task<JornadaChofer> CrearJornadaAsync(JornadaChofer jornada);
        Task CerrarJornadasAnterioresAsync(int usuarioId);

        // ── Alumnos ───────────────────────────────────────────
        Task<List<Alumno>> ObtenerAlumnosPorAutobusAsync(int autobusId);
        Task<Alumno?> ObtenerAlumnoPorIdAsync(int id);
        Task<Alumno?> ObtenerAlumnoPorCodigoQRAsync(string codigoQR);
        Task<bool> ExisteAsistenciaIndividualAsync(int alumnoId, DateOnly fecha, string tipoRuta);

        // ── Asistencia ────────────────────────────────────────
        Task<List<Asistencia>> ObtenerAsistenciaPorFechaAsync(int autobusId, DateOnly fecha, string tipoRuta);
        Task GuardarAsistenciaAsync(List<Asistencia> registros);
        Task<bool> ExisteAsistenciaAsync(int autobusId, DateOnly fecha, string tipoRuta);

        // ── Incidencias ───────────────────────────────────────
        Task<List<Incidencia>> ObtenerIncidenciasPorAutobusAsync(int autobusId);
        Task<Incidencia> CrearIncidenciaAsync(Incidencia incidencia);

        // ── Padre ─────────────────────────────────────────────
        Task<List<Alumno>> ObtenerHijosDePadreAsync(int usuarioId);
        Task<List<Asistencia>> ObtenerAsistenciaAlumnoAsync(int alumnoId, int dias = 14);
        Task<List<Incidencia>> ObtenerIncidenciasAlumnoAsync(int alumnoId);
        Task<JornadaChofer?> ObtenerJornadaActivaPorAutobusAsync(int autobusId);

        // ── Admin: gestión de vínculos padre-alumno ───────────
        Task<List<PadreAlumno>> ObtenerVinculosPadreAsync(int usuarioId);
        Task AsignarAlumnoPadreAsync(int usuarioId, int alumnoId);
        Task EliminarVinculoPadreAsync(int padreAlumnoId);

        // ── CRUD Alumnos (Admin) ──────────────────────────────
        Task<List<Alumno>> ObtenerTodosAlumnosAsync();
        Task<Alumno> CrearAlumnoAsync(Alumno alumno);
        Task ActualizarAlumnoAsync(Alumno alumno);
        Task EliminarAlumnoAsync(int id);
        Task<bool> ExisteAlumnoEnJornadaAsync(int alumnoId);

        // ── Notificaciones ────────────────────────────────────
        Task<List<Notificacion>> ObtenerNotificacionesAsync(int usuarioId);
        Task<int> ContarNoLeidasAsync(int usuarioId);
        Task CrearNotificacionAsync(Notificacion notificacion);
        Task MarcarLeidaAsync(int notificacionId, int usuarioId);
        Task MarcarTodasLeidasAsync(int usuarioId);
        Task<List<PadreAlumno>> ObtenerPadresPorAlumnoAsync(int alumnoId);
        Task<List<PadreAlumno>> ObtenerPadresPorAutobusAsync(int autobusId);
        // ── Dashboard ─────────────────────────────────────────
        Task<DashboardViewModel> ObtenerDatosDashboardAsync();


    }
}