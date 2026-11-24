using Microsoft.AspNetCore.Identity;

namespace Parqueadero.Data.Identity;

public class AplicacionUser : IdentityUser
{
    public string NombreCompleto { get; set; } = "";
    public string? CorreoPersonal { get; set; }
    public string? CorreoUniversidad { get; set; }
}
