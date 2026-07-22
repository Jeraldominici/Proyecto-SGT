namespace TransporteEscolar.ViewModels
{
    public class ReporteViewModel
    {
        // ── Resumen general ──────────────────────────────────
        public int TotalUsuarios { get; set; }
        public int UsuariosActivos { get; set; }
        public int UsuariosBloqueados { get; set; }
        public int TotalAutobuses { get; set; }
        public int AutobusesActivos { get; set; }
        public int TotalAccesos { get; set; }
        public int AccesosExitosos { get; set; }
        public int AccesosFallidos { get; set; }

        // ── Accesos por usuario ──────────────────────────────
        public List<AccesosPorUsuario> AccesosPorUsuario { get; set; } = new();

        // ── Intentos fallidos ────────────────────────────────
        public List<FallidosPorUsuario> FallidosPorUsuario { get; set; } = new();

        // ── Actividad por fecha (últimos 14 días) ────────────
        public List<ActividadPorFecha> ActividadPorFecha { get; set; } = new();

        // ── Usuarios bloqueados ──────────────────────────────
        public List<TransporteEscolar.Models.Usuario> UsuariosBloqueadosList { get; set; } = new();

        // Filtro de fecha
        public DateTime FechaDesde { get; set; } = DateTime.Today.AddDays(-30);
        public DateTime FechaHasta { get; set; } = DateTime.Today;
    }

    public class AccesosPorUsuario
    {
        public string NombreUsuario { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public int TotalAccesos { get; set; }
        public int Exitosos { get; set; }
        public int Fallidos { get; set; }
        public DateTime? UltimoAcceso { get; set; }
    }

    public class FallidosPorUsuario
    {
        public string NombreUsuario { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public int TotalFallidos { get; set; }
        public bool Bloqueado { get; set; }
        public string? UltimaIP { get; set; }
    }

    public class ActividadPorFecha
    {
        public DateTime Fecha { get; set; }
        public int Exitosos { get; set; }
        public int Fallidos { get; set; }
        public int Total => Exitosos + Fallidos;
    }
}