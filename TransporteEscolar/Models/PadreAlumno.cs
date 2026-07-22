namespace TransporteEscolar.Models
{
    public class PadreAlumno
    {
        public int PadreAlumnoId { get; set; }
        public int UsuarioId { get; set; }
        public int AlumnoId { get; set; }

        public Usuario Usuario { get; set; } = null!;
        public Alumno Alumno { get; set; } = null!;
    }
}