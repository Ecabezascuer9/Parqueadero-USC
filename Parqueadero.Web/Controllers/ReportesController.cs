using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parqueadero.Data;
using Parqueadero.Web.Models;

namespace Parqueadero.Web.Controllers;

[Authorize(Roles = "Administrador")]
public class ReportesController(AppDbContext context) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var desde = DateTime.Today.AddDays(-6);
        var ahora = DateTime.Now;
        var historial = await context.RegistrosAcceso
            .AsNoTracking()
            .Where(item => item.HoraEntrada < DateTime.Today.AddDays(1)
                && (item.HoraSalida == null || item.HoraSalida >= desde))
            .ToListAsync(cancellationToken);
        var registrosSemana = historial.Where(item => item.HoraEntrada >= desde).ToList();
        var finalizados = registrosSemana.Where(item => item.DuracionMinutos.HasValue).ToList();
        var dias = Enumerable.Range(0, 7).Select(offset => desde.AddDays(offset)).ToList();

        int OcupadosEn(DateTime instante) => historial.Count(item =>
            item.HoraEntrada <= instante && (item.HoraSalida is null || item.HoraSalida > instante));

        var porDia = dias
            .Select(offset =>
            {
                var puntos = Enumerable.Range(0, 24)
                    .Select(hora => offset.AddHours(hora))
                    .Where(instante => instante <= ahora)
                    .Select(instante => instante.Date == ahora.Date && instante.Hour == ahora.Hour ? ahora : instante)
                    .Select(OcupadosEn)
                    .ToList();
                return (offset.ToString("ddd dd/MM"), puntos.Count == 0 ? 0 : puntos.Max());
            })
            .ToList();
        var porHora = Enumerable.Range(0, 24)
            .Select(hora =>
            {
                var valores = dias
                    .Select(dia => dia.AddHours(hora))
                    .Where(instante => instante <= ahora)
                    .Select(instante => instante.Date == ahora.Date && instante.Hour == ahora.Hour ? ahora : instante)
                    .Select(OcupadosEn)
                    .ToList();
                return ($"{hora:00}:00", valores.Count == 0 ? 0 : (int)Math.Round(valores.Average()));
            })
            .ToList();

        return View(new ReportesViewModel
        {
            PermanenciaPromedioMinutos = finalizados.Count == 0 ? 0 : finalizados.Average(item => item.DuracionMinutos!.Value),
            EntradasHoy = registrosSemana.Count(item => item.HoraEntrada.Date == DateTime.Today),
            EntradasALasDiez = registrosSemana.Count(item => item.HoraEntrada.Hour == 10),
            EntradasALasCinco = registrosSemana.Count(item => item.HoraEntrada.Hour == 17),
            OcupacionPorDia = porDia,
            OcupacionPorHora = porHora
        });
    }
}
