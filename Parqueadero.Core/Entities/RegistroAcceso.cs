using Parqueadero.Core.Enums;

namespace Parqueadero.Core.Entities;

public class RegistroAcceso
{
    public int Id { get; set; }
    public int TarjetaId { get; set; }
    public Tarjeta? Tarjeta { get; set; }
    public TipoParqueadero TipoParqueadero { get; set; } = TipoParqueadero.Estudiantes;
    public DateTime HoraEntrada { get; set; }
    public DateTime? HoraSalida { get; set; }
    public int? DuracionMinutos { get; set; }
}
