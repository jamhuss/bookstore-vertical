using Ardalis.ApiEndpoints;
using BookStore.Models.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Endpoints.Orders;

public class GetOrderById
    : EndpointBaseAsync
        .WithRequest<int>
        .WithActionResult<GetOrderByIdResponse>
{
    private readonly AppDbContext _db;

    public GetOrderById(AppDbContext db) => _db = db;

    [HttpGet("api/orders/{id:int}")]
    public override async Task<ActionResult<GetOrderByIdResponse>> HandleAsync(
        int id,
        CancellationToken ct = default)
    {
        var order = await _db.Orders
            .Where(o => o.Id == id)
            .Select(o => new GetOrderByIdResponse(
                o.Id,
                o.UserEmail,
                o.TotalPrice,
                o.Items.Select(i => new OrderItemDto(
                    i.BookId,
                    i.Book!.Title,
                    i.Quantity,
                    i.UnitPrice,
                    i.UnitPrice * i.Quantity)).ToList()))
            .FirstOrDefaultAsync(ct);

        if (order is null)
        {
            return NotFound();
        }

        return Ok(order);
    }
}
public record GetOrderByIdResponse(
    int Id,
    string UserEmail,
    decimal TotalPrice,
    List<OrderItemDto> Items
);

public record OrderItemDto(
    int BookId,
    string Title,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal
);


