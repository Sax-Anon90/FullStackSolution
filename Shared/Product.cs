using System.Text.Json.Serialization;

namespace Shared;

// Shared by the API and client so their JSON contracts cannot drift apart.
public class Product
{
    [JsonRequired] public int Id { get; set; }
    [JsonRequired] public string Name { get; set; } = string.Empty;
    [JsonRequired] public decimal Price { get; set; }
    [JsonRequired] public int Stock { get; set; }
    [JsonRequired] public Category Category { get; set; } = new();
}

public class Category
{
    [JsonRequired] public int Id { get; set; }
    [JsonRequired] public string Name { get; set; } = string.Empty;
}
