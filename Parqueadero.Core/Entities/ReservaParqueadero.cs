using System.ComponentModel.DataAnnotations.Schema;
using Parqueadero.Core.Enums;

namespace Parqueadero.Core.Entities;

public class ReservaParqueadero
{
    public int Id { get; set; }
    public required string CodigoTicket { get; set; }
    public required string UsuarioId { get; set; }
    public int TarjetaId { get; set; }
    public Tarjeta? Tarjeta { get; set; }
    public DateTime FechaHoraDeseada { get; set; }
    public int DuracionHoras { get; set; }
    public EstadoReserva Estado { get; set; } = EstadoReserva.Confirmada;

    [NotMapped]
    public DateTime FinEstimado => FechaHoraDeseada.AddHours(DuracionHoras);
}
