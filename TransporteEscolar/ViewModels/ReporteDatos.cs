namespace TransporteEscolar.ViewModels
{
    public class ReporteDatos
    {
        public int TotalUsuarios { get; set; }
        public int UsuariosActivos { get; set; }
        public int UsuariosBloqueados { get; set; }
        public int TotalAutobuses { get; set; }
        public int AutobusesActivos { get; set; }
        public int TotalAccesos { get; set; }
        public int AccesosExitosos { get; set; }
        public int AccesosFallidos { get; set; }

        public List<TransporteEscolar.Models.BitacoraAcceso> Bitacora { get; set; } = new();
        public List<TransporteEscolar.Models.Usuario> Usuarios { get; set; } = new();
        public List<TransporteEscolar.Models.Autobus> Autobuses { get; set; } = new();
    }
}