using BookStore.Models.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Features.Orders;

[HttpDelete("api/orders/{id:int}")]
[AllowAnonymous]
public class DeleteOrder
    : Endpoint<DeleteOrderRequest>
{
    private readonly AppDbContext _db;

    public DeleteOrder(AppDbContext db) => _db = db;

    public override async Task HandleAsync(DeleteOrderRequest request, CancellationToken ct = default)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.Id, ct);

        if (order is null)
        {
            await Send.NotFoundAsync(cancellation: ct);
            return;
        }

        _db.OrderItems.RemoveRange(order.Items);
        _db.Orders.Remove(order);
        await _db.SaveChangesAsync(ct);

        await Send.NoContentAsync(cancellation: ct);
    }
}

public class DeleteOrderRequest
{
    public int Id { get; set; }
}
