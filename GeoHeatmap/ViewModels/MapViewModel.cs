using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GeoHeatmap.Models;

namespace GeoHeatmap;

public partial class MapViewModel : ObservableObject
{
    private readonly IDataService _data;
    private readonly ILocationService _loc;

    private CancellationTokenSource? _cts;

    [ObservableProperty] private bool isTracking;
    [ObservableProperty] private int samplesCount;
    [ObservableProperty] private DateTime? lastFixTimeUtc;

    public event Action? SamplesChanged; // notify UI to redraw

    public List<LocationSample> CachedSamples { get; private set; } = new();

    public MapViewModel(IDataService data, ILocationService loc)
    {
        _data = data; _loc = loc;
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        await _data.InitAsync();
        CachedSamples = await _data.GetAllAsync();
        SamplesCount = CachedSamples.Count;
        SamplesChanged?.Invoke();
    }

    [RelayCommand]
    public async Task ToggleTrackingAsync()
    {
        if (!IsTracking)
        {
            if (!await _loc.EnsurePermissionAsync())
            {
                var page = Application.Current?.MainPage;
                if (page != null)
                    await page.DisplayAlert("Permission", "Location permission is required.", "OK");
                return;
            }
            IsTracking = true;
            _cts = new CancellationTokenSource();
            _ = Task.Run(() => TrackLoopAsync(_cts.Token));
        }
        else
        {
            IsTracking = false;
            _cts?.Cancel();
        }
    }

    private async Task TrackLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var fix = await _loc.GetLocationAsync(ct);
            if (fix != null)
            {
                var sample = new LocationSample
                {
                    Latitude = fix.Latitude,
                    Longitude = fix.Longitude,
                    AccuracyMeters = fix.Accuracy,
                    TimestampUtc = DateTime.UtcNow
                };
                await _data.InsertAsync(sample);
                CachedSamples.Add(sample);
                SamplesCount = CachedSamples.Count;
                LastFixTimeUtc = sample.TimestampUtc;
                MainThread.BeginInvokeOnMainThread(() => SamplesChanged?.Invoke());
            }
            await Task.Delay(TimeSpan.FromSeconds(5), ct);
        }
    }

    [RelayCommand]
    public async Task RefreshHeatmapAsync()
    {
        CachedSamples = await _data.GetAllAsync();
        SamplesCount = CachedSamples.Count;
        SamplesChanged?.Invoke();
    }

    [RelayCommand]
    public async Task ClearDataAsync()
    {
        await _data.DeleteAllAsync();
        CachedSamples.Clear();
        SamplesCount = 0;
        SamplesChanged?.Invoke();
    }
}