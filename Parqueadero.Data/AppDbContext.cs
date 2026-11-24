using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Parqueadero.Core.Entities;
using Parqueadero.Data.Identity;

namespace Parqueadero.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<AplicacionUser>(options)
{
    public DbSet<Tarjeta> Tarjetas => Set<Tarjeta>();
    public DbSet<RegistroAcceso> RegistrosAcceso => Set<RegistroAcceso>();
    public DbSet<ConfiguracionParqueadero> ConfiguracionParqueadero => Set<ConfiguracionParqueadero>();
    public DbSet<ReservaParqueadero> ReservasParqueadero => Set<ReservaParqueadero>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Tarjeta>()
            .HasIndex(tarjeta => tarjeta.Codigo)
            .IsUnique();

        builder.Entity<Tarjeta>()
            .Property(tarjeta => tarjeta.Estado)
            .HasConversion<string>();

        builder.Entity<RegistroAcceso>()
            .HasIndex(registro => registro.TarjetaId)
            .IsUnique()
            .HasFilter("\"HoraSalida\" IS NULL");

        builder.Entity<RegistroAcceso>()
            .Property(registro => registro.TipoParqueadero)
            .HasConversion<string>();

        builder.Entity<RegistroAcceso>()
            .HasOne(registro => registro.Tarjeta)
            .WithMany(tarjeta => tarjeta.Registros)
            .HasForeignKey(registro => registro.TarjetaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ConfiguracionParqueadero>().HasData(
            new ConfiguracionParqueadero
            {
                Id = 1,
                CupoTotal = 60,
                CupoEstudiantes = 30,
                CupoDocentes = 15,
                CupoEgresados = 10,
                CupoArenaUSC = 5,
                CupoBloque6 = 20,
                CupoAtrasCanchas = 15,
                CupoBloque8 = 15,
                CupoPlazoleta = 10,
                ArenaUSCAbierta = false
            });

        builder.Entity<ReservaParqueadero>()
            .Property(reserva => reserva.Estado)
            .HasConversion<string>();

        builder.Entity<ReservaParqueadero>()
            .HasIndex(reserva => reserva.CodigoTicket)
            .IsUnique();

        builder.Entity<ReservaParqueadero>()
            .HasIndex(reserva => new { reserva.UsuarioId, reserva.FechaHoraDeseada, reserva.Estado });

        builder.Entity<ReservaParqueadero>()
            .HasOne(reserva => reserva.Tarjeta)
            .WithMany()
            .HasForeignKey(reserva => reserva.TarjetaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ReservaParqueadero>()
            .HasOne<AplicacionUser>()
            .WithMany()
            .HasForeignKey(reserva => reserva.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
