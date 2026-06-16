using Ardalis.ApiEndpoints;
using BookStore.Helpers;
using BookStore.Models.Data;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Endpoints.Orders;

public class GetAllOrders
    : EndpointBaseAsync
        .WithRequest<ListRequest>
        .WithActionResult<PagedResult<GetAllOrdersResponse>>
{
    private readonly AppDbContext _db;

    public GetAllOrders(AppDbContext db) => _db = db;

    [HttpGet("api/orders")]
    public override async Task<ActionResult<PagedResult<GetAllOrdersResponse>>> HandleAsync(
        [FromQuery] ListRequest request,
        CancellationToken ct = default)
    {
        var query = _db.Orders
            .OrderByDescending(o => o.Id)
            .Select(o => new GetAllOrdersResponse(
                o.Id,
                o.UserEmail,
                o.TotalPrice,
                o.Items.Select(i => new GetAllOrdersResponseItem(
                    i.BookId,
                    i.Book.Title,
                    i.Quantity,
                    i.UnitPrice,
                    i.LineTotal
                )).ToList()
            ));

        var result = await query.ToPagedResultAsync(request.Page, request.PageSize, ct);

        return Ok(result);
    }
}

public record GetAllOrdersResponse(
    int Id,
    string UserEmail,
    decimal TotalPrice,
    List<GetAllOrdersResponseItem> Items
);

public record GetAllOrdersResponseItem(
    int BookId,
    string Title,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal
);

