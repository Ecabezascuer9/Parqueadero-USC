using Parqueadero.Data.Identity;
using System.ComponentModel.DataAnnotations;

namespace Parqueadero.Web.Models;

public class UsuariosViewModel
{
    public IReadOnlyList<AplicacionUser> Usuarios { get; set; } = [];
    public IReadOnlyList<string> Roles { get; set; } = [];
    public IReadOnlyDictionary<string, string> RolesPorUsuario { get; set; } =
        new Dictionary<string, string>();

    [Required, StringLength(100)]
    [Display(Name = "Nombre completo")]
    public string NombreCompleto { get; set; } = "";

    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required, StringLength(100, MinimumLength = 8)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [Required]
    public string Rol { get; set; } = "Estudiante";
}
