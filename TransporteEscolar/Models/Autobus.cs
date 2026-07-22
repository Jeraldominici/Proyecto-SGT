namespace TransporteEscolar.Models
{
    /// <summary>
    /// Representa un autobús escolar registrado en el sistema.
    /// </summary>
    public class Autobus
    {
        public int AutobusId { get; set; }
        public string Ficha { get; set; } = string.Empty;
        public string Placa { get; set; } = string.Empty;
        public int Capacidad { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime FechaAlta { get; set; }

        // Navegación
        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    }
}