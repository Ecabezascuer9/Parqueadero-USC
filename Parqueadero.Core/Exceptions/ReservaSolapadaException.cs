namespace Parqueadero.Core.Exceptions;

public sealed class ReservaSolapadaException()
    : Exception("Ya tienes una reserva que se cruza con ese horario.");
