using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parqueadero.Core.Exceptions;
using Parqueadero.Core.Interfaces;
using Parqueadero.Data;
using Parqueadero.Web.Models;
using System.Security.Claims;

namespace Parqueadero.Web.Controllers;

[Authorize(Roles = "Administrador,Empleado,Estudiante")]
public class AccesoController(
    IParqueaderoService parqueaderoService,
    AppDbContext context) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewBag.Aforo = await parqueaderoService.ObtenerAforoAsync(cancellationToken);
        var consulta = context.RegistrosAcceso
            .AsNoTracking()
            .Include(registro => registro.Tarjeta)
            .AsQueryable();

        if (User.IsInRole("Estudiante"))
        {
            var usuarioId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(usuarioId))
            {
                return Challenge();
            }

            var tarjetasReservadas = await context.ReservasParqueadero
                .AsNoTracking()
                .Where(reserva => reserva.UsuarioId == usuarioId)
                .Select(reserva => reserva.TarjetaId)
                .Distinct()
                .ToListAsync(cancellationToken);

            consulta = consulta.Where(registro => tarjetasReservadas.Contains(registro.TarjetaId));
        }

        ViewBag.Recientes = await consulta
            .OrderByDescending(registro => registro.HoraEntrada)
            .Take(8)
            .ToListAsync(cancellationToken);
        return View(new AccesoViewModel());
    }

    [Authorize(Roles = "Administrador,Empleado")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registrar(AccesoViewModel model, CancellationToken cancellationToken)
    {
        model.CodigoTarjeta = model.CodigoTarjeta?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(model.CodigoTarjeta))
        {
            ModelState.AddModelError(nameof(model.CodigoTarjeta), "Ingresa el código de la tarjeta.");
        }

        if (!ModelState.IsValid || model.Operacion is not ("Entrada" or "Salida"))
        {
            ModelState.AddModelError(string.Empty, "Verifica los datos del registro.");
            return await Index(cancellationToken);
        }

        try
        {
            var registro = model.Operacion == "Entrada"
                ? await parqueaderoService.RegistrarEntradaAsync(model.CodigoTarjeta, model.TipoParqueadero, cancellationToken)
                : await parqueaderoService.RegistrarSalidaAsync(model.CodigoTarjeta, cancellationToken);

            TempData["Mensaje"] = model.Operacion == "Entrada"
                ? $"Entrada registrada para la tarjeta {model.CodigoTarjeta.Trim().ToUpperInvariant()}."
                : $"Salida registrada. Permanencia: {registro.DuracionMinutos} minutos.";
        }
        catch (Exception exception) when (exception is ParqueaderoLlenoException
            or TarjetaEnUsoException
            or TarjetaInactivaException
            or EntradaNoActivaException)
        {
            TempData["Error"] = exception.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
