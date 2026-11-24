namespace Parqueadero.Core.Models;

public sealed record ResumenAforo(int CupoTotal, int Ocupados, int Disponibles)
{
    public double PorcentajeOcupacion => CupoTotal == 0 ? 0 : (double)Ocupados / CupoTotal * 100;
    public string Estado => Disponibles == 0
        ? "Lleno"
        : (double)Disponibles / CupoTotal <= 0.3 ? "Casi lleno" : "Disponible";
}
