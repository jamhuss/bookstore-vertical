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
            .Select(o => new GetAllOrdersResponse
            {
                Id = o.Id,
                UserEmail = o.UserEmail,
                TotalPrice = o.TotalPrice,
                Items = o.Items.Select(i => new OrderItemDto
                {
                    BookId = i.BookId,
                    Title = i.Book!.Title,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    LineTotal = i.UnitPrice * i.Quantity
                }).ToList()
            });

        var result = await query.ToPagedResultAsync(request.Page, request.PageSize, ct);

        return Ok(result);
    }
}

public class GetAllOrdersResponse
{
    public int Id { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; } = 0;
    public List<OrderItemDto> Items { get; set; } = new();
}

public class OrderItemDto
{
    public int BookId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Quantity { get; set; } = 0;
    public decimal UnitPrice { get; set; } = 0;
    public decimal LineTotal { get; set; } = 0;
}