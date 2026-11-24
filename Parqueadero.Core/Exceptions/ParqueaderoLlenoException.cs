namespace Parqueadero.Core.Exceptions;

public sealed class ParqueaderoLlenoException : Exception
{
    public ParqueaderoLlenoException() : base("El parqueadero está lleno.") { }
    public ParqueaderoLlenoException(string mensaje) : base(mensaje) { }
}
