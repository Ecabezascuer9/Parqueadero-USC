using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Parqueadero.Core.Interfaces;
using Parqueadero.Data;
using Parqueadero.Data.Identity;
using Parqueadero.Data.Services;
using System.Windows;

namespace Parqueadero.Desktop;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(SharedDatabase.GetConnectionString()));
            serviceCollection
                .AddIdentityCore<AplicacionUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>();
            serviceCollection.AddScoped<IParqueaderoService, ParqueaderoService>();
            Services = serviceCollection.BuildServiceProvider();
            await DatabaseSeeder.InitializeAsync(Services);
            var window = new MainWindow();
            MainWindow = window;
            window.Show();
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                $"No fue posible iniciar la base de datos del parqueadero.\n\n{exception.Message}",
                "Error de inicio",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (Services is IDisposable disposable)
        {
            disposable.Dispose();
        }

        base.OnExit(e);
    }
}
