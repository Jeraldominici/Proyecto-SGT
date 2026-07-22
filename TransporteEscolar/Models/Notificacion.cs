namespace TransporteEscolar.Models
{
    public class Notificacion
    {
        public int NotificacionId { get; set; }
        public int UsuarioId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public int? AlumnoId { get; set; }
        public bool Leida { get; set; }
        public DateTime FechaHora { get; set; }

        public Usuario Usuario { get; set; } = null!;
        public Alumno? Alumno { get; set; }

        // Helper
        public string Icono => Tipo switch
        {
            "Ausencia" => "⚠️",
            "Incidencia" => "🚨",
            "Jornada" => "🚌",
            "Manual" => "📢",
            _ => "🔔"
        };
    }
}