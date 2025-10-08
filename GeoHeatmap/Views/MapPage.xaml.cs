using GeoHeatmap.Models;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace GeoHeatmap;

public partial class MapPage : ContentPage
{
    private readonly MapViewModel _vm;
    private readonly HeatmapService _heat;

    public MapPage(MapViewModel vm, HeatmapService heat)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
        _heat = heat;

        _vm.SamplesChanged += () => HeatCanvas.InvalidateSurface();

        // Redraw when the map region changes (pan/zoom)
        TheMap.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(Microsoft.Maui.Controls.Maps.Map.VisibleRegion))
                HeatCanvas.InvalidateSurface();
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.InitializeAsync();
        await CenterOnUserAsync();
    }

    private async Task CenterOnUserAsync()
    {
        try
        {
            var loc = await Geolocation.Default.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(8)));
            if (loc != null)
            {
                var position = new Location(loc.Latitude, loc.Longitude);
                TheMap.MoveToRegion(MapSpan.FromCenterAndRadius(position, Distance.FromKilometers(1.2)));
            }
        }
        catch { /* ignore for simulator */ }
    }

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var info = e.Info;
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        var span = TheMap.VisibleRegion;
        if (span == null) return;

        _heat.DrawHeat(canvas, info, _vm.CachedSamples, span!, radiusPx: 26f, blurPx: 20f);
    }
}
