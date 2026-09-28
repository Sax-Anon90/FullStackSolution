using System.Net.Http.Json;
using System.Text.Json;
using Shared;

namespace ClientApp.Services;

public class ProductService(HttpClient httpClient)
{
    private Product[]? cachedProducts;
    private DateTimeOffset cacheExpiresAt;
    private readonly SemaphoreSlim requestLock = new(1, 1);

    public DateTimeOffset? LastFetchedAt { get; private set; }

    public async Task<Product[]> GetProductsAsync(bool forceRefresh = false)
    {
        // Activity 4: preserve data across page navigation for one minute.
        // A lock prevents simultaneous normal loads from making duplicate requests.
        await requestLock.WaitAsync();
        try
        {
            if (!forceRefresh && cachedProducts is not null && DateTimeOffset.UtcNow < cacheExpiresAt)
                return cachedProducts;

            // Activity 2: use the updated route; non-success status codes must not look like empty data.
            using var response = await httpClient.GetAsync("api/productlist");
            response.EnsureSuccessStatusCode();

            // ReadFromJsonAsync uses web defaults, including case-insensitive property matching.
            // Missing fields, invalid numbers, invalid syntax and wrong root shapes raise JsonException.
            var products = await response.Content.ReadFromJsonAsync<Product[]>()
                ?? throw new JsonException("The API returned null instead of a product array.");

            // Valid JSON can still contain invalid product data (e.g. a null category).
            if (products.Any(p => p is null || p.Id <= 0 || string.IsNullOrWhiteSpace(p.Name)
                || p.Price < 0 || p.Stock < 0 || p.Category is null
                || p.Category.Id <= 0 || string.IsNullOrWhiteSpace(p.Category.Name))
                || products.Select(p => p.Id).Distinct().Count() != products.Length)
                throw new JsonException("The API returned invalid product fields or duplicate IDs.");

            // Only successful, validated responses enter the cache.
            cachedProducts = products;
            LastFetchedAt = DateTimeOffset.UtcNow;
            cacheExpiresAt = LastFetchedAt.Value.AddMinutes(1);
            return products;
        }
        finally
        {
            requestLock.Release();
        }
    }
}
