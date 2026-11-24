using Parqueadero.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace Parqueadero.Web.Models;

public class TarjetasViewModel
{
    public IReadOnlyList<Tarjeta> Tarjetas { get; set; } = [];

    [Required, StringLength(30)]
    [Display(Name = "Código de tarjeta")]
    public string NuevoCodigo { get; set; } = "";
}
