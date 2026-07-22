using Microsoft.EntityFrameworkCore;
using TransporteEscolar.Data;
using TransporteEscolar.Models;
using TransporteEscolar.Repositories.Interfaces;
using TransporteEscolar.ViewModels;   

namespace TransporteEscolar.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly AppDbContext _context;

        public UsuarioRepository(AppDbContext context)
            => _context = context;

        public async Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario)
            => await _context.Usuarios
                .Include(u => u.Rol)
                .Include(u => u.Autobus)
                .FirstOrDefaultAsync(u =>
                    u.NombreUsuario == nombreUsuario && u.Activo);

        public async Task ActualizarUltimoAccesoAsync(int usuarioId)
        {
            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario is null) return;
            usuario.UltimoAcceso = DateTime.UtcNow;
            usuario.IntentosFallidos = 0;
            await _context.SaveChangesAsync();
        }

        public async Task IncrementarIntentosFallidosAsync(int usuarioId)
        {
            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario is null) return;
            usuario.IntentosFallidos++;
            if (usuario.IntentosFallidos >= 5)
                usuario.Bloqueado = true;
            await _context.SaveChangesAsync();
        }

        public async Task ResetearIntentosFallidosAsync(int usuarioId)
        {
            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario is null) return;
            usuario.IntentosFallidos = 0;
            await _context.SaveChangesAsync();
        }

        public async Task RegistrarBitacoraAsync(BitacoraAcceso bitacora)
        {
            _context.BitacoraAccesos.Add(bitacora);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Usuario>> ObtenerTodosAsync()
        => await _context.Usuarios
            .Include(u => u.Rol)
            .Include(u => u.Autobus)
            .OrderBy(u => u.NombreCompleto)
            .ToListAsync();

        public async Task<Usuario?> ObtenerPorIdAsync(int id)
            => await _context.Usuarios
                .Include(u => u.Rol)
                .Include(u => u.Autobus)
                .FirstOrDefaultAsync(u => u.UsuarioId == id);

        public async Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario, int excluirId = 0)
            => await _context.Usuarios
                .AnyAsync(u => u.NombreUsuario == nombreUsuario && u.UsuarioId != excluirId);

        public async Task<Usuario> CrearAsync(Usuario usuario)
        {
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            return usuario;
        }

        public async Task ActualizarAsync(Usuario usuario)
        {
            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario is null) return;
            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Rol>> ObtenerRolesAsync()
            => await _context.Roles.Where(r => r.Activo).ToListAsync();

        public async Task<List<Autobus>> ObtenerAutobusesActivosAsync()
            => await _context.Autobuses.Where(a => a.Activo).ToListAsync();

        // ── CRUD Autobuses ───────────────────────────────────────────

        public async Task<List<Autobus>> ObtenerTodosAutobusesAsync()
            => await _context.Autobuses
                .Include(a => a.Usuarios)
                .OrderBy(a => a.Ficha)
                .ToListAsync();

        public async Task<Autobus?> ObtenerAutobusPorIdAsync(int id)
            => await _context.Autobuses
                .Include(a => a.Usuarios)
                .FirstOrDefaultAsync(a => a.AutobusId == id);

        public async Task<bool> ExisteFichaAsync(string ficha, int excluirId = 0)
            => await _context.Autobuses
                .AnyAsync(a => a.Ficha == ficha && a.AutobusId != excluirId);

        public async Task<bool> ExistePlacaAsync(string placa, int excluirId = 0)
            => await _context.Autobuses
                .AnyAsync(a => a.Placa == placa && a.AutobusId != excluirId);

        public async Task<Autobus> CrearAutobusAsync(Autobus autobus)
        {
            _context.Autobuses.Add(autobus);
            await _context.SaveChangesAsync();
            return autobus;
        }

        public async Task ActualizarAutobusAsync(Autobus autobus)
        {
            _context.Autobuses.Update(autobus);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAutobusAsync(int id)
        {
            var autobus = await _context.Autobuses.FindAsync(id);
            if (autobus is null) return;
            _context.Autobuses.Remove(autobus);
            await _context.SaveChangesAsync();
        }
        // ── Bitácora / Auditoría ─────────────────────────────────────

        public async Task<List<BitacoraAcceso>> ObtenerBitacoraAsync(int pagina, int porPagina)
            => await _context.BitacoraAccesos
                .Include(b => b.Usuario)
                    .ThenInclude(u => u != null ? u.Rol : null)
                .OrderByDescending(b => b.FechaHora)
                .Skip((pagina - 1) * porPagina)
                .Take(porPagina)
                .ToListAsync();

        public async Task<int> ContarBitacoraAsync()
            => await _context.BitacoraAccesos.CountAsync();

        public async Task<List<BitacoraAcceso>> FiltrarBitacoraAsync(
            string? usuario, bool? exitoso, DateTime? desde, DateTime? hasta)
        {
            var query = _context.BitacoraAccesos
                .Include(b => b.Usuario)
                    .ThenInclude(u => u != null ? u.Rol : null)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(usuario))
                query = query.Where(b => b.NombreUsuario.Contains(usuario));

            if (exitoso.HasValue)
                query = query.Where(b => b.Exitoso == exitoso.Value);

            if (desde.HasValue)
                query = query.Where(b => b.FechaHora >= desde.Value);

            if (hasta.HasValue)
                query = query.Where(b => b.FechaHora <= hasta.Value.AddDays(1));

            return await query
                .OrderByDescending(b => b.FechaHora)
                .Take(500)
                .ToListAsync();
        }
        public async Task LimpiarBitacoraAsync(int diasAntiguedad)
        {
            var fechaLimite = DateTime.UtcNow.AddDays(-diasAntiguedad);
            var antiguos = _context.BitacoraAccesos
                .Where(b => b.FechaHora < fechaLimite);
            _context.BitacoraAccesos.RemoveRange(antiguos);
            await _context.SaveChangesAsync();
        }
        public async Task<ReporteDatos> ObtenerDatosReporteAsync(DateTime desde, DateTime hasta)
        {
            var hastaFin = hasta.AddDays(1);

            var usuarios = await _context.Usuarios
                .Include(u => u.Rol)
                .Include(u => u.Autobus)
                .ToListAsync();

            var autobuses = await _context.Autobuses.ToListAsync();

            var bitacora = await _context.BitacoraAccesos
                .Include(b => b.Usuario)
                    .ThenInclude(u => u != null ? u.Rol : null)
                .Where(b => b.FechaHora >= desde && b.FechaHora < hastaFin)
                .OrderByDescending(b => b.FechaHora)
                .ToListAsync();

            return new ReporteDatos
            {
                TotalUsuarios = usuarios.Count,
                UsuariosActivos = usuarios.Count(u => u.Activo && !u.Bloqueado),
                UsuariosBloqueados = usuarios.Count(u => u.Bloqueado),
                TotalAutobuses = autobuses.Count,
                AutobusesActivos = autobuses.Count(a => a.Activo),
                TotalAccesos = bitacora.Count,
                AccesosExitosos = bitacora.Count(b => b.Exitoso),
                AccesosFallidos = bitacora.Count(b => !b.Exitoso),
                Bitacora = bitacora,
                Usuarios = usuarios,
                Autobuses = autobuses
            };

        }
        // ── Choferes ─────────────────────────────────────────────────

        public async Task<List<Chofer>> ObtenerChoferesAsync()
            => await _context.Choferes
                .OrderBy(c => c.NombreCompleto)
                .ToListAsync();

        public async Task<Chofer?> ObtenerChoferPorIdAsync(int id)
            => await _context.Choferes
                .Include(c => c.Jornadas)
                .FirstOrDefaultAsync(c => c.ChoferId == id);

        public async Task<bool> ExisteDUIAsync(string dui, int excluirId = 0)
            => await _context.Choferes
                .AnyAsync(c => c.DUI == dui && c.ChoferId != excluirId);

        public async Task<Chofer> CrearChoferAsync(Chofer chofer)
        {
            _context.Choferes.Add(chofer);
            await _context.SaveChangesAsync();
            return chofer;
        }

        public async Task ActualizarChoferAsync(Chofer chofer)
        {
            _context.Choferes.Update(chofer);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarChoferAsync(int id)
        {
            var chofer = await _context.Choferes.FindAsync(id);
            if (chofer is null) return;
            _context.Choferes.Remove(chofer);
            await _context.SaveChangesAsync();
        }

        // ── Jornadas ─────────────────────────────────────────────────

        public async Task<JornadaChofer?> ObtenerJornadaActivaAsync(int usuarioId)
            => await _context.JornadaChofer
                .Include(j => j.Chofer)
                .Include(j => j.Autobus)
                .Where(j => j.UsuarioId == usuarioId && j.Activa)
                .OrderByDescending(j => j.FechaHora)
                .FirstOrDefaultAsync();

        public async Task<JornadaChofer> CrearJornadaAsync(JornadaChofer jornada)
        {
            _context.JornadaChofer.Add(jornada);
            await _context.SaveChangesAsync();
            return jornada;
        }

        public async Task CerrarJornadasAnterioresAsync(int usuarioId)
        {
            var jornadas = await _context.JornadaChofer
                .Where(j => j.UsuarioId == usuarioId && j.Activa)
                .ToListAsync();
            foreach (var j in jornadas)
                j.Activa = false;
            await _context.SaveChangesAsync();
        }
        // ── Alumnos ──────────────────────────────────────────────────

        public async Task<List<Alumno>> ObtenerAlumnosPorAutobusAsync(int autobusId)
            => await _context.Alumnos
                .Where(a => a.AutobusId == autobusId && a.Activo)
                .OrderBy(a => a.NombreCompleto)
                .ToListAsync();

        public async Task<Alumno?> ObtenerAlumnoPorIdAsync(int id)
            => await _context.Alumnos
                .Include(a => a.Autobus)
                .FirstOrDefaultAsync(a => a.AlumnoId == id);

        public async Task<Alumno?> ObtenerAlumnoPorCodigoQRAsync(string codigoQR)
            => await _context.Alumnos
                .Include(a => a.Autobus)
                .FirstOrDefaultAsync(a => a.CodigoQR == codigoQR);

        public async Task<bool> ExisteAsistenciaIndividualAsync(
    int alumnoId, DateOnly fecha, string tipoRuta)
    => await _context.Asistencias
        .AnyAsync(a => a.AlumnoId == alumnoId
                    && a.Fecha == fecha
                    && a.TipoRuta == tipoRuta);

        // ── Asistencia ───────────────────────────────────────────────

        public async Task<List<Asistencia>> ObtenerAsistenciaPorFechaAsync(
            int autobusId, DateOnly fecha, string tipoRuta)
            => await _context.Asistencias
                .Include(a => a.Alumno)
                .Where(a => a.AutobusId == autobusId
                         && a.Fecha == fecha
                         && a.TipoRuta == tipoRuta)
                .ToListAsync();

        public async Task GuardarAsistenciaAsync(List<Asistencia> registros)
        {
            _context.Asistencias.AddRange(registros);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExisteAsistenciaAsync(
            int autobusId, DateOnly fecha, string tipoRuta)
            => await _context.Asistencias
                .AnyAsync(a => a.AutobusId == autobusId
                            && a.Fecha == fecha
                            && a.TipoRuta == tipoRuta);

        // ── Incidencias ──────────────────────────────────────────────

        public async Task<List<Incidencia>> ObtenerIncidenciasPorAutobusAsync(int autobusId)
            => await _context.Incidencias
                .Include(i => i.Alumno)
                .Where(i => i.AutobusId == autobusId)
                .OrderByDescending(i => i.FechaHora)
                .Take(50)
                .ToListAsync();

        public async Task<Incidencia> CrearIncidenciaAsync(Incidencia incidencia)
        {
            _context.Incidencias.Add(incidencia);
            await _context.SaveChangesAsync();
            return incidencia;
        }
        // ── Padre ────────────────────────────────────────────────────

        public async Task<List<Alumno>> ObtenerHijosDePadreAsync(int usuarioId)
            => await _context.PadreAlumno
                .Where(p => p.UsuarioId == usuarioId)
                .Include(p => p.Alumno).ThenInclude(a => a.Autobus)
                .Select(p => p.Alumno)
                .OrderBy(a => a.NombreCompleto)
                .ToListAsync();

        public async Task<List<Asistencia>> ObtenerAsistenciaAlumnoAsync(int alumnoId, int dias = 14)
        {
            var desde = DateOnly.FromDateTime(DateTime.Today.AddDays(-dias));
            return await _context.Asistencias
                .Where(a => a.AlumnoId == alumnoId && a.Fecha >= desde)
                .OrderByDescending(a => a.Fecha)
                .ThenBy(a => a.TipoRuta)
                .ToListAsync();
        }

        public async Task<List<Incidencia>> ObtenerIncidenciasAlumnoAsync(int alumnoId)
            => await _context.Incidencias
                .Where(i => i.AlumnoId == alumnoId)
                .OrderByDescending(i => i.FechaHora)
                .Take(20)
                .ToListAsync();

        public async Task<JornadaChofer?> ObtenerJornadaActivaPorAutobusAsync(int autobusId)
            => await _context.JornadaChofer
                .Include(j => j.Chofer)
                .Include(j => j.Usuario)
                .Where(j => j.AutobusId == autobusId && j.Activa)
                .OrderByDescending(j => j.FechaHora)
                .FirstOrDefaultAsync();

        // ── Admin: vínculos padre-alumno ─────────────────────────────

        public async Task<List<PadreAlumno>> ObtenerVinculosPadreAsync(int usuarioId)
            => await _context.PadreAlumno
                .Include(p => p.Alumno).ThenInclude(a => a.Autobus)
                .Where(p => p.UsuarioId == usuarioId)
                .ToListAsync();

        public async Task AsignarAlumnoPadreAsync(int usuarioId, int alumnoId)
        {
            var existe = await _context.PadreAlumno
                .AnyAsync(p => p.UsuarioId == usuarioId && p.AlumnoId == alumnoId);
            if (existe) return;

            _context.PadreAlumno.Add(new PadreAlumno
            {
                UsuarioId = usuarioId,
                AlumnoId = alumnoId
            });
            await _context.SaveChangesAsync();
        }

        public async Task EliminarVinculoPadreAsync(int padreAlumnoId)
        {
            var vinculo = await _context.PadreAlumno.FindAsync(padreAlumnoId);
            if (vinculo is null) return;
            _context.PadreAlumno.Remove(vinculo);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Alumno>> ObtenerTodosAlumnosAsync()
    => await _context.Alumnos
        .Include(a => a.Autobus)
        .OrderBy(a => a.NombreCompleto)
        .ToListAsync();

        public async Task<Alumno> CrearAlumnoAsync(Alumno alumno)
        {
            _context.Alumnos.Add(alumno);
            await _context.SaveChangesAsync();
            return alumno;
        }

        public async Task ActualizarAlumnoAsync(Alumno alumno)
        {
            _context.Alumnos.Update(alumno);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAlumnoAsync(int id)
        {
            var alumno = await _context.Alumnos.FindAsync(id);
            if (alumno is null) return;
            _context.Alumnos.Remove(alumno);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExisteAlumnoEnJornadaAsync(int alumnoId)
            => await _context.Asistencias
                .AnyAsync(a => a.AlumnoId == alumnoId);

        public async Task<List<Notificacion>> ObtenerNotificacionesAsync(int usuarioId)
    => await _context.Notificaciones
        .Include(n => n.Alumno)
        .Where(n => n.UsuarioId == usuarioId)
        .OrderByDescending(n => n.FechaHora)
        .Take(50)
        .ToListAsync();

        public async Task<int> ContarNoLeidasAsync(int usuarioId)
            => await _context.Notificaciones
                .CountAsync(n => n.UsuarioId == usuarioId && !n.Leida);

        public async Task CrearNotificacionAsync(Notificacion notificacion)
        {
            _context.Notificaciones.Add(notificacion);
            await _context.SaveChangesAsync();
        }

        public async Task MarcarLeidaAsync(int notificacionId, int usuarioId)
        {
            var n = await _context.Notificaciones
                .FirstOrDefaultAsync(n => n.NotificacionId == notificacionId
                                       && n.UsuarioId == usuarioId);
            if (n is null) return;
            n.Leida = true;
            await _context.SaveChangesAsync();
        }

        public async Task MarcarTodasLeidasAsync(int usuarioId)
        {
            var pendientes = await _context.Notificaciones
                .Where(n => n.UsuarioId == usuarioId && !n.Leida)
                .ToListAsync();
            foreach (var n in pendientes) n.Leida = true;
            await _context.SaveChangesAsync();
        }

        public async Task<List<PadreAlumno>> ObtenerPadresPorAlumnoAsync(int alumnoId)
            => await _context.PadreAlumno
                .Where(p => p.AlumnoId == alumnoId)
                .ToListAsync();

        public async Task<List<PadreAlumno>> ObtenerPadresPorAutobusAsync(int autobusId)
            => await _context.PadreAlumno
                .Include(p => p.Alumno)
                .Where(p => p.Alumno.AutobusId == autobusId)
                .ToListAsync();
        public async Task<DashboardViewModel> ObtenerDatosDashboardAsync()
        {
            var hoy = DateOnly.FromDateTime(DateTime.Today);
            var hace14 = DateOnly.FromDateTime(DateTime.Today.AddDays(-13));
            var modelo = new DashboardViewModel();

            // ── Tarjetas ──────────────────────────────────────────────
            var usuarios = await _context.Usuarios.Include(u => u.Rol).ToListAsync();
            var alumnos = await _context.Alumnos.Include(a => a.Autobus).ToListAsync();
            var autobuses = await _context.Autobuses.ToListAsync();
            var choferes = await _context.Choferes.ToListAsync();

            modelo.TotalUsuarios = usuarios.Count;
            modelo.UsuariosActivos = usuarios.Count(u => u.Activo && !u.Bloqueado);
            modelo.TotalAlumnos = alumnos.Count(a => a.Activo);
            modelo.TotalAutobuses = autobuses.Count(b => b.Activo);
            modelo.TotalChoferes = choferes.Count(c => c.Activo);

            var bitacoraHoy = await _context.BitacoraAccesos
                .Where(b => b.FechaHora.Date == DateTime.Today)
                .ToListAsync();
            modelo.AccesosHoy = bitacoraHoy.Count(b => b.Exitoso);

            var asistHoy = await _context.Asistencias
                .Where(a => a.Fecha == hoy).ToListAsync();
            modelo.AusenciasHoy = asistHoy.Count(a => !a.Presente);

            var incHoy = await _context.Incidencias
                .Where(i => i.Fecha == hoy).ToListAsync();
            modelo.IncidenciasHoy = incHoy.Count;

            // ── Accesos por día (últimos 14 días) ─────────────────────
            var bitacora14 = await _context.BitacoraAccesos
                .Where(b => b.FechaHora.Date >= DateTime.Today.AddDays(-13))
                .ToListAsync();

            for (int i = 13; i >= 0; i--)
            {
                var fecha = DateTime.Today.AddDays(-i);
                modelo.AccesosFechas.Add(fecha.ToString("dd/MM"));
                modelo.AccesosExitosos.Add(bitacora14.Count(b =>
                    b.FechaHora.Date == fecha && b.Exitoso));
                modelo.AccesosFallidos.Add(bitacora14.Count(b =>
                    b.FechaHora.Date == fecha && !b.Exitoso));
            }

            // ── Asistencia por día (últimos 14 días) ──────────────────
            var asist14 = await _context.Asistencias
                .Where(a => a.Fecha >= hace14).ToListAsync();

            for (int i = 13; i >= 0; i--)
            {
                var fecha = hoy.AddDays(-i);
                modelo.AsistFechas.Add(fecha.ToString("dd/MM"));
                modelo.AsistPresentes.Add(asist14.Count(a =>
                    a.Fecha == fecha && a.Presente));
                modelo.AsistAusentes.Add(asist14.Count(a =>
                    a.Fecha == fecha && !a.Presente));
            }

            // ── Usuarios por rol ──────────────────────────────────────
            var porRol = usuarios
                .Where(u => u.Rol != null)
                .GroupBy(u => u.Rol!.Nombre)
                .Select(g => new { Rol = g.Key, Count = g.Count() })
                .ToList();

            modelo.RolesNombres = porRol.Select(r => r.Rol).ToList();
            modelo.RolesCantidades = porRol.Select(r => r.Count).ToList();

            // ── Alumnos por autobús ───────────────────────────────────
            var porBus = alumnos
                .Where(a => a.Activo)
                .GroupBy(a => a.Autobus?.Ficha ?? "Sin bus")
                .Select(g => new { Ficha = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .ToList();

            modelo.BusesFichas = porBus.Select(b => b.Ficha).ToList();
            modelo.BusesAlumnos = porBus.Select(b => b.Count).ToList();

            // ── Incidencias por tipo ──────────────────────────────────
            var porTipo = await _context.Incidencias
                .GroupBy(i => i.Tipo)
                .Select(g => new { Tipo = g.Key, Count = g.Count() })
                .ToListAsync();

            modelo.IncTipos = porTipo.Select(t => t.Tipo).ToList();
            modelo.IncCantidades = porTipo.Select(t => t.Count).ToList();

            return modelo;
        }
    }


}
