using Microsoft.Extensions.DependencyInjection;
using Parqueadero.Core.Exceptions;
using Parqueadero.Core.Interfaces;
using Parqueadero.Data;
using System.Windows;

namespace Parqueadero.Desktop;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += async (_, _) => await RefreshOccupancyAsync();
    }

    private async void RegisterEntry_Click(object sender, RoutedEventArgs e) =>
        await RegisterAsync(isEntry: true);

    private async void RegisterExit_Click(object sender, RoutedEventArgs e) =>
        await RegisterAsync(isEntry: false);

    private async void Refresh_Click(object sender, RoutedEventArgs e) =>
        await RefreshOccupancyAsync();

    private async Task RegisterAsync(bool isEntry)
    {
        var code = CardCode.Text.Trim();
        if (string.IsNullOrWhiteSpace(code))
        {
            ResultMessage.Text = "Ingresa el código de la tarjeta.";
            ResultMessage.Foreground = System.Windows.Media.Brushes.Firebrick;
            return;
        }

        try
        {
            using var scope = App.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IParqueaderoService>();
            var registro = isEntry
                ? await service.RegistrarEntradaAsync(code)
                : await service.RegistrarSalidaAsync(code);
            ResultMessage.Text = isEntry
                ? $"Entrada registrada: {code.ToUpperInvariant()} · {registro.HoraEntrada:HH:mm:ss}"
                : $"Salida registrada: {code.ToUpperInvariant()} · {registro.DuracionMinutos} minutos";
            ResultMessage.Foreground = System.Windows.Media.Brushes.SeaGreen;
            CardCode.Clear();
            await RefreshOccupancyAsync();
        }
        catch (Exception exception) when (exception is ParqueaderoLlenoException
            or TarjetaEnUsoException
            or TarjetaInactivaException
            or EntradaNoActivaException)
        {
            ResultMessage.Text = exception.Message;
            ResultMessage.Foreground = System.Windows.Media.Brushes.Firebrick;
        }
        catch (Exception exception)
        {
            MessageBox.Show(exception.Message, "No fue posible registrar el movimiento", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task RefreshOccupancyAsync()
    {
        try
        {
            using var scope = App.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IParqueaderoService>();
            var aforo = await service.ObtenerAforoAsync();
            Availability.Text = $"{aforo.Disponibles} / {aforo.CupoTotal}";
            OccupancyDetails.Text = $"{aforo.Ocupados} ocupados · {aforo.PorcentajeOcupacion:0}% de ocupación · {aforo.Estado}";
            HeaderStatus.Text = aforo.Estado.ToUpperInvariant();
        }
        catch (Exception exception)
        {
            MessageBox.Show(exception.Message, "No fue posible consultar el aforo", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
