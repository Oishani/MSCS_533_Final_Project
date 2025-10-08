using CommunityToolkit.Mvvm;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Controls.Maps;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace GeoHeatmap;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiMaps() // Map control
            .UseSkiaSharp() // SKCanvasView
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // DI registrations
        builder.Services.AddSingleton<IDataService, SqliteDataService>();
        builder.Services.AddSingleton<ILocationService, MauiLocationService>();
        builder.Services.AddSingleton<HeatmapService>();
        builder.Services.AddSingleton<MapViewModel>();
        builder.Services.AddSingleton<MapPage>();

        return builder.Build();
    }
}