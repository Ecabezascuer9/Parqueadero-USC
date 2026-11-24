namespace Parqueadero.Core.Exceptions;

public sealed class TarjetaInactivaException : Exception
{
    public TarjetaInactivaException()
        : base("La tarjeta no existe o está inactiva.") { }

    public TarjetaInactivaException(string mensaje)
        : base(mensaje) { }
}
