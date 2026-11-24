using Parqueadero.Core.Enums;

namespace Parqueadero.Core.Entities;

public class Tarjeta
{
    public int Id { get; set; }
    public required string Codigo { get; set; }
    public EstadoTarjeta Estado { get; set; } = EstadoTarjeta.Activa;
    public ICollection<RegistroAcceso> Registros { get; set; } = new List<RegistroAcceso>();
}
