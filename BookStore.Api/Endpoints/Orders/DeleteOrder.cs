using Ardalis.ApiEndpoints;
using BookStore.Models.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Endpoints.Orders;

public class DeleteOrder
    : EndpointBaseAsync
        .WithRequest<int>
        .WithActionResult
{
    private readonly AppDbContext _db;

    public DeleteOrder(AppDbContext db) => _db = db;

    [HttpDelete("api/orders/{id:int}")]
    public override async Task<ActionResult> HandleAsync(
        int id,
        CancellationToken ct = default)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

        if (order is null)
        {
            return NotFound();
        }

        _db.OrderItems.RemoveRange(order.Items);
        _db.Orders.Remove(order);
        await _db.SaveChangesAsync(ct);

        return NoContent();
    }
}
