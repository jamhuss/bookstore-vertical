using BookStore.Models.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

using Order = BookStore.Models.Data.Order;

namespace BookStore.Endpoints.Orders;

[HttpPost("api/orders/{OrderId:int}/books")]
[AllowAnonymous]
public class AddBookToOrder
    : Endpoint<AddBookToOrder.AddBookRequest, AddBookToOrder.OrderDto, AddBookToOrder.OrderMapper>
{
    private readonly AppDbContext _db;

    public AddBookToOrder(AppDbContext db) => _db = db;

    public override async Task HandleAsync(AddBookRequest request, CancellationToken ct = default)
    {
        var quantity = request.Quantity < 1 ? 1 : request.Quantity;

        var order = await _db.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Book)
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
                Book = book,
                Quantity = quantity,
                UnitPrice = book.Price
            });
        }

        order.TotalPrice = order.Items.Sum(i => i.UnitPrice * i.Quantity);

        await _db.SaveChangesAsync(ct);

        var dto = Map.FromEntity(order);

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

    public class OrderMapper : Mapper<AddBookRequest, OrderDto, Order>
    {
        public override OrderDto FromEntity(Order e) => new(
            e.Id,
            e.UserEmail,
            e.TotalPrice,
            e.Items.Select(i => new OrderItemDto(
                i.BookId,
                i.Book!.Title,
                i.Quantity,
                i.UnitPrice,
                i.LineTotal)).ToList());
    }
}
