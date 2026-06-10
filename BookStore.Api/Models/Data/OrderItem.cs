namespace BookStore.Models.Data;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int BookId { get; set; }
    public Book? Book { get; set; }
    public int Quantity { get; set; }

    /// <summary>Price frozen at the time the order was placed.</summary>
    public decimal UnitPrice { get; set; }

    public decimal LineTotal => UnitPrice * Quantity;
}
