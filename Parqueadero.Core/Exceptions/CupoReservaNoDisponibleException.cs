namespace Parqueadero.Core.Exceptions;

public sealed class CupoReservaNoDisponibleException()
    : Exception("Se agotaron los cupos disponibles para ese horario. Las reservas de llegada dentro de la próxima hora están limitadas por el aforo.");
