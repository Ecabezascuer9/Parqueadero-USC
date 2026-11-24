namespace Parqueadero.Core.Entities;

public class ConfiguracionParqueadero
{
    public int Id { get; set; }
    public int CupoTotal { get; set; }
    public int CupoEstudiantes { get; set; } = 30;
    public int CupoDocentes { get; set; } = 15;
    public int CupoEgresados { get; set; } = 10;
    public int CupoArenaUSC { get; set; } = 5;
    public int CupoBloque6 { get; set; } = 20;
    public int CupoAtrasCanchas { get; set; } = 15;
    public int CupoBloque8 { get; set; } = 15;
    public int CupoPlazoleta { get; set; } = 10;
    public bool ArenaUSCAbierta { get; set; }
}
