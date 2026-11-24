using System.ComponentModel.DataAnnotations;

namespace Parqueadero.Web.Models;

public class RegistroEgresadoViewModel
{
    [Required, StringLength(100)]
    [Display(Name = "Nombre completo")]
    public string NombreCompleto { get; set; } = "";

    [Required, EmailAddress]
    [Display(Name = "Correo personal")]
    public string CorreoPersonal { get; set; } = "";

    [Required, EmailAddress]
    [Display(Name = "Correo de la universidad")]
    public string CorreoUniversidad { get; set; } = "";

    [Required, StringLength(100, MinimumLength = 8)]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = "";

    [Required, Compare(nameof(Password))]
    [DataType(DataType.Password)]
    [Display(Name = "Confirmar contraseña")]
    public string ConfirmarPassword { get; set; } = "";
}
