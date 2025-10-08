using GeoHeatmap.Models;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps;
using SkiaSharp;

namespace GeoHeatmap;

public class HeatmapService
{
    // Projects lat/long to screen coordinates using an equirectangular approximation
    // good enough for small visible regions.
    public (float x, float y) ProjectToScreen(Location topLeft, Location bottomRight, double lat, double lon, int width, int height)
    {
        var lonSpan = bottomRight.Longitude - topLeft.Longitude;
        var latSpan = topLeft.Latitude - bottomRight.Latitude;
        if (lonSpan == 0 || latSpan == 0) return (0, 0);

        var x = (float)((lon - topLeft.Longitude) / lonSpan * width);
        var y = (float)((topLeft.Latitude - lat) / latSpan * height);
        return (x, y);
    }

    public void DrawHeat(SKCanvas canvas, SKImageInfo info, IEnumerable<LocationSample> samples,
                         Microsoft.Maui.Maps.MapSpan visibleSpan, float radiusPx = 36f, float blurPx = 24f)
    {
        var mapSpan = visibleSpan;
        var halfLat = mapSpan.LatitudeDegrees / 2.0;
        var halfLon = mapSpan.LongitudeDegrees / 2.0;
        var topLeft = new Location(mapSpan.Center.Latitude + halfLat, mapSpan.Center.Longitude - halfLon);
        var bottomRight = new Location(mapSpan.Center.Latitude - halfLat, mapSpan.Center.Longitude + halfLon);

        // --- heat accumulation layer ---
        using var heatPaint = new SKPaint
        {
            IsAntialias = true,
            ImageFilter = SKImageFilter.CreateBlur(blurPx, blurPx),
            BlendMode = SKBlendMode.Plus
        };

        foreach (var s in samples)
        {
            // Skip if outside current bounds for perf
            if (s.Latitude > topLeft.Latitude || s.Latitude < bottomRight.Latitude) continue;
            if (s.Longitude < topLeft.Longitude || s.Longitude > bottomRight.Longitude) continue;

            var (x, y) = ProjectToScreen(topLeft, bottomRight, s.Latitude, s.Longitude, info.Width, info.Height);
            heatPaint.Color = new SKColor(255, 0, 0, 96); // visible heat
            canvas.DrawCircle(x, y, radiusPx, heatPaint);
        }

        using var dotPaint = new SKPaint { IsAntialias = true, Color = new SKColor(33, 150, 243, 230) }; // blue 600-ish
        const float dotRadius = 4.5f;
        foreach (var s in samples)
        {
            if (s.Latitude > topLeft.Latitude || s.Latitude < bottomRight.Latitude) continue;
            if (s.Longitude < topLeft.Longitude || s.Longitude > bottomRight.Longitude) continue;

            var (x, y) = ProjectToScreen(topLeft, bottomRight, s.Latitude, s.Longitude, info.Width, info.Height);
            canvas.DrawCircle(x, y, dotRadius, dotPaint);
        }
    }
}