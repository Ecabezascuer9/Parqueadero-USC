using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Parqueadero.Core.Enums;
using Parqueadero.Data;
using Parqueadero.Web.Models;

namespace Parqueadero.Web.Controllers;

public class ParqueaderosController(AppDbContext context) : Controller
{
    [Authorize(Roles = "Estudiante,Administrador")]
    public Task<IActionResult> Estudiantes(CancellationToken cancellationToken) =>
        MostrarAsync(TipoParqueadero.Bloque6, "Parqueaderos de estudiantes", cancellationToken);

    [Authorize(Roles = "Docente,Administrador")]
    public Task<IActionResult> Docentes(CancellationToken cancellationToken) =>
        MostrarAsync(TipoParqueadero.Bloque8, "Parqueadero de docentes", cancellationToken);

    [Authorize(Roles = "Egresado,Administrador")]
    public Task<IActionResult> Egresados(CancellationToken cancellationToken) =>
        MostrarAsync(TipoParqueadero.Plazoleta, "Parqueadero de egresados", cancellationToken);

    [AllowAnonymous]
    public Task<IActionResult> Arena(CancellationToken cancellationToken) =>
        MostrarAsync(TipoParqueadero.ArenaUSC, "Parqueadero arena USC", cancellationToken);

    private async Task<IActionResult> MostrarAsync(TipoParqueadero tipo, string nombre, CancellationToken cancellationToken)
    {
        var config = await context.ConfiguracionParqueadero.AsNoTracking().SingleAsync(cancellationToken);
        var cupo = tipo switch
        {
            TipoParqueadero.Estudiantes => config.CupoEstudiantes,
            TipoParqueadero.Docentes => config.CupoDocentes,
            TipoParqueadero.Egresados => config.CupoEgresados,
            TipoParqueadero.Bloque6 => config.CupoBloque6,
            TipoParqueadero.AtrasCanchas => config.CupoAtrasCanchas,
            TipoParqueadero.Bloque8 => config.CupoBloque8,
            TipoParqueadero.Plazoleta => config.CupoPlazoleta,
            _ => config.CupoArenaUSC
        };
        var ocupados = await context.RegistrosAcceso.CountAsync(
            registro => registro.HoraSalida == null && registro.TipoParqueadero == tipo,
            cancellationToken);

        return View("Index", new ParqueaderoViewModel
        {
            Tipo = tipo,
            Nombre = nombre,
            Descripcion = tipo switch
            {
                TipoParqueadero.Bloque6 =>
                    "Debajo del Bloque 6, diseñado para guardar motos y carros con seguridad y comodidad para los estudiantes.",
                TipoParqueadero.AtrasCanchas =>
                    "Ubicado al fondo de las canchas de fútbol y cerca del jardín de niños, un lugar único para que los estudiantes guarden sus vehículos.",
                TipoParqueadero.Bloque8 =>
                    "Parqueadero ubicado junto al Bloque 8 y habilitado para docentes.",
                TipoParqueadero.Plazoleta =>
                    "Parqueadero ubicado en la plazoleta y habilitado para egresados.",
                _ =>
                    "Parqueadero hecho para eventos de la Arena USC y el Gran Auditorio, abierto a todo el público."
            },
            CupoTotal = cupo,
            Ocupados = ocupados,
            Abierto = tipo != TipoParqueadero.ArenaUSC || config.ArenaUSCAbierta
        });
    }
}
