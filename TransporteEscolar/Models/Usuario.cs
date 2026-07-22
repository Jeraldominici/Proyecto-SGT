namespace TransporteEscolar.Models
{
    /// <summary>
    /// Representa un usuario autenticable del sistema.
    /// </summary>
    public class Usuario
    {
        public int UsuarioId { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string? Email { get; set; }
        public int RolId { get; set; }
        public int? AutobusId { get; set; }
        public bool Activo { get; set; } = true;
        public int IntentosFallidos { get; set; }
        public bool Bloqueado { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? UltimoAcceso { get; set; }
        public string? FotoUrl { get; set; }

        // Navegación
        public Rol Rol { get; set; } = null!;
        public Autobus? Autobus { get; set; }
    }
}