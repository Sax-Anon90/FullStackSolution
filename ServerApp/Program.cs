using ServerApp;

var builder = WebApplication.CreateBuilder(args);

// Activity 2: register CORS services as well as the middleware.
// Restrict access to our client rather than allowing every website.
var clientOrigin = builder.Configuration["ClientOrigin"] ?? "http://localhost:5100";
builder.Services.AddCors(options => options.AddPolicy("BlazorClient", policy =>
    policy.WithOrigins(clientOrigin)
          .AllowAnyHeader()
          .AllowAnyMethod()
          .WithExposedHeaders("X-Generated-At")));

// Activity 4: cache the entire successful response for 30 seconds.
builder.Services.AddOutputCache(options => options.AddPolicy("Products", policy =>
    policy.Expire(TimeSpan.FromSeconds(30)).SetVaryByHeader("Origin")));

var app = builder.Build();
app.UseCors("BlazorClient");
app.UseOutputCache();

app.MapGet("/", () => Results.Ok(new { application = "InventoryHub API", products = "/api/productlist" }));

// Activities 2 and 3: the corrected route returns a JSON array, with nested categories.
// ASP.NET Core serializes the public properties as camelCase JSON automatically.
app.MapGet("/api/productlist", (HttpContext context, ILogger<Program> logger) =>
{
    // This header stays identical on a cache hit, making caching easy to verify.
    context.Response.Headers["X-Generated-At"] = DateTimeOffset.UtcNow.ToString("O");
    logger.LogInformation("Generating product response (output cache miss).");
    return Results.Ok(ProductData.GetProducts());
}).CacheOutput("Products");

app.Run();
