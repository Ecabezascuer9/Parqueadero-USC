using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parqueadero.Core.Entities;
using Parqueadero.Core.Enums;
using Parqueadero.Data;
using Parqueadero.Web.Models;

namespace Parqueadero.Web.Controllers;

[Authorize(Roles = "Administrador")]
public class TarjetasController(AppDbContext context) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        return View(new TarjetasViewModel
        {
            Tarjetas = await context.Tarjetas.AsNoTracking().OrderBy(item => item.Codigo).ToListAsync(cancellationToken)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(TarjetasViewModel model, CancellationToken cancellationToken)
    {
        var codigo = model.NuevoCodigo.Trim().ToUpperInvariant();
        if (!ModelState.IsValid)
        {
            model.Tarjetas = await context.Tarjetas
                .AsNoTracking()
                .OrderBy(item => item.Codigo)
                .ToListAsync(cancellationToken);
            return View("Index", model);
        }

        if (await context.Tarjetas.AnyAsync(item => item.Codigo == codigo, cancellationToken))
        {
            TempData["Error"] = "Ya existe una tarjeta con ese código.";
            return RedirectToAction(nameof(Index));
        }

        context.Tarjetas.Add(new Tarjeta { Codigo = codigo });
        await context.SaveChangesAsync(cancellationToken);
        TempData["Mensaje"] = "Tarjeta creada.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(int id, CancellationToken cancellationToken)
    {
        var tarjeta = await context.Tarjetas.FindAsync([id], cancellationToken);
        if (tarjeta is null)
        {
            return NotFound();
        }

        var tieneEntrada = await context.RegistrosAcceso.AnyAsync(
            registro => registro.TarjetaId == id && registro.HoraSalida == null,
            cancellationToken);
        if (tieneEntrada && tarjeta.Estado == EstadoTarjeta.Activa)
        {
            TempData["Error"] = "No se puede desactivar una tarjeta que está dentro del parqueadero.";
            return RedirectToAction(nameof(Index));
        }

        tarjeta.Estado = tarjeta.Estado == EstadoTarjeta.Activa
            ? EstadoTarjeta.Inactiva
            : EstadoTarjeta.Activa;
        await context.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Index));
    }
}
