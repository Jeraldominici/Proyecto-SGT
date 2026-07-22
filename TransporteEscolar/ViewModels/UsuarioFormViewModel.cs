using System.ComponentModel.DataAnnotations;

namespace TransporteEscolar.ViewModels
{
    public class UsuarioFormViewModel
    {
        public int UsuarioId { get; set; }

        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
        [Display(Name = "Usuario")]
        public string NombreUsuario { get; set; } = string.Empty;

        [Display(Name = "Contraseña")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mínimo 6 caracteres.")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Display(Name = "Confirmar Contraseña")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        public string? ConfirmPassword { get; set; }

        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        [StringLength(200, ErrorMessage = "Máximo 200 caracteres.")]
        [Display(Name = "Nombre Completo")]
        public string NombreCompleto { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Formato de email inválido.")]
        [StringLength(200)]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Seleccione un rol.")]
        [Display(Name = "Rol")]
        public int RolId { get; set; }

        [Display(Name = "Autobús Asignado")]
        public int? AutobusId { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;

        // Para los dropdowns
        public List<TransporteEscolar.Models.Rol> Roles { get; set; } = new();
        public List<TransporteEscolar.Models.Autobus> Autobuses { get; set; } = new();

        // Para saber si es edición o creación
        public bool EsEdicion => UsuarioId > 0;
    }
}