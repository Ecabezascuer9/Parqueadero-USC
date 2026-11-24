using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parqueadero.Data;
using Parqueadero.Core.Enums;
using Parqueadero.Web.Models;

namespace Parqueadero.Web.Controllers;

[Authorize(Roles = "Administrador")]
public class ConfiguracionController(AppDbContext context) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var configuracion = await context.ConfiguracionParqueadero
            .AsNoTracking()
            .SingleAsync(cancellationToken);
        var ocupados = await context.RegistrosAcceso
            .CountAsync(registro => registro.HoraSalida == null, cancellationToken);
        var reservasSimultaneas = await ObtenerMaximoReservadoAsync(cancellationToken);

        return View(new ConfiguracionViewModel
        {
            CupoTotal = configuracion.CupoTotal,
            CupoEstudiantes = configuracion.CupoEstudiantes,
            CupoDocentes = configuracion.CupoDocentes,
            CupoEgresados = configuracion.CupoEgresados,
            CupoArenaUSC = configuracion.CupoArenaUSC,
            CupoBloque6 = configuracion.CupoBloque6,
            CupoAtrasCanchas = configuracion.CupoAtrasCanchas,
            CupoBloque8 = configuracion.CupoBloque8,
            CupoPlazoleta = configuracion.CupoPlazoleta,
            ArenaUSCAbierta = configuracion.ArenaUSCAbierta,
            Ocupados = ocupados,
            ReservasSimultaneas = reservasSimultaneas
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ConfiguracionViewModel model, CancellationToken cancellationToken)
    {
        var configuracion = await context.ConfiguracionParqueadero.SingleAsync(cancellationToken);
        model.CupoTotal = model.CupoBloque6 + model.CupoAtrasCanchas +
            model.CupoBloque8 + model.CupoPlazoleta + model.CupoArenaUSC;
        model.Ocupados = await context.RegistrosAcceso
            .CountAsync(registro => registro.HoraSalida == null, cancellationToken);
        model.ReservasSimultaneas = await ObtenerMaximoReservadoAsync(cancellationToken);

        if (ModelState.IsValid && model.CupoTotal < model.Ocupados)
        {
            ModelState.AddModelError(nameof(model.CupoTotal), "El aforo no puede ser menor que los vehículos que están dentro.");
        }

        if (ModelState.IsValid && model.CupoTotal < model.ReservasSimultaneas)
        {
            ModelState.AddModelError(nameof(model.CupoTotal), "El aforo no puede ser menor que las reservas confirmadas que coinciden en el horario de mayor demanda.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        configuracion.CupoEstudiantes = model.CupoEstudiantes;
        configuracion.CupoDocentes = model.CupoDocentes;
        configuracion.CupoEgresados = model.CupoEgresados;
        configuracion.CupoArenaUSC = model.CupoArenaUSC;
        configuracion.CupoBloque6 = model.CupoBloque6;
        configuracion.CupoAtrasCanchas = model.CupoAtrasCanchas;
        configuracion.CupoBloque8 = model.CupoBloque8;
        configuracion.CupoPlazoleta = model.CupoPlazoleta;
        configuracion.ArenaUSCAbierta = model.ArenaUSCAbierta;
        configuracion.CupoTotal = model.CupoBloque6 + model.CupoAtrasCanchas +
            model.CupoBloque8 + model.CupoPlazoleta + model.CupoArenaUSC;
        await context.SaveChangesAsync(cancellationToken);
        TempData["Mensaje"] = "La capacidad del parqueadero se actualizó.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<int> ObtenerMaximoReservadoAsync(CancellationToken cancellationToken)
    {
        var ahora = DateTime.Now;
        var reservas = await context.ReservasParqueadero
            .AsNoTracking()
            .Where(reserva =>
                reserva.Estado == EstadoReserva.Confirmada &&
                reserva.FechaHoraDeseada >= ahora.AddHours(-12))
            .Select(reserva => new { reserva.FechaHoraDeseada, reserva.DuracionHoras })
            .ToListAsync(cancellationToken);

        var periodos = reservas
            .Select(reserva => (
                Inicio: reserva.FechaHoraDeseada,
                Fin: reserva.FechaHoraDeseada.AddHours(reserva.DuracionHoras)))
            .Where(periodo => periodo.Fin > ahora);
        var eventos = periodos
            .Select(periodo => (
                Momento: periodo.Inicio < ahora ? ahora : periodo.Inicio,
                Cambio: 1))
            .Concat(periodos.Select(periodo => (
                Momento: periodo.Fin,
                Cambio: -1)))
            .OrderBy(evento => evento.Momento)
            .ThenBy(evento => evento.Cambio);

        var actual = 0;
        var maximo = 0;
        foreach (var evento in eventos)
        {
            actual += evento.Cambio;
            maximo = Math.Max(maximo, actual);
        }

        return maximo;
    }
}
