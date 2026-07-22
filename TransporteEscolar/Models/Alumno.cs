namespace TransporteEscolar.Models
{
    public class Alumno
    {
        public int AlumnoId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public DateOnly? FechaNacimiento { get; set; }
        public string? GradoEscolar { get; set; }
        public string? NombreTutor { get; set; }
        public string? TelefonoEmergencia { get; set; }
        public string? FotoUrl { get; set; }
        public string? DireccionRecogida { get; set; }
        public string? DireccionEntrega { get; set; }
        public int AutobusId { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime FechaAlta { get; set; }
        public string CodigoQR { get; set; } = string.Empty;

        // Navegación
        public Autobus Autobus { get; set; } = null!;
        public ICollection<Asistencia> Asistencias { get; set; } = new List<Asistencia>();
        public ICollection<Incidencia> Incidencias { get; set; } = new List<Incidencia>();
    }
}