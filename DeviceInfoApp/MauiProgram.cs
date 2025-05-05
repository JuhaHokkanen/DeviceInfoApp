using DeviceInfoApp.Services;
using DeviceInfoApp;
using Microcharts.Maui;


public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMicrocharts();

                return builder.Build();
    }
}
