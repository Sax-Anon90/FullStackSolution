using System.Net;
using System.Text;
using System.Text.Json;
using ClientApp.Services;

const string validJson = """
[{"id":1,"name":"Laptop","price":1200.50,"stock":25,"category":{"id":101,"name":"Electronics"}}]
""";
var passed = 0;

await Check("Nested camelCase JSON and decimal prices", async () =>
{
    var (service, _) = Create(validJson);
    var products = await service.GetProductsAsync();
    Require(products.Length == 1 && products[0].Category.Name == "Electronics"
        && products[0].Price == 1200.50m, "Product contract was not deserialized correctly.");
});
await Check("Correct route and cached repeat calls", async () =>
{
    var (service, handler) = Create(validJson);
    await service.GetProductsAsync();
    await service.GetProductsAsync();
    Require(handler.Calls == 1, "A cached load made a redundant request.");
    Require(handler.LastPath == "/api/productlist", "The API route is incorrect.");
});
await Check("Concurrent loads share one successful fetch", async () =>
{
    var (service, handler) = Create(validJson);
    await Task.WhenAll(Enumerable.Range(0, 5).Select(_ => service.GetProductsAsync()));
    Require(handler.Calls == 1, "Concurrent loads made redundant requests.");
});
await Check("Refresh bypasses the client cache", async () =>
{
    var (service, handler) = Create(validJson);
    await service.GetProductsAsync();
    await service.GetProductsAsync(forceRefresh: true);
    Require(handler.Calls == 2, "Refresh did not request fresh data.");
});
await Check("Empty arrays are valid", async () =>
{
    var (service, _) = Create("[]");
    Require((await service.GetProductsAsync()).Length == 0, "Empty inventory should be supported.");
});
foreach (var badJson in new[] { "not JSON", "{}", "null", "[{}]",
    validJson.Replace("1200.50", "-1"), validJson.Replace("25", "-2"),
    validJson.Replace("{\"id\":101,\"name\":\"Electronics\"}", "null"),
    validJson.Replace("\"price\":1200.50", "\"price\":\"abc\"") })
{
    await Check($"Invalid response rejected: {badJson[..Math.Min(45, badJson.Length)]}", async () =>
    {
        var (service, _) = Create(badJson);
        await Expect<JsonException>(async () => { await service.GetProductsAsync(); });
    });
}
await Check("Duplicate product IDs rejected", async () =>
{
    var item = validJson[1..^1];
    var (service, _) = Create($"[{item},{item}]");
    await Expect<JsonException>(async () => { await service.GetProductsAsync(); });
});
foreach (var status in new[] { HttpStatusCode.NotFound, HttpStatusCode.InternalServerError })
{
    await Check($"HTTP {(int)status} is surfaced", async () =>
    {
        var (service, handler) = Create(validJson);
        handler.Status = status;
        try { await service.GetProductsAsync(); throw new Exception("Expected an HTTP error."); }
        catch (HttpRequestException ex) { Require(ex.StatusCode == status, "HTTP status was lost."); }
    });
}
await Check("Timeout is surfaced", async () =>
{
    var (service, handler) = Create(validJson);
    handler.Error = new TaskCanceledException("Simulated timeout");
    await Expect<OperationCanceledException>(async () => { await service.GetProductsAsync(); });
});
await Check("Network failure is surfaced", async () =>
{
    var (service, handler) = Create(validJson);
    handler.Error = new HttpRequestException("Simulated network failure");
    await Expect<HttpRequestException>(async () => { await service.GetProductsAsync(); });
});
await Check("Failed requests are not cached and can be retried", async () =>
{
    var (service, handler) = Create("broken");
    await Expect<JsonException>(async () => { await service.GetProductsAsync(); });
    handler.Json = validJson;
    var products = await service.GetProductsAsync();
    Require(handler.Calls == 2 && products.Length == 1, "Failed request prevented recovery.");
});
await Check("Failed refresh preserves the last successful cache", async () =>
{
    var (service, handler) = Create(validJson);
    await service.GetProductsAsync();
    handler.Json = "broken";
    await Expect<JsonException>(async () => { await service.GetProductsAsync(true); });
    Require((await service.GetProductsAsync()).Length == 1 && handler.Calls == 2,
        "Failed refresh damaged the successful cache.");
});
Console.WriteLine($"All {passed} service checks passed.");

async Task Check(string name, Func<Task> test)
{
    await test();
    passed++;
    Console.WriteLine($"PASS: {name}");
}
static void Require(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}
static async Task Expect<T>(Func<Task> action) where T : Exception
{
    try { await action(); }
    catch (T) { return; }
    throw new Exception($"Expected {typeof(T).Name}.");
}
static (ProductService Service, FakeHandler Handler) Create(string json)
{
    var handler = new FakeHandler { Json = json };
    return (new ProductService(new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5200/") }), handler);
}

class FakeHandler : HttpMessageHandler
{
    public string Json { get; set; } = "[]";
    public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;
    public Exception? Error { get; set; }
    public int Calls { get; private set; }
    public string? LastPath { get; private set; }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Calls++;
        LastPath = request.RequestUri?.AbsolutePath;
        // Yield so concurrent-call coverage actually overlaps requests.
        await Task.Delay(10, cancellationToken);
        if (Error is not null) throw Error;
        return new HttpResponseMessage(Status)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        };
    }
}
