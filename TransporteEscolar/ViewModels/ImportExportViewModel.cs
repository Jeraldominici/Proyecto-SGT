namespace TransporteEscolar.ViewModels
{
    public class ImportPreviewViewModel
    {
        public string Entidad { get; set; } = string.Empty;
        public List<string> Columnas { get; set; } = new();
        public List<List<string>> Filas { get; set; } = new();
        public List<string> Errores { get; set; } = new();
        public string DatosJson { get; set; } = string.Empty;
        public int TotalValidos => Filas.Count - Errores.Count;
    }
}