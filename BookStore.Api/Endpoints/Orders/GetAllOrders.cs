using Ardalis.ApiEndpoints;
using BookStore.Helpers;
using BookStore.Models.Data;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Endpoints.Orders;

public class GetAllOrders
    : EndpointBaseAsync
        .WithRequest<ListRequest>
        .WithActionResult<PagedResult<GetAllOrders.OrderDto>>
{
    private readonly AppDbContext _db;

    public GetAllOrders(AppDbContext db) => _db = db;

    [HttpGet("api/orders")]
    public override async Task<ActionResult<PagedResult<OrderDto>>> HandleAsync(
        [FromQuery] ListRequest request,
        CancellationToken ct = default)
    {
        var query = _db.Orders
            .OrderByDescending(o => o.Id)
            .Select(o => new OrderDto(
                o.Id,
                o.UserEmail,
                o.TotalPrice,
                o.Items.Select(i => new OrderItemDto(
                    i.BookId,
                    i.Book!.Title,
                    i.Quantity,
                    i.UnitPrice,
                    i.UnitPrice * i.Quantity)).ToList()));

        var result = await query.ToPagedResultAsync(request.Page, request.PageSize, ct);

        return Ok(result);
    }

    public record OrderDto(
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
}
