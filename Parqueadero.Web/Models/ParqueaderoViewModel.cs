using Parqueadero.Core.Enums;

namespace Parqueadero.Web.Models;

public class ParqueaderoViewModel
{
    public TipoParqueadero Tipo { get; set; }
    public string Nombre { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public int CupoTotal { get; set; }
    public int Ocupados { get; set; }
    public bool Abierto { get; set; } = true;
    public int Disponibles => Abierto ? Math.Max(0, CupoTotal - Ocupados) : 0;
}
