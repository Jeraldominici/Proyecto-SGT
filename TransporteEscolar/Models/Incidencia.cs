namespace TransporteEscolar.Models
{
    public class Incidencia
    {
        public int IncidenciaId { get; set; }
        public int AutobusId { get; set; }
        public int UsuarioId { get; set; }
        public int? AlumnoId { get; set; }
        public DateOnly Fecha { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public DateTime FechaHora { get; set; }

        // Navegación
        public Autobus Autobus { get; set; } = null!;
        public Usuario Usuario { get; set; } = null!;
        public Alumno? Alumno { get; set; }
    }
}