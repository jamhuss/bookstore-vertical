namespace BookStore.Models.Data;

public class Order
{
    public int Id { get; set; }
    public string UserEmail { get; set; } = null!;
    public decimal TotalPrice { get; set; }
    public List<OrderItem> Items { get; set; } = new();
}
