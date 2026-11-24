using System.ComponentModel.DataAnnotations;
using Parqueadero.Core.Entities;

namespace Parqueadero.Web.Models;

public class ReservasViewModel
{
    public IReadOnlyList<ReservaParqueadero> Reservas { get; set; } = [];
    public IReadOnlyDictionary<string, string> CorreosUsuarios { get; set; } =
        new Dictionary<string, string>();

    [Required]
    [Display(Name = "Fecha y hora deseada")]
    public DateTime? FechaHoraDeseada { get; set; }

    [Range(1, 12)]
    [Display(Name = "Duración estimada (horas)")]
    public int DuracionHoras { get; set; } = 2;
}
