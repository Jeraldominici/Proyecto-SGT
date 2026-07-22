using System.ComponentModel.DataAnnotations;

namespace TransporteEscolar.ViewModels
{
    public class IncidenciaViewModel
    {
        [Required(ErrorMessage = "El título es obligatorio.")]
        [StringLength(100)]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(1000)]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecciona un tipo.")]
        public string Tipo { get; set; } = string.Empty;

        public int? AlumnoId { get; set; }
    }
}