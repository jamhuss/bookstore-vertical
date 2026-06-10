namespace BookStore.Models.Data;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Author { get; set; } = null!;
    public string Genre { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Price { get; set; }
}
