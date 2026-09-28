"""Run against ServerApp: python scripts/check_api.py [--expiry]. Standard library only."""
import json
import sys
import time
from urllib.request import Request, urlopen
from urllib.error import HTTPError, URLError

BASE = "http://localhost:5200"
ORIGIN = "http://localhost:5100"

def get(path="/api/productlist", origin=ORIGIN):
    start = time.perf_counter()
    with urlopen(Request(BASE + path, headers={"Origin": origin}), timeout=10) as response:
        return json.load(response), response.headers, (time.perf_counter() - start) * 1000

# CI may launch the API just before running this script.
for attempt in range(30):
    try:
        with urlopen(BASE + "/", timeout=1):
            break
    except (URLError, TimeoutError):
        time.sleep(1)
else:
    raise SystemExit("ServerApp is not available on http://localhost:5200. Start it first.")

products, headers, first_ms = get()
assert len(products) == 8, "Expected eight demo products"
assert headers.get_content_type() == "application/json"
assert headers["Access-Control-Allow-Origin"] == ORIGIN
for product in products:
    assert set(product) == {"id", "name", "price", "stock", "category"}
    assert set(product["category"]) == {"id", "name"}
    assert isinstance(product["stock"], int) and product["stock"] >= 0
assert products[0]["name"] == "Laptop" and products[0]["price"] == 1200.5
assert products[1]["name"] == "Headphones" and products[1]["category"]["id"] == 102
print("PASS: JSON schema, nested categories, sample values and allowed CORS origin")

cached, cached_headers, cached_ms = get()
assert cached == products
assert headers["X-Generated-At"] == cached_headers["X-Generated-At"], "Response was not reused"
print("PASS: Repeated request reused the server output cache")
print(f"First measured request: {first_ms:.2f} ms; next: {cached_ms:.2f} ms")
print("Timing is observational, not proof of a speedup. Restart the server for a cold first request.")

_, denied_headers, _ = get(origin="http://untrusted.example")
assert denied_headers.get("Access-Control-Allow-Origin") is None
# Check origin variation did not poison a previously cached allowed response.
_, allowed_headers, _ = get()
assert allowed_headers["Access-Control-Allow-Origin"] == ORIGIN
print("PASS: Disallowed origin has no browser access grant; allowed origin still works")

request = Request(BASE + "/api/productlist", method="OPTIONS", headers={
    "Origin": ORIGIN, "Access-Control-Request-Method": "GET",
    "Access-Control-Request-Headers": "content-type"})
with urlopen(request) as response:
    assert response.headers["Access-Control-Allow-Origin"] == ORIGIN
    assert "GET" in response.headers["Access-Control-Allow-Methods"]
print("PASS: CORS preflight")
try:
    get("/api/products")
except HTTPError as error:
    assert error.code == 404
else:
    raise AssertionError("Old API route should return 404")
print("PASS: Old route returns 404")

if "--expiry" in sys.argv:
    print("Waiting 31 seconds to verify server cache expiration…")
    time.sleep(31)
    _, expired_headers, _ = get()
    assert expired_headers["X-Generated-At"] != headers["X-Generated-At"]
    print("PASS: Server output cache expires")
print("All API checks passed.")
