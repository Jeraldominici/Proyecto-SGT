using System.ComponentModel.DataAnnotations;
using TransporteEscolar.Models;

namespace TransporteEscolar.ViewModels
{
    public class AlumnoFormViewModel
    {
        public int AlumnoId { get; set; }

        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        [StringLength(200)]
        [Display(Name = "Nombre Completo")]
        public string NombreCompleto { get; set; } = string.Empty;

        [Display(Name = "Fecha de Nacimiento")]
        public DateOnly? FechaNacimiento { get; set; }

        [StringLength(50)]
        [Display(Name = "Grado / Año Escolar")]
        public string? GradoEscolar { get; set; }

        [StringLength(200)]
        [Display(Name = "Nombre del Tutor")]
        public string? NombreTutor { get; set; }

        [StringLength(20)]
        [Display(Name = "Teléfono de Emergencia")]
        public string? TelefonoEmergencia { get; set; }

        [Display(Name = "Foto")]
        public IFormFile? FotoArchivo { get; set; }
        public string? FotoUrlActual { get; set; }

        [StringLength(300)]
        [Display(Name = "Dirección de Recogida")]
        public string? DireccionRecogida { get; set; }

        [StringLength(300)]
        [Display(Name = "Dirección de Entrega")]
        public string? DireccionEntrega { get; set; }

        [Required(ErrorMessage = "Selecciona un autobús.")]
        [Display(Name = "Autobús")]
        public int AutobusId { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;

        public List<Autobus> Autobuses { get; set; } = new();
        public bool EsEdicion => AlumnoId > 0;
    }
}