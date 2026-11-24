using Microsoft.AspNetCore.Mvc;
using Parqueadero.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Parqueadero.Core.Enums;
using Parqueadero.Data;
using Parqueadero.Web.Models;

namespace Parqueadero.Web.Controllers;

public class HomeController(IParqueaderoService parqueaderoService, AppDbContext context) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var configuracion = await context.ConfiguracionParqueadero.AsNoTracking().SingleAsync(cancellationToken);
        var tipos = new List<TipoParqueadero>();

        if (User.IsInRole("Estudiante"))
            tipos = [TipoParqueadero.Bloque6, TipoParqueadero.AtrasCanchas, TipoParqueadero.ArenaUSC];
        else if (User.IsInRole("Docente"))
            tipos = [TipoParqueadero.Bloque8, TipoParqueadero.ArenaUSC];
        else if (User.IsInRole("Egresado"))
            tipos = [TipoParqueadero.Plazoleta, TipoParqueadero.ArenaUSC];
        else if (User.IsInRole("Administrador"))
            tipos = [TipoParqueadero.Bloque6, TipoParqueadero.AtrasCanchas, TipoParqueadero.Bloque8, TipoParqueadero.Plazoleta, TipoParqueadero.ArenaUSC];
        else
            tipos = [TipoParqueadero.ArenaUSC];

        var items = new List<ParqueaderoAforoItem>();
        foreach (var tipo in tipos)
        {
            var cupo = tipo switch
            {
                TipoParqueadero.Bloque6 => configuracion.CupoBloque6,
                TipoParqueadero.AtrasCanchas => configuracion.CupoAtrasCanchas,
                TipoParqueadero.Bloque8 => configuracion.CupoBloque8,
                TipoParqueadero.Plazoleta => configuracion.CupoPlazoleta,
                _ => configuracion.CupoArenaUSC
            };
            var ocupados = await context.RegistrosAcceso.CountAsync(
                registro => registro.HoraSalida == null && registro.TipoParqueadero == tipo,
                cancellationToken);
            items.Add(new ParqueaderoAforoItem
            {
                Tipo = tipo,
                Nombre = Nombre(tipo),
                Descripcion = Descripcion(tipo),
                CupoTotal = cupo,
                Ocupados = ocupados,
                Abierto = tipo != TipoParqueadero.ArenaUSC || configuracion.ArenaUSCAbierta
            });
        }

        items = items.OrderByDescending(item => item.Disponibles).ToList();
        var titulo = User.IsInRole("Estudiante") ? "Tus parqueaderos" :
            User.IsInRole("Docente") ? "Parqueaderos para docentes" :
            User.IsInRole("Egresado") ? "Parqueaderos para egresados" :
            "Aforo en vivo";
        return View(new AforoPerfilViewModel { Titulo = titulo, Parqueaderos = items });
    }

    private static string Nombre(TipoParqueadero tipo) => tipo switch
    {
        TipoParqueadero.Bloque6 => "Bloque 6",
        TipoParqueadero.AtrasCanchas => "Canchas",
        TipoParqueadero.Bloque8 => "Bloque 8",
        TipoParqueadero.Plazoleta => "Plazoleta",
        _ => "Arena USC"
    };

    private static string Descripcion(TipoParqueadero tipo) => tipo switch
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
    };

    [HttpGet]
    public async Task<IActionResult> Aforo(CancellationToken cancellationToken)
    {
        Response.Headers.CacheControl = "no-store";
        return Json(await parqueaderoService.ObtenerAforoAsync(cancellationToken));
    }

    public IActionResult Error() => View(new ErrorViewModel
    {
        RequestId = HttpContext.TraceIdentifier
    });
}
