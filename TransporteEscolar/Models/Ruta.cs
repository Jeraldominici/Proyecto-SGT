namespace TransporteEscolar.Models
{
    public class Ruta
    {
        public int RutaId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int? AutobusId { get; set; }
        public bool Activa { get; set; } = true;
        public DateTime FechaCreacion { get; set; }

        // Navegación
        public Autobus? Autobus { get; set; }
        public List<RutaParada> Paradas { get; set; } = new();
        public List<Viaje> Viajes { get; set; } = new();
    }
}