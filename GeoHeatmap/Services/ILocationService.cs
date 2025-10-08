namespace GeoHeatmap;

public interface ILocationService
{
    Task<bool> EnsurePermissionAsync();
    Task<Location?> GetLocationAsync(CancellationToken ct);
}

