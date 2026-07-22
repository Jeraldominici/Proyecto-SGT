namespace TransporteEscolar.Models
{
    public class BitacoraAcceso
    {
        public int BitacoraId { get; set; }
        public int? UsuarioId { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public DateTime FechaHora { get; set; }
        public bool Exitoso { get; set; }
        public string? DireccionIP { get; set; }
        public string? UserAgent { get; set; }
        public string? Detalle { get; set; }

        public Usuario? Usuario { get; set; }
    }
}