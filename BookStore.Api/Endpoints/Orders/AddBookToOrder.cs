using BookStore.Models.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Endpoints.Orders;

[HttpPost("api/orders/{OrderId:int}/books")]
[AllowAnonymous]
public class AddBookToOrder
    : Endpoint<AddBookToOrder.AddBookRequest, AddBookToOrder.OrderDto>
{
    private readonly AppDbContext _db;

    public AddBookToOrder(AppDbContext db) => _db = db;

    public override async Task HandleAsync(AddBookRequest request, CancellationToken ct = default)
    {
        var quantity = request.Quantity < 1 ? 1 : request.Quantity;

        var order = await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, ct);

        if (order is null)
        {
            await Send.ResultAsync(Results.NotFound($"Order {request.OrderId} was not found."));
            return;
        }

        var book = await _db.Books.FirstOrDefaultAsync(b => b.Id == request.BookId, ct);
        if (book is null)
        {
            await Send.ResultAsync(Results.BadRequest($"Book {request.BookId} does not exist."));
            return;
        }

        var existing = order.Items.FirstOrDefault(i => i.BookId == request.BookId);
        if (existing is not null)
        {
            existing.Quantity += quantity;
        }
        else
        {
            order.Items.Add(new OrderItem
            {
                OrderId = order.Id,
                BookId = book.Id,
                Quantity = quantity,
                UnitPrice = book.Price
            });
        }

        order.TotalPrice = order.Items.Sum(i => i.UnitPrice * i.Quantity);

        await _db.SaveChangesAsync(ct);

        var titles = await _db.Books
            .Where(b => order.Items.Select(i => i.BookId).Contains(b.Id))
            .ToDictionaryAsync(b => b.Id, b => b.Title, ct);

        var dto = new OrderDto(
            order.Id,
            order.UserEmail,
            order.TotalPrice,
            order.Items.Select(i => new OrderItemDto(
                i.BookId,
                titles[i.BookId],
                i.Quantity,
                i.UnitPrice,
                i.UnitPrice * i.Quantity)).ToList());

        await Send.OkAsync(dto, cancellation: ct);
    }

    public record AddBookRequest(int OrderId, int BookId, int Quantity);

    public record OrderDto(
        int Id,
        string UserEmail,
        decimal TotalPrice,
        List<OrderItemDto> Items);

    public record OrderItemDto(
        int BookId,
        string Title,
        int Quantity,
        decimal UnitPrice,
        decimal LineTotal);
}
