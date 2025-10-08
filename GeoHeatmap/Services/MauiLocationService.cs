namespace GeoHeatmap;

public class MauiLocationService : ILocationService
{
    public async Task<bool> EnsurePermissionAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }
        return status == PermissionStatus.Granted;
    }

    public async Task<Location?> GetLocationAsync(CancellationToken ct)
    {
        var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));
        try
        {
            return await Geolocation.Default.GetLocationAsync(request, ct);
        }
        catch (Exception)
        {
            return null;
        }
    }
}