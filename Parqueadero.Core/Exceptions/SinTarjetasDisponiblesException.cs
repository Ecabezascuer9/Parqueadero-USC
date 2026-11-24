namespace Parqueadero.Core.Exceptions;

public sealed class SinTarjetasDisponiblesException()
    : Exception("No hay tarjetas físicas disponibles para asignar en ese horario.");
