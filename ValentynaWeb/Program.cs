using ValentynaWeb.Content;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddHealthChecks();
builder.Services.AddOutputCache(o => o.AddBasePolicy(b => b.Expire(TimeSpan.FromMinutes(5))));
builder.Services.AddLocalization(o => o.ResourcesPath = "Resources");
builder.Services.AddSingleton<IContentProvider, JsonContentProvider>();

builder.Services.AddMemoryCache();
builder.Services.Configure<GooglePlacesOptions>(builder.Configuration.GetSection("GooglePlaces"));
builder.Services.AddHttpClient(PlacesReviewsProvider.HttpClientName, c =>
{
    c.BaseAddress = new Uri("https://places.googleapis.com/v1/");
    c.Timeout = TimeSpan.FromSeconds(5);
});
builder.Services.AddSingleton<IReviewsProvider, PlacesReviewsProvider>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseOutputCache();
app.UseAuthorization();
app.MapRazorPages();
app.MapHealthChecks("/health");

app.Run();
