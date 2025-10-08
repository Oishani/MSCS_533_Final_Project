using GeoHeatmap.Models;

namespace GeoHeatmap;

public interface IDataService
{
    Task InitAsync();
    Task<int> InsertAsync(LocationSample sample);
    Task<List<LocationSample>> GetAllAsync();
    Task<List<LocationSample>> GetSinceAsync(DateTime sinceUtc);
    Task<int> DeleteAllAsync();
}