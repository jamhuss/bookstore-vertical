namespace BookStore.Models.Data;

/// <summary>
/// Idempotent startup seeding for the in-memory store so the API has data to work
/// with immediately after cloning and running.
/// </summary>
public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (db.Books.Any())
        {
            return;
        }

        var books = new List<Book>
        {
            new() { Title = "The Pragmatic Programmer", Author = "Andrew Hunt", Genre = "Tech", Description = "Your journey to mastery in software craftsmanship.", Price = 349m },
            new() { Title = "Clean Architecture", Author = "Robert C. Martin", Genre = "Tech", Description = "A craftsman's guide to software structure and design.", Price = 399m },
            new() { Title = "Dune", Author = "Frank Herbert", Genre = "Sci-Fi", Description = "Epic tale of politics, religion and survival on Arrakis.", Price = 199m },
            new() { Title = "The Hobbit", Author = "J.R.R. Tolkien", Genre = "Fantasy", Description = "Bilbo Baggins' unexpected journey to the Lonely Mountain.", Price = 159m },
            new() { Title = "Project Hail Mary", Author = "Andy Weir", Genre = "Sci-Fi", Description = "A lone astronaut must save humanity from extinction.", Price = 229m },
            new() { Title = "Pride and Prejudice", Author = "Jane Austen", Genre = "Fiction", Description = "A classic story of love, reputation and class.", Price = 129m },
            new() { Title = "Sapiens", Author = "Yuval Noah Harari", Genre = "Non-Fiction", Description = "A brief history of humankind.", Price = 249m },
            new() { Title = "The Name of the Wind", Author = "Patrick Rothfuss", Genre = "Fantasy", Description = "The legend of Kvothe, told in his own words.", Price = 189m }
        };

        db.Books.AddRange(books);
        db.SaveChanges();

        var orders = new List<Order>
        {
            new()
            {
                UserEmail = "anna@example.com",
                Items = new List<OrderItem>
                {
                    new() { BookId = books[0].Id, Quantity = 1, UnitPrice = books[0].Price },
                    new() { BookId = books[2].Id, Quantity = 2, UnitPrice = books[2].Price }
                }
            },
            new()
            {
                UserEmail = "erik@example.com",
                Items = new List<OrderItem>
                {
                    new() { BookId = books[3].Id, Quantity = 1, UnitPrice = books[3].Price },
                    new() { BookId = books[6].Id, Quantity = 1, UnitPrice = books[6].Price },
                    new() { BookId = books[7].Id, Quantity = 3, UnitPrice = books[7].Price }
                }
            }
        };

        foreach (var order in orders)
        {
            order.TotalPrice = order.Items.Sum(i => i.UnitPrice * i.Quantity);
        }

        db.Orders.AddRange(orders);
        db.SaveChanges();
    }
}
