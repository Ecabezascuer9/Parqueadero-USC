namespace Parqueadero.Web.Models;

public class ReportesViewModel
{
    public double PermanenciaPromedioMinutos { get; set; }
    public int EntradasHoy { get; set; }
    public int EntradasALasDiez { get; set; }
    public int EntradasALasCinco { get; set; }
    public IReadOnlyList<(string Etiqueta, int Cantidad)> OcupacionPorDia { get; set; } = [];
    public IReadOnlyList<(string Etiqueta, int Cantidad)> OcupacionPorHora { get; set; } = [];
}
