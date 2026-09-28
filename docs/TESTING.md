# Verification and performance worksheet

## Automated checks

1. `dotnet build FullStackSolution.sln`
2. `dotnet run --project InventoryHub.Checks`
3. Start ServerApp; run `python scripts/check_api.py --expiry`.
4. `dotnet publish ClientApp -c Release -o artifacts/client`

The service checks compile the actual ProductService source, use a fake HttpMessageHandler and require no external test packages. They test nested data, decimals, route, cache hits, refresh, concurrent requests, empty arrays, invalid responses, HTTP/network/timeout failures and recovery. They are a console runner: use `dotnet run`, not `dotnet test`.

The Python script tests the live API's schema, sample values, CORS headers/preflight, cache reuse, the old route's 404 and optional cache expiration. A CORS header test is useful but does not replace checking in a browser.

## Browser acceptance checks

- [ ] Start both apps and open http://localhost:5100. Confirm eight products.
- [ ] Confirm 206 units, USD 43,023.42 inventory value and three products needing attention.
- [ ] Search “Laptop”: two products. Search “no-such-product”: empty-result state. Clear filters.
- [ ] Choose Accessories: three products. Choose Out of stock: Mechanical Keyboard only (clear category first).
- [ ] View a product, check nested category and stock value, then close details.
- [ ] Navigate About → Products. Check navigation, content and request count.
- [ ] Stop ServerApp and click Refresh. Confirm a readable connectivity error and retained previous data.
- [ ] Restart ServerApp and click Try again. Confirm successful recovery.
- [ ] At a narrow browser width, check stacked filters/cards and horizontal table scrolling.
- [ ] Use Tab/Enter to navigate links, filters, View, Refresh and Retry.
- [ ] Check browser Console for unexpected exceptions and CORS failures.

To inspect the initial-loading state, hard-reload with browser Network throttling. To test empty API data, temporarily return `Array.Empty<Product>()` from the endpoint, restart the API and hard-reload the client. Revert afterwards.

For malformed JSON UI handling, temporarily change the endpoint return to `Results.Text("not JSON", "application/json")`, restart and Refresh. Confirm the JSON-specific error, then revert and restart. For a 404, temporarily change the service path to `api/products`, rebuild/restart the client, check the message and revert. The automated service checks simulate timeouts and HTTP failures without requiring you to wait or alter source.

## Performance measurement

### Client request reduction

1. Open browser DevTools → Network. Filter to `productlist`, clear the log, then hard-reload.
2. Expect one product request. Type in Search and change filters: expect zero additional requests.
3. Go to About and return within 60 seconds: expect zero additional requests.
4. Click Refresh: expect one additional request (client cache bypassed).
5. Wait over 60 seconds from the last successful fetch, then navigate away and back: expect one additional request. Merely waiting on the page does not trigger polling.
6. For a baseline, temporarily disable the cache-hit `if` block in ProductService, rebuild/restart, and repeat three About → Products navigations. Expect three additional loads in that baseline versus zero with the cache within its TTL. Revert the change.

### Server cache

1. Restart the API for a cold cache. Run `python scripts/check_api.py --expiry` or send repeated identical requests from ServerApp.http.
2. Compare `X-Generated-At`: identical within 30 seconds, new after expiration. Observe the “Generating product response” server log only on misses.
3. Record cold/warm request durations. Use several runs and report medians; do not include SDK startup/build time. Maintain the same Origin for comparable cache keys.
4. For a disabled-cache baseline, temporarily remove `.CacheOutput("Products")` from the endpoint, restart and repeat. The header changes each request and the endpoint executes each time. Restore caching after measuring.

Small in-memory data may not show a meaningful latency reduction; avoided API requests and skipped endpoint executions are stronger evidence. Do not invent a percentage improvement.

| Measurement | Baseline | Optimized | Conditions / observations |
|---|---|---|---|
| Requests after three return navigations | [measure] | [measure] | Within 60 seconds |
| Requests while typing/filtering | [measure] | [measure] | Same actions |
| Endpoint executions across repeated requests | [measure] | [measure] | Same Origin, within 30 seconds |
| Median response duration (ms) | [measure] | [measure] | Number of runs: [fill] |

## Delivery verification status

Static checks only were possible in the generation environment. No .NET SDK was installed and downloads were unreachable. Automated .NET/API checks and browser checks above remain to be run on your machine or CI. Record the real results before submission.
