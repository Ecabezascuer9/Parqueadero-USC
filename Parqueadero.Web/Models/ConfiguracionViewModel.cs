using System.ComponentModel.DataAnnotations;

namespace Parqueadero.Web.Models;

public class ConfiguracionViewModel
{
    [Range(1, 10000)]
    [Display(Name = "Cupos totales")]
    public int CupoTotal { get; set; }

    [Range(1, 10000), Display(Name = "Cupos estudiantes")]
    public int CupoEstudiantes { get; set; }

    [Range(1, 10000), Display(Name = "Cupos docentes")]
    public int CupoDocentes { get; set; }

    [Range(1, 10000), Display(Name = "Cupos egresados")]
    public int CupoEgresados { get; set; }

    [Range(1, 10000), Display(Name = "Cupos arena USC")]
    public int CupoArenaUSC { get; set; }

    [Range(1, 10000), Display(Name = "Cupos Bloque 6")]
    public int CupoBloque6 { get; set; }

    [Range(1, 10000), Display(Name = "Cupos en Canchas")]
    public int CupoAtrasCanchas { get; set; }

    [Range(1, 10000), Display(Name = "Cupos Bloque 8")]
    public int CupoBloque8 { get; set; }

    [Range(1, 10000), Display(Name = "Cupos plazoleta")]
    public int CupoPlazoleta { get; set; }

    [Display(Name = "Arena USC abierta")]
    public bool ArenaUSCAbierta { get; set; }

    public int Ocupados { get; set; }

    public int ReservasSimultaneas { get; set; }
}
