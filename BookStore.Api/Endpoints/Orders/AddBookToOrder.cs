using Ardalis.ApiEndpoints;
using BookStore.Helpers;
using BookStore.Models.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Endpoints.Orders;

/// <summary>
/// Demonstrates the FromMultiSource helper: the request record is bound from BOTH
/// the route (OrderId) and the query string (BookId, Quantity) at the same time,
/// working around the Ardalis "single request object" limitation.
/// </summary>
public class AddBookToOrder
    : EndpointBaseAsync
        .WithRequest<AddBookToOrder.AddBookRequest>
        .WithActionResult<AddBookToOrder.OrderDto>
{
    private readonly AppDbContext _db;

    public AddBookToOrder(AppDbContext db) => _db = db;

    [HttpPost("api/orders/{OrderId:int}/books")]
    public override async Task<ActionResult<OrderDto>> HandleAsync(
        [FromMultiSource] AddBookRequest request,
        CancellationToken ct = default)
    {
        var quantity = request.Quantity < 1 ? 1 : request.Quantity;

        var order = await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, ct);

        if (order is null)
        {
            return NotFound($"Order {request.OrderId} was not found.");
        }

        var book = await _db.Books.FirstOrDefaultAsync(b => b.Id == request.BookId, ct);
        if (book is null)
        {
            return BadRequest($"Book {request.BookId} does not exist.");
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

        return Ok(dto);
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
