namespace TransporteEscolar.Models
{
    public class JornadaChofer
    {
        public int JornadaId { get; set; }
        public int UsuarioId { get; set; }
        public int ChoferId { get; set; }
        public int AutobusId { get; set; }
        public DateTime FechaHora { get; set; }
        public bool Activa { get; set; } = true;

        // Navegación
        public Usuario Usuario { get; set; } = null!;
        public Chofer Chofer { get; set; } = null!;
        public Autobus Autobus { get; set; } = null!;
    }
}