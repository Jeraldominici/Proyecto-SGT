namespace TransporteEscolar.Models
{
    /// <summary>
    /// Representa un rol del sistema (Admin, Azafata, Padre).
    /// </summary>
    public class Rol
    {
        public int RolId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Activo { get; set; } = true;

        // Navegación
        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    }
}