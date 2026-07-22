namespace TransporteEscolar.Models
{
    public class Asistencia
    {
        public int AsistenciaId { get; set; }
        public int AlumnoId { get; set; }
        public int AutobusId { get; set; }
        public int UsuarioId { get; set; }
        public DateOnly Fecha { get; set; }
        public string TipoRuta { get; set; } = string.Empty; // "Ida" | "Vuelta"
        public bool Presente { get; set; } = true;
        public string? Observacion { get; set; }
        public DateTime FechaHora { get; set; }

        // Navegación
        public Alumno Alumno { get; set; } = null!;
        public Autobus Autobus { get; set; } = null!;
        public Usuario Usuario { get; set; } = null!;
    }
}