using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.FileProviders;
using ValentynaWeb.Content.Models;

namespace ValentynaWeb.Content;

public class JsonContentProvider : IContentProvider, IDisposable
{
    private readonly PhysicalFileProvider _fileProvider;
    private readonly string _contentRoot;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly ConcurrentDictionary<string, object> _cache = new();

    public JsonContentProvider(IWebHostEnvironment env)
    {
        _contentRoot = Path.Combine(env.ContentRootPath, "Content");
        _fileProvider = new PhysicalFileProvider(_contentRoot);

        Watch<SalonContent>("salon.json");
        Watch<ServicesContent>("services.json");
        Watch<MastersContent>("masters.json");
        Watch<ReviewsContent>("reviews.json");
        Watch<GalleryContent>("gallery.json");
        Watch<NewsContent>("news.json");
    }

    private void Watch<T>(string fileName) where T : new()
    {
        Load<T>(fileName);
        RegisterWatch<T>(fileName);
    }

    private void RegisterWatch<T>(string fileName) where T : new()
    {
        _fileProvider.Watch(fileName).RegisterChangeCallback(_ =>
        {
            Load<T>(fileName);
            RegisterWatch<T>(fileName);
        }, null);
    }

    private void Load<T>(string key) where T : new()
    {
        var path = Path.Combine(_contentRoot, key);
        try
        {
            _cache[key] = File.Exists(path)
                ? JsonSerializer.Deserialize<T>(File.ReadAllText(path), _jsonOptions) ?? new T()
                : new T();
        }
        catch
        {
            _cache.TryAdd(key, new T());
        }
    }

    private T Get<T>(string key) where T : new() =>
        _cache.TryGetValue(key, out var val) && val is T typed ? typed : new T();

    public SalonContent Salon => Get<SalonContent>("salon.json");
    public ServicesContent Services => Get<ServicesContent>("services.json");
    public MastersContent Masters => Get<MastersContent>("masters.json");
    public ReviewsContent Reviews => Get<ReviewsContent>("reviews.json");
    public GalleryContent Gallery => Get<GalleryContent>("gallery.json");
    public NewsContent News => Get<NewsContent>("news.json");

    public void Dispose() => _fileProvider.Dispose();
}
