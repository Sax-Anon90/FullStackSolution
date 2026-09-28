# Activity walkthrough

## Activity 1 — connect the client and API

`ClientApp/Program.cs` registers an HttpClient with the API's base URL. `FetchProducts.razor` calls ProductService inside `OnInitializedAsync`, awaits the result and renders a loading state while waiting. The service checks HTTP status and deserializes the JSON into shared Product objects. The page distinguishes timeout, connectivity, HTTP and JSON errors and offers retry.

The supplied starter's broken foreach syntax is corrected. Prices use decimal instead of double because they represent money. The final solution uses the updated route from Activity 2, not the original Activity 1 route.

## Activity 2 — resolve integration issues

| Issue | Final correction | How to check |
|---|---|---|
| Wrong endpoint | Both ends use `/api/productlist`. | Inspect Network requests; old `/api/products` returns 404. |
| CORS | Register AddCors; run UseCors before output caching; allow the exact client origin. | Browser can display results; inspect allow-origin header. |
| JSON syntax or structure | Web JSON deserialization, JsonRequired fields and basic value checks. | Service checks reject invalid JSON, missing fields, null category and duplicate IDs. |
| JSON casing | ReadFromJsonAsync uses web defaults; server emits camelCase. | Nested categories display correctly. |
| API failures | EnsureSuccessStatusCode and specific catches. | Stop API and click Refresh; restore and Retry. |

The activity shows AllowAnyOrigin as a quick demo. This version uses a named policy limited to the configured Blazor origin. It also includes AddCors, which must accompany middleware registration. CORS does not authenticate callers or block non-browser clients.

## Activity 3 — standard JSON

The response root is an array (no wrapper object). Each item contains id, name, price, stock, and category; category contains id and name. The original two products retain their IDs, prices, stock and categories. Six additional samples demonstrate filtering and stock states.

Example first item:

```json
{
  "id": 1,
  "name": "Laptop",
  "price": 1200.50,
  "stock": 25,
  "category": { "id": 101, "name": "Electronics" }
}
```

JSON numeric values do not preserve display formatting; 50 and 50.00 represent the same value. The client formats prices to two decimals.

## Activity 4 — optimize and consolidate

The client reuses validated results for one minute and filters locally. The API uses 30-second output caching. Comments document why each is present. The tests cover cached repeat calls, concurrent loads, refresh, malformed responses and recovery. Use TESTING.md to gather actual browser request counts and server-cache evidence, then complete REFLECTION.md after your Copilot review.
