namespace Parqueadero.Core.Exceptions;

public sealed class FechaReservaInvalidaException()
    : Exception("El horario debe comenzar al menos 10 minutos después de la hora actual.");
