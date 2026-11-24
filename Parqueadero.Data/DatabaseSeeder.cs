using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Parqueadero.Core.Entities;
using Parqueadero.Core.Enums;
using Parqueadero.Data.Identity;

namespace Parqueadero.Data;

public static class DatabaseSeeder
{
    public static readonly string[] Roles =
    [
        "Administrador",
        "Empleado",
        "Estudiante",
        "Docente",
        "Egresado"
    ];

    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(role));
                EnsureSucceeded(result, $"No fue posible crear el rol {role}.");
            }
        }

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AplicacionUser>>();
        await CreateUserIfMissing(userManager, "admin@usc.edu.co", "Administración USC", "Administrador", "Admin123!");
        await CreateUserIfMissing(userManager, "empleado@usc.edu.co", "Operador de acceso", "Empleado", "Empleado123!");
        await CreateUserIfMissing(userManager, "estudiante@usc.edu.co", "Estudiante de prueba", "Estudiante", "Estudiante123!");
        await CreateUserIfMissing(userManager, "docente@usc.edu.co", "Docente de prueba", "Docente", "Docente123!");
        await CreateUserIfMissing(userManager, "egresado@usc.edu.co", "Egresado de prueba", "Egresado", "Egresado123!");

        if (!await context.Tarjetas.AnyAsync())
        {
            var tarjetas = Enumerable.Range(1, 10)
                .Select(numero => new Tarjeta
                {
                    Codigo = $"USC-{numero:000}",
                    Estado = numero == 10 ? EstadoTarjeta.Inactiva : EstadoTarjeta.Activa
                });
            context.Tarjetas.AddRange(tarjetas);
            await context.SaveChangesAsync();
        }

        if (!await context.RegistrosAcceso.AnyAsync())
        {
            var tarjeta = await context.Tarjetas.FirstAsync(item => item.Codigo == "USC-001");
            context.RegistrosAcceso.Add(new RegistroAcceso
            {
                TarjetaId = tarjeta.Id,
                HoraEntrada = DateTime.Now.AddHours(-2.5),
                HoraSalida = DateTime.Now.AddHours(-1.5),
                DuracionMinutos = 60
            });
            await context.SaveChangesAsync();
        }
    }

    private static async Task CreateUserIfMissing(
        UserManager<AplicacionUser> userManager,
        string email,
        string nombre,
        string role,
        string password)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new AplicacionUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                NombreCompleto = nombre
            };

            var createResult = await userManager.CreateAsync(user, password);
            EnsureSucceeded(createResult, $"No fue posible crear la cuenta {email}.");
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            var roleResult = await userManager.AddToRoleAsync(user, role);
            EnsureSucceeded(roleResult, $"No fue posible asignar el rol {role} a {email}.");
        }
    }

    private static void EnsureSucceeded(IdentityResult result, string message)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"{message} {string.Join("; ", result.Errors.Select(error => error.Description))}");
        }
    }
}
