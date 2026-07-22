namespace TransporteEscolar.ViewModels
{
    public class BitacoraViewModel
    {
        public List<TransporteEscolar.Models.BitacoraAcceso> Registros { get; set; } = new();

        // Filtros activos
        public string? FiltroUsuario { get; set; }
        public bool? FiltroExitoso { get; set; }
        public DateTime? FiltroDesde { get; set; }
        public DateTime? FiltroHasta { get; set; }

        // Paginación
        public int PaginaActual { get; set; } = 1;
        public int TotalPaginas { get; set; }
        public int TotalRegistros { get; set; }
        public int PorPagina { get; set; } = 20;

        // Estadísticas rápidas
        public int TotalExitosos => Registros.Count(r => r.Exitoso);
        public int TotalFallidos => Registros.Count(r => !r.Exitoso);

        public bool HayFiltros =>
            !string.IsNullOrWhiteSpace(FiltroUsuario) ||
            FiltroExitoso.HasValue ||
            FiltroDesde.HasValue ||
            FiltroHasta.HasValue;
    }
}