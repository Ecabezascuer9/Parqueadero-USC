using Parqueadero.Core.Entities;
using Parqueadero.Core.Models;

namespace Parqueadero.Core.Interfaces;

public interface IParqueaderoService
{
    Task<ResumenAforo> ObtenerAforoAsync(CancellationToken cancellationToken = default);
    Task<RegistroAcceso> RegistrarEntradaAsync(
        string codigoTarjeta,
        Parqueadero.Core.Enums.TipoParqueadero tipoParqueadero = Parqueadero.Core.Enums.TipoParqueadero.Estudiantes,
        CancellationToken cancellationToken = default);
    Task<RegistroAcceso> RegistrarSalidaAsync(string codigoTarjeta, CancellationToken cancellationToken = default);
    Task<ReservaParqueadero> SolicitarReservaAsync(
        string usuarioId,
        DateTime fechaHoraDeseada,
        int duracionHoras,
        CancellationToken cancellationToken = default);
}
