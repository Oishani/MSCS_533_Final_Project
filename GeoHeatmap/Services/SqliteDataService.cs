using GeoHeatmap.Models;
using SQLite;

namespace GeoHeatmap;

public class SqliteDataService : IDataService
{
    private const string DbName = "geo.db3";
    private SQLiteAsyncConnection? _conn;
    private bool _initialized;

    public async Task InitAsync()
    {
        if (_initialized) return;
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, DbName);
        _conn = new SQLiteAsyncConnection(dbPath);
        await _conn.CreateTableAsync<LocationSample>();
        _initialized = true;
    }

    public async Task<int> InsertAsync(LocationSample sample)
    {
        if (!_initialized) await InitAsync();
        return await _conn!.InsertAsync(sample);
    }

    public async Task<List<LocationSample>> GetAllAsync()
    {
        if (!_initialized) await InitAsync();
        return await _conn!.Table<LocationSample>().OrderBy(s => s.TimestampUtc).ToListAsync();
    }

    public async Task<List<LocationSample>> GetSinceAsync(DateTime sinceUtc)
    {
        if (!_initialized) await InitAsync();
        return await _conn!.Table<LocationSample>()
            .Where(s => s.TimestampUtc >= sinceUtc)
            .OrderBy(s => s.TimestampUtc).ToListAsync();
    }

    public async Task<int> DeleteAllAsync()
    {
        if (!_initialized) await InitAsync();
        return await _conn!.DeleteAllAsync<LocationSample>();
    }
}