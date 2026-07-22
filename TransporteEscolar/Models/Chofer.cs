namespace TransporteEscolar.Models
{
    public class Chofer
    {
        public int ChoferId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string DUI { get; set; } = string.Empty;
        public string Licencia { get; set; } = string.Empty;
        public string? FotoUrl { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime FechaAlta { get; set; }

        // Navegación
        public ICollection<JornadaChofer> Jornadas { get; set; } = new List<JornadaChofer>();
    }
}