using BookStore.Models.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Endpoints.Orders;

[HttpGet("api/orders/{id:int}")]
[AllowAnonymous]
public class GetOrderById
    : Endpoint<GetOrderByIdRequest, GetOrderByIdResponse>
{
    private readonly AppDbContext _db;

    public GetOrderById(AppDbContext db) => _db = db;

    public override async Task HandleAsync(GetOrderByIdRequest request, CancellationToken ct = default)
    {
        var order = await _db.Orders
            .Where(o => o.Id == request.Id)
            .Select(o => new GetOrderByIdResponse(
                o.Id,
                o.UserEmail,
                o.TotalPrice,
                o.Items.Select(i => new GetOrderByIdResponseItem(
                    i.BookId,
                    i.Book!.Title,
                    i.Quantity,
                    i.UnitPrice,
                    i.UnitPrice * i.Quantity)).ToList()))
            .FirstOrDefaultAsync(ct);

        if (order is null)
        {
            await Send.NotFoundAsync(cancellation: ct);
            return;
        }

        await Send.OkAsync(order, cancellation: ct);
    }
}

public class GetOrderByIdRequest
{
    public int Id { get; set; }
}

public record GetOrderByIdResponse(
    int Id,
    string UserEmail,
    decimal TotalPrice,
    List<GetOrderByIdResponseItem> Items
);

public record GetOrderByIdResponseItem(
    int BookId,
    string Title,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal
);


