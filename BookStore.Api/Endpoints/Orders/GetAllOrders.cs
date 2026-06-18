using BookStore.Helpers;
using BookStore.Models.Data;
using Microsoft.AspNetCore.Authorization;

namespace BookStore.Endpoints.Orders;

[HttpGet("api/orders")]
[AllowAnonymous]
public class GetAllOrders
    : Endpoint<ListRequest, PagedResult<GetAllOrdersResponse>>
{
    private readonly AppDbContext _db;

    public GetAllOrders(AppDbContext db) => _db = db;

    public override async Task HandleAsync(ListRequest request, CancellationToken ct = default)
    {
        var query = _db.Orders
            .OrderByDescending(o => o.Id)
            .Select(o => new GetAllOrdersResponse(
                o.Id,
                o.UserEmail,
                o.TotalPrice,
                o.Items.Select(i => new GetAllOrdersResponseItem(
                    i.BookId,
                    i.Book!.Title,
                    i.Quantity,
                    i.UnitPrice,
                    i.LineTotal
                )).ToList()
            ));

        var result = await query.ToPagedResultAsync(request.Page, request.PageSize, ct);

        await Send.OkAsync(result, cancellation: ct);
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

