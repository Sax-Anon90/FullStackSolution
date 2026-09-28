# InventoryHub

A small, readable Blazor WebAssembly + ASP.NET Core Minimal API project for **Course 7: Full Stack Integration**, Microsoft Full Stack Developer Professional Certificate.

The final application combines the four supplied activities. It displays inventory from `/api/productlist`, handles integration failures, uses nested JSON categories, and reduces redundant work through client and server caching.

## Run it (start here)

Prerequisites: the **.NET 10 SDK** (not just the runtime), a browser, and an internet connection for the first NuGet restore. Use VS Code with C# tooling, or a Visual Studio version that supports .NET 10.

1. Extract the ZIP. Open a terminal in the folder containing `FullStackSolution.sln`.
2. Check your SDK and build:

```shell
dotnet --version
dotnet restore FullStackSolution.sln
dotnet build FullStackSolution.sln
```

3. In terminal 1, start the API:

```shell
dotnet run --project ServerApp --launch-profile http
```

4. In terminal 2, start the client:

```shell
dotnet run --project ClientApp --launch-profile http
```

5. Open **http://localhost:5100**. The API is **http://localhost:5200/api/productlist**. Leave both terminals running. Stop them with Ctrl+C.

In Visual Studio, open the solution and configure multiple startup projects: `ServerApp` and `ClientApp`, using the `http` launch profile. `Shared` is a class library; `InventoryHub.Checks` is a separate verification utility.

Local HTTP avoids certificate setup for this exercise. Production deployments need HTTPS and appropriate environment configuration.

## What is included?

- Eight sample products, including the original Laptop and Headphones.
- Summary cards, a responsive product table and expandable details.
- Local search, category filters and stock filters.
- Loading, empty, error, retry and refresh states.
- Typed nested category JSON and decimal prices.
- 60-second client cache; 30-second API output cache.
- Dependency-free service checks, a live API check script and GitHub Actions.

Prices use USD to match the assignment. Low stock means 1–10 units; out of stock means zero. The “In stock” filter selects the healthy-stock status (more than 10 units). Data is read-only and in memory. The supplied activities do not require database storage, editing, authentication, EF Core or migrations.

## Understand the code in this order

| File | What it does |
|---|---|
| `Shared/Product.cs` | Defines Product and Category, the JSON contract shared by client and API. |
| `ServerApp/ProductData.cs` | Supplies the sample inventory. |
| `ServerApp/Program.cs` | Registers CORS/output caching and maps the product endpoint. |
| `ClientApp/Program.cs` | Registers HttpClient and ProductService. |
| `ClientApp/Services/ProductService.cs` | Fetches, validates and caches products. |
| `ClientApp/Pages/FetchProducts.razor` | Loads products, catches expected errors and renders the interface. |
| `ClientApp/wwwroot/css/app.css` | Styles the responsive interface without a UI library. |

The flow is: page → service → HttpClient → API → JSON → shared models → page. No repositories, CQRS, MediatR or mapping frameworks are needed here.

## Configuration

| Setting | File | Default |
|---|---|---|
| API address used by the browser | `ClientApp/wwwroot/appsettings.json` | `http://localhost:5200/` |
| Allowed browser origin | `ServerApp/appsettings.json` | `http://localhost:5100` |
| Client listening address | `ClientApp/Properties/launchSettings.json` | `http://localhost:5100` |
| API listening address | `ServerApp/Properties/launchSettings.json` | `http://localhost:5200` |

Keep the four settings consistent if you change ports. Use `localhost` consistently; `127.0.0.1` is a different origin. An origin has scheme, host and port, but no trailing slash. Never store secrets in the client's public configuration.

## How the optimizations work

**Client:** ProductService lives for the lifetime of this WebAssembly app in a browser tab. A successful fetch is reused for 60 seconds when requested again. Navigating to About and back within that period makes no extra API request. Search and filters always use the loaded array. A semaphore coalesces overlapping normal loads, and the page disables repeat refresh clicks while loading. Refresh explicitly bypasses the client cache. Failed responses never replace successful cached data.

**Server:** ASP.NET Core output caching stores the full successful response for 30 seconds, skipping endpoint execution and serialization on cache hits. The cache varies by Origin so different CORS responses remain separate. `X-Generated-At` stays the same on a cache hit. A log message appears only when the endpoint executes. The response may still be server-cached after a client Refresh.

Neither cache polls automatically. Expiry is checked on the next load. This is suitable for the read-only sample; a real writable inventory would need cache invalidation and an explicit freshness policy. Timing gains on eight in-memory products may be tiny. Do not claim measured improvements without recording your results.

## Check the project

```shell
dotnet run --project InventoryHub.Checks
```

With ServerApp running, and Python 3 installed:

```shell
python scripts/check_api.py
python scripts/check_api.py --expiry
```

On macOS/Linux use `python3` if required. The second command also verifies expiration and takes at least 31 seconds. See `docs/TESTING.md` for browser/error/performance checks. `ServerApp/ServerApp.http` can be opened in Visual Studio or VS Code with a REST Client extension, or reproduced in Postman.

**Verification status of this generated delivery:** static source/configuration/archive checks were performed. The generation environment had no .NET SDK and could not reach SDK/NuGet downloads, so compilation, execution, browser rendering and .NET/API checks have **not** been verified here. Run the commands above and confirm a green GitHub Actions run before submitting. The workflow builds, runs the service checks, publishes the client and tests the live API.
