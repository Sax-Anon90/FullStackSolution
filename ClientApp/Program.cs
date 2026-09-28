using ClientApp;
using ClientApp.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Point HttpClient at the API, not the client application's own address.
var apiUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5200/";
builder.Services.AddScoped(_ => new HttpClient
{
    BaseAddress = new Uri(apiUrl),
    Timeout = TimeSpan.FromSeconds(10)
});
builder.Services.AddScoped<ProductService>();
await builder.Build().RunAsync();
