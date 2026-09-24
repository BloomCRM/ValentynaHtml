using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using ValentynaWeb.Content.Models;

namespace ValentynaWeb.Content;

public class GooglePlacesOptions
{
    public string? ApiKey { get; set; }
    public string? PlaceId { get; set; }
    public int CacheHours { get; set; } = 6;
    /// <summary>Показуємо лише відгуки з оцінкою від цього значення (рішення салону — 4–5★).</summary>
    public int MinRating { get; set; } = 4;
}

/// <summary>
/// Відгуки з Google Places API (New) — тимчасово, до схвалення Business Profile API.
/// Без ApiKey/PlaceId або при помилці API — відгуки з reviews.json.
/// </summary>
public class PlacesReviewsProvider : IReviewsProvider
{
    public const string HttpClientName = "GooglePlaces";
    private const string CacheKey = "reviews:places";
    private static readonly TimeSpan FailureCacheDuration = TimeSpan.FromMinutes(10);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;
    private readonly IOptionsMonitor<GooglePlacesOptions> _options;
    private readonly IContentProvider _content;
    private readonly ILogger<PlacesReviewsProvider> _logger;
    private readonly SemaphoreSlim _fetchLock = new(1, 1);

    public PlacesReviewsProvider(
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache,
        IOptionsMonitor<GooglePlacesOptions> options,
        IContentProvider content,
        ILogger<PlacesReviewsProvider> logger)
    {
        _httpClientFactory = httpClientFactory;
        _cache = cache;
        _options = options;
        _content = content;
        _logger = logger;
    }

    public async Task<ReviewsContent> GetAsync()
    {
        var o = _options.CurrentValue;
        if (string.IsNullOrWhiteSpace(o.ApiKey) || string.IsNullOrWhiteSpace(o.PlaceId))
            return _content.Reviews;

        if (_cache.TryGetValue(CacheKey, out ReviewsContent? cached) && cached != null)
            return cached;

        // Один запит до Google на всіх, навіть якщо кеш протух під навантаженням
        await _fetchLock.WaitAsync();
        try
        {
            if (_cache.TryGetValue(CacheKey, out cached) && cached != null)
                return cached;

            try
            {
                var fetched = await FetchAsync(o);
                _cache.Set(CacheKey, fetched, TimeSpan.FromHours(o.CacheHours));
                return fetched;
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
            {
                _logger.LogWarning(ex, "Google Places reviews fetch failed, falling back to reviews.json");
                _cache.Set(CacheKey, _content.Reviews, FailureCacheDuration);
                return _content.Reviews;
            }
        }
        finally
        {
            _fetchLock.Release();
        }
    }

    private async Task<ReviewsContent> FetchAsync(GooglePlacesOptions o)
    {
        // Google не перекладає відгуки, а за languageCode віддає інші — написані цією мовою.
        // Тому UA-версія сайту показує українські відгуки, EN — англійські.
        var ukTask = FetchPlaceAsync(o, "uk");
        var enTask = FetchPlaceAsync(o, "en");
        var uk = await ukTask;
        var en = await enTask;

        var ukReviews = ToReviews(uk, "ua", o.MinRating);
        var ukNames = (uk.Reviews ?? []).Select(r => r.Name).ToHashSet();
        var enReviews = ToReviews(en with { Reviews = en.Reviews?.Where(r => !ukNames.Contains(r.Name)).ToList() }, "en", o.MinRating);

        List<Review> reviews = (ukReviews.Count, enReviews.Count) switch
        {
            (0, 0) => _content.Reviews.Reviews,
            (_, 0) => ukReviews.Select(r => { r.Language = null; return r; }).ToList(),
            (0, _) => enReviews.Select(r => { r.Language = null; return r; }).ToList(),
            _ => [.. ukReviews, .. enReviews]
        };

        return new ReviewsContent
        {
            Rating = uk.Rating,
            TotalCount = uk.UserRatingCount,
            ProfileUrl = uk.GoogleMapsUri ?? _content.Reviews.ProfileUrl,
            Reviews = reviews
        };
    }

    private static List<Review> ToReviews(PlaceDto place, string language, int minRating) =>
        (place.Reviews ?? [])
            .Where(r => r.Rating >= minRating)
            .Select(r => new Review
            {
                AuthorName = r.AuthorAttribution?.DisplayName ?? "",
                AuthorUrl = r.AuthorAttribution?.Uri,
                AvatarUrl = r.AuthorAttribution?.PhotoUri ?? "",
                Rating = r.Rating,
                Text = r.OriginalText?.Text ?? r.Text?.Text ?? "",
                Language = language,
                RelativeDate = r.RelativePublishTimeDescription,
                PublishedAt = r.PublishTime,
                GoogleUrl = r.GoogleMapsUri
            })
            .Where(r => !string.IsNullOrWhiteSpace(r.Text))
            .ToList();

    private async Task<PlaceDto> FetchPlaceAsync(GooglePlacesOptions o, string languageCode)
    {
        var client = _httpClientFactory.CreateClient(HttpClientName);
        using var request = new HttpRequestMessage(HttpMethod.Get,
            $"places/{Uri.EscapeDataString(o.PlaceId!)}?languageCode={languageCode}");
        request.Headers.Add("X-Goog-Api-Key", o.ApiKey);
        request.Headers.Add("X-Goog-FieldMask", "rating,userRatingCount,googleMapsUri,reviews");

        using var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Places API {(int)response.StatusCode}: {body}");
        }

        await using var stream = await response.Content.ReadAsStreamAsync();
        return await JsonSerializer.DeserializeAsync<PlaceDto>(stream)
            ?? throw new JsonException("Empty Places API response");
    }

    private sealed record PlaceDto(
        [property: JsonPropertyName("rating")] double? Rating,
        [property: JsonPropertyName("userRatingCount")] int? UserRatingCount,
        [property: JsonPropertyName("googleMapsUri")] string? GoogleMapsUri,
        [property: JsonPropertyName("reviews")] List<PlaceReviewDto>? Reviews);

    private sealed record PlaceReviewDto(
        [property: JsonPropertyName("name")] string? Name,
        [property: JsonPropertyName("rating")] int Rating,
        [property: JsonPropertyName("publishTime")] DateTimeOffset? PublishTime,
        [property: JsonPropertyName("relativePublishTimeDescription")] string? RelativePublishTimeDescription,
        [property: JsonPropertyName("text")] LocalizedTextDto? Text,
        [property: JsonPropertyName("originalText")] LocalizedTextDto? OriginalText,
        [property: JsonPropertyName("authorAttribution")] AuthorAttributionDto? AuthorAttribution,
        [property: JsonPropertyName("googleMapsUri")] string? GoogleMapsUri);

    private sealed record LocalizedTextDto(
        [property: JsonPropertyName("text")] string? Text,
        [property: JsonPropertyName("languageCode")] string? LanguageCode);

    private sealed record AuthorAttributionDto(
        [property: JsonPropertyName("displayName")] string? DisplayName,
        [property: JsonPropertyName("uri")] string? Uri,
        [property: JsonPropertyName("photoUri")] string? PhotoUri);
}
