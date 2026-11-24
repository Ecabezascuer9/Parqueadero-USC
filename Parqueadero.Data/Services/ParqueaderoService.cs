using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Parqueadero.Core.Entities;
using Parqueadero.Core.Enums;
using Parqueadero.Core.Exceptions;
using Parqueadero.Core.Interfaces;
using Parqueadero.Core.Models;

namespace Parqueadero.Data.Services;

public sealed class ParqueaderoService(AppDbContext context) : IParqueaderoService
{
    public async Task<ResumenAforo> ObtenerAforoAsync(CancellationToken cancellationToken = default)
    {
        var configuracion = await context.ConfiguracionParqueadero
            .AsNoTracking()
            .SingleAsync(cancellationToken);
        var ocupados = await context.RegistrosAcceso
            .CountAsync(registro => registro.HoraSalida == null, cancellationToken);

        return new ResumenAforo(configuracion.CupoTotal, ocupados, configuracion.CupoTotal - ocupados);
    }

    public async Task<RegistroAcceso> RegistrarEntradaAsync(
        string codigoTarjeta,
        TipoParqueadero tipoParqueadero = TipoParqueadero.Estudiantes,
        CancellationToken cancellationToken = default)
    {
        var codigo = NormalizarCodigo(codigoTarjeta);
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        var tarjeta = await context.Tarjetas.SingleOrDefaultAsync(
            item => item.Codigo == codigo,
            cancellationToken);
        if (tarjeta is null || tarjeta.Estado != EstadoTarjeta.Activa)
        {
            throw new TarjetaInactivaException();
        }

        var tarjetasActivas = (await context.RegistrosAcceso
            .Where(registro => registro.HoraSalida == null)
            .Select(registro => registro.TarjetaId)
            .ToListAsync(cancellationToken))
            .ToHashSet();
        if (tarjetasActivas.Contains(tarjeta.Id))
        {
            throw new TarjetaEnUsoException();
        }

        var configuracion = await context.ConfiguracionParqueadero
            .AsNoTracking()
            .SingleAsync(cancellationToken);

        if (tipoParqueadero == TipoParqueadero.ArenaUSC && !configuracion.ArenaUSCAbierta)
        {
            throw new ParqueaderoLlenoException("El parqueadero de la arena USC está cerrado.");
        }

        var cupo = tipoParqueadero switch
        {
            TipoParqueadero.Estudiantes => configuracion.CupoEstudiantes,
            TipoParqueadero.Docentes => configuracion.CupoDocentes,
            TipoParqueadero.Egresados => configuracion.CupoEgresados,
            TipoParqueadero.ArenaUSC => configuracion.CupoArenaUSC,
            TipoParqueadero.Bloque6 => configuracion.CupoBloque6,
            TipoParqueadero.AtrasCanchas => configuracion.CupoAtrasCanchas,
            TipoParqueadero.Bloque8 => configuracion.CupoBloque8,
            TipoParqueadero.Plazoleta => configuracion.CupoPlazoleta,
            _ => configuracion.CupoTotal
        };
        var ocupados = await context.RegistrosAcceso
            .CountAsync(
                registro => registro.HoraSalida == null &&
                    registro.TipoParqueadero == tipoParqueadero,
                cancellationToken);
        if (ocupados >= cupo)
        {
            throw new ParqueaderoLlenoException();
        }

        var registro = new RegistroAcceso
        {
            TarjetaId = tarjeta.Id,
            TipoParqueadero = tipoParqueadero,
            HoraEntrada = DateTime.Now
        };
        context.RegistrosAcceso.Add(registro);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return registro;
        }
        catch (DbUpdateException exception) when (EsIndiceTarjetaActiva(exception))
        {
            throw new TarjetaEnUsoException();
        }
    }

    public async Task<RegistroAcceso> RegistrarSalidaAsync(
        string codigoTarjeta,
        CancellationToken cancellationToken = default)
    {
        var codigo = NormalizarCodigo(codigoTarjeta);
        var tarjeta = await context.Tarjetas
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Codigo == codigo, cancellationToken);

        if (tarjeta is null)
        {
            throw new TarjetaInactivaException("La tarjeta no existe.");
        }

        if (tarjeta.Estado != EstadoTarjeta.Activa)
        {
            throw new TarjetaInactivaException();
        }

        var registrosActivos = await context.RegistrosAcceso
            .Include(item => item.Tarjeta)
            .Where(item => item.HoraSalida == null)
            .ToListAsync(cancellationToken);
        var registrosPorTarjeta = registrosActivos.ToDictionary(
            item => item.Tarjeta!.Codigo,
            StringComparer.OrdinalIgnoreCase);

        if (!registrosPorTarjeta.TryGetValue(codigo, out var registro))
        {
            throw new EntradaNoActivaException();
        }

        var ahora = DateTime.Now;
        registro.HoraSalida = ahora;
        registro.DuracionMinutos = Math.Max(0, (int)(ahora - registro.HoraEntrada).TotalMinutes);
        await context.SaveChangesAsync(cancellationToken);
        return registro;
    }

    public async Task<ReservaParqueadero> SolicitarReservaAsync(
        string usuarioId,
        DateTime fechaHoraDeseada,
        int duracionHoras,
        CancellationToken cancellationToken = default)
    {
        if (fechaHoraDeseada < DateTime.Now.AddMinutes(10))
        {
            throw new FechaReservaInvalidaException();
        }

        if (duracionHoras is < 1 or > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(duracionHoras), "La duración debe estar entre 1 y 12 horas.");
        }

        var finEstimado = fechaHoraDeseada.AddHours(duracionHoras);
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        var reservasConfirmadas = await context.ReservasParqueadero
            .AsNoTracking()
            .Where(reserva => reserva.Estado == EstadoReserva.Confirmada)
            .ToListAsync(cancellationToken);

        static bool SeSolapan(DateTime inicioA, DateTime finA, DateTime inicioB, DateTime finB) =>
            inicioA < finB && inicioB < finA;

        var reservasSolapadas = reservasConfirmadas
            .Where(reserva => SeSolapan(
                fechaHoraDeseada,
                finEstimado,
                reserva.FechaHoraDeseada,
                reserva.FinEstimado))
            .ToList();

        if (reservasSolapadas.Any(reserva => reserva.UsuarioId == usuarioId))
        {
            throw new ReservaSolapadaException();
        }

        var configuracion = await context.ConfiguracionParqueadero
            .AsNoTracking()
            .SingleAsync(cancellationToken);

        var tarjetasDentro = (await context.RegistrosAcceso
            .Where(registro => registro.HoraSalida == null)
            .Select(registro => registro.TarjetaId)
            .ToListAsync(cancellationToken))
            .ToHashSet();
        var limiteReservasProximas = DateTime.Now.AddHours(1);
        var reservasQueOcuparanCupo = reservasSolapadas.Count(
            reserva => !tarjetasDentro.Contains(reserva.TarjetaId));
        if (fechaHoraDeseada <= limiteReservasProximas &&
            tarjetasDentro.Count + reservasQueOcuparanCupo >= configuracion.CupoTotal)
        {
            throw new CupoReservaNoDisponibleException();
        }

        var tarjetasReservadas = reservasSolapadas
            .Select(reserva => reserva.TarjetaId)
            .ToHashSet();
        var tarjeta = await context.Tarjetas
            .Where(item => item.Estado == EstadoTarjeta.Activa)
            .OrderBy(item => item.Codigo)
            .FirstOrDefaultAsync(
                item => !tarjetasDentro.Contains(item.Id) && !tarjetasReservadas.Contains(item.Id),
                cancellationToken);

        if (tarjeta is null)
        {
            throw new SinTarjetasDisponiblesException();
        }

        var reservaNueva = new ReservaParqueadero
        {
            CodigoTicket = $"USC-R{Guid.NewGuid():N}"[..17].ToUpperInvariant(),
            UsuarioId = usuarioId,
            TarjetaId = tarjeta.Id,
            FechaHoraDeseada = fechaHoraDeseada,
            DuracionHoras = duracionHoras
        };
        context.ReservasParqueadero.Add(reservaNueva);
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        reservaNueva.Tarjeta = tarjeta;
        return reservaNueva;
    }

    private static string NormalizarCodigo(string codigo) => codigo.Trim().ToUpperInvariant();

    private static bool EsIndiceTarjetaActiva(DbUpdateException exception) =>
        exception.InnerException is SqliteException sqliteException &&
        sqliteException.SqliteErrorCode == 19 &&
        sqliteException.Message.Contains("RegistrosAcceso.TarjetaId", StringComparison.OrdinalIgnoreCase);
}
