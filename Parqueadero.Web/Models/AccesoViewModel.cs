using System.ComponentModel.DataAnnotations;

using Parqueadero.Core.Enums;

namespace Parqueadero.Web.Models;

public class AccesoViewModel
{
    [Required, StringLength(30)]
    [Display(Name = "Código de tarjeta")]
    public string CodigoTarjeta { get; set; } = "";

    [Required]
    public string Operacion { get; set; } = "Entrada";

    [Display(Name = "Parqueadero")]
    public TipoParqueadero TipoParqueadero { get; set; } = TipoParqueadero.Estudiantes;
}
