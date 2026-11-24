using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parqueadero.Core.Enums;
using Parqueadero.Core.Exceptions;
using Parqueadero.Core.Interfaces;
using Parqueadero.Data;
using Parqueadero.Web.Models;
using System.Security.Claims;

namespace Parqueadero.Web.Controllers;

[Authorize(Roles = "Docente,Administrador")]
public class ReservasController(
    AppDbContext context,
    IParqueaderoService parqueaderoService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        return View(await CrearModeloAsync(cancellationToken));
    }

    [Authorize(Roles = "Docente")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Solicitar(ReservasViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid || model.FechaHoraDeseada is null)
        {
            return await VolverConModeloAsync(model, cancellationToken);
        }

        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(usuarioId))
        {
            return Challenge();
        }

        try
        {
            var reserva = await parqueaderoService.SolicitarReservaAsync(
                usuarioId,
                model.FechaHoraDeseada.Value,
                model.DuracionHoras,
                cancellationToken);
            TempData["Mensaje"] =
                $"Solicitud confirmada. Ticket: {reserva.CodigoTicket} · Tarjeta asignada: {reserva.Tarjeta?.Codigo}.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception exception) when (exception is FechaReservaInvalidaException
            or ReservaSolapadaException
            or CupoReservaNoDisponibleException
            or SinTarjetasDisponiblesException)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return await VolverConModeloAsync(model, cancellationToken);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id, CancellationToken cancellationToken)
    {
        var reserva = await context.ReservasParqueadero
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (reserva is null)
        {
            return NotFound();
        }

        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!User.IsInRole("Administrador") && reserva.UsuarioId != usuarioId)
        {
            return Forbid();
        }

        if (reserva.Estado != EstadoReserva.Confirmada || DateTime.Now >= reserva.FechaHoraDeseada)
        {
            TempData["Error"] = "Solo puedes cancelar reservas confirmadas antes de su hora programada.";
            return RedirectToAction(nameof(Index));
        }

        reserva.Estado = EstadoReserva.Cancelada;
        await context.SaveChangesAsync(cancellationToken);
        TempData["Mensaje"] = $"Se canceló la reserva {reserva.CodigoTicket}.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<IActionResult> VolverConModeloAsync(
        ReservasViewModel model,
        CancellationToken cancellationToken)
    {
        var datos = await CrearModeloAsync(cancellationToken);
        model.Reservas = datos.Reservas;
        model.CorreosUsuarios = datos.CorreosUsuarios;
        ViewBag.MinFechaHora = DateTime.Now.AddMinutes(10).ToString("yyyy-MM-ddTHH:mm");
        return View("Index", model);
    }

    private async Task<ReservasViewModel> CrearModeloAsync(CancellationToken cancellationToken)
    {
        var consulta = context.ReservasParqueadero
            .AsNoTracking()
            .Include(reserva => reserva.Tarjeta)
            .AsQueryable();

        if (!User.IsInRole("Administrador"))
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            consulta = consulta.Where(reserva => reserva.UsuarioId == usuarioId);
        }

        var reservas = await consulta
            .OrderByDescending(reserva => reserva.FechaHoraDeseada)
            .ToListAsync(cancellationToken);
        var idsUsuarios = reservas.Select(reserva => reserva.UsuarioId).Distinct().ToList();
        var correos = await context.Users
            .AsNoTracking()
            .Where(usuario => idsUsuarios.Contains(usuario.Id))
            .ToDictionaryAsync(usuario => usuario.Id, usuario => usuario.Email ?? "", cancellationToken);

        ViewBag.MinFechaHora = DateTime.Now.AddMinutes(10).ToString("yyyy-MM-ddTHH:mm");
        return new ReservasViewModel
        {
            Reservas = reservas,
            CorreosUsuarios = correos
        };
    }
}
