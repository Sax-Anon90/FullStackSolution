using Shared;

namespace ServerApp;

public static class ProductData
{
    // In-memory sample data is enough for this integration assignment.
    // A database and CRUD endpoints are intentionally outside the supplied scope.
    public static Product[] GetProducts()
    {
        return new Product[]
        {
        new Product
        {
            Id = 1,
            Name = "Laptop",
            Price = 1200.50m,
            Stock = 25,
            Category = new Category { Id = 101, Name = "Electronics" }
        },
        new Product
        {
            Id = 2,
            Name = "Headphones",
            Price = 50.00m,
            Stock = 100,
            Category = new Category { Id = 102, Name = "Accessories" }
        },
        new Product
        {
            Id = 3,
            Name = "Wireless Mouse",
            Price = 24.99m,
            Stock = 8,
            Category = new Category { Id = 102, Name = "Accessories" }
        },
        new Product
        {
            Id = 4,
            Name = "27-inch Monitor",
            Price = 249.00m,
            Stock = 16,
            Category = new Category { Id = 101, Name = "Electronics" }
        },
        new Product
        {
            Id = 5,
            Name = "Mechanical Keyboard",
            Price = 89.50m,
            Stock = 0,
            Category = new Category { Id = 102, Name = "Accessories" }
        },
        new Product
        {
            Id = 6,
            Name = "Desk Lamp",
            Price = 35.00m,
            Stock = 7,
            Category = new Category { Id = 103, Name = "Office" }
        },
        new Product
        {
            Id = 7,
            Name = "USB-C Dock",
            Price = 119.00m,
            Stock = 18,
            Category = new Category { Id = 101, Name = "Electronics" }
        },
        new Product
        {
            Id = 8,
            Name = "Laptop Stand",
            Price = 45.00m,
            Stock = 32,
            Category = new Category { Id = 103, Name = "Office" }
        }
        };
    }
}
