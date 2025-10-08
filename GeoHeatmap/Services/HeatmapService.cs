using GeoHeatmap.Models;
using Microsoft.Maui.Controls.Maps;
using SkiaSharp;

namespace GeoHeatmap;

public class HeatmapService
{
    // Projects lat/long to screen coordinates using an equirectangular approximation
    // good enough for small visible regions.
    public (float x, float y) ProjectToScreen(Position topLeft, Position bottomRight, double lat, double lon, int width, int height)
    {
        var lonSpan = bottomRight.Longitude - topLeft.Longitude;
        var latSpan = topLeft.Latitude - bottomRight.Latitude;
        if (lonSpan == 0 || latSpan == 0) return (0, 0);

        var x = (float)((lon - topLeft.Longitude) / lonSpan * width);
        var y = (float)((topLeft.Latitude - lat) / latSpan * height);
        return (x, y);
    }

    public void DrawHeat(SKCanvas canvas, SKImageInfo info, IEnumerable<LocationSample> samples,
                         MapSpan? visibleSpan, float radiusPx = 24f, float blurPx = 18f)
    {
        if (visibleSpan == null) return;
        var mapSpan = visibleSpan.Value;
        var halfLat = mapSpan.LatitudeDegrees / 2.0;
        var halfLon = mapSpan.LongitudeDegrees / 2.0;
        var topLeft = new Position(mapSpan.Center.Latitude + halfLat, mapSpan.Center.Longitude - halfLon);
        var bottomRight = new Position(mapSpan.Center.Latitude - halfLat, mapSpan.Center.Longitude + halfLon);

        using var blurPaint = new SKPaint
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
            // low alpha; accumulation = intensity
            blurPaint.Color = new SKColor(255, 0, 0, 18);
            canvas.DrawCircle(x, y, radiusPx, blurPaint);
        }
    }
}