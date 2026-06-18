using BookStore.Models.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

using Order = BookStore.Models.Data.Order;

namespace BookStore.Endpoints.Orders;

[HttpPut("api/orders/{id:int}")]
[AllowAnonymous]
public class UpdateOrder : Endpoint<UpdateOrderRequest, UpdateOrderResponse, UpdateOrderMapper>
{
    private readonly AppDbContext _db;

    public UpdateOrder(AppDbContext db) => _db = db;

    public override async Task HandleAsync(UpdateOrderRequest request, CancellationToken ct = default)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.Id, ct);

        if (order is null)
        {
            await Send.NotFoundAsync(cancellation: ct);
            return;
        }

        var bookIds = request.Items.Select(i => i.BookId).Distinct().ToList();

        var books = await _db.Books
            .Where(b => bookIds.Contains(b.Id))
            .ToDictionaryAsync(b => b.Id, ct);

        var missing = bookIds.Where(bid => !books.ContainsKey(bid)).ToList();
        if (missing.Count > 0)
        {
            await Send.ResultAsync(Results.BadRequest($"The following book ids do not exist: {string.Join(", ", missing)}."));
            return;
        }

        _db.OrderItems.RemoveRange(order.Items);

        order.UserEmail = request.UserEmail;
        order.Items = request.Items.Select(i => new OrderItem
        {
            OrderId = order.Id,
            BookId = i.BookId,
            Book = books[i.BookId],
            Quantity = i.Quantity,
            UnitPrice = books[i.BookId].Price
        }).ToList();
        order.TotalPrice = order.Items.Sum(i => i.UnitPrice * i.Quantity);

        await _db.SaveChangesAsync(ct);

        var dto = Map.FromEntity(order);

        await Send.OkAsync(dto, cancellation: ct);
    }
}

public record UpdateOrderRequest(
    int Id,
    string UserEmail,
    List<UpdateOrderItem> Items);

public record UpdateOrderItem(
    int BookId,
    int Quantity);

public record UpdateOrderResponse(
    int Id,
    string UserEmail,
    decimal TotalPrice,
    List<UpdateOrderResponseItem> Items);

public record UpdateOrderResponseItem(
    int BookId,
    string Title,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);

public class UpdateOrderMapper : Mapper<UpdateOrderRequest, UpdateOrderResponse, Order>
{
    public override UpdateOrderResponse FromEntity(Order e) => new(
        e.Id,
        e.UserEmail,
        e.TotalPrice,
        e.Items.Select(i => new UpdateOrderResponseItem(
            i.BookId,
            i.Book!.Title,
            i.Quantity,
            i.UnitPrice,
            i.LineTotal)).ToList());
}

public class UpdateOrderValidator : Validator<UpdateOrderRequest>
{
    public UpdateOrderValidator()
    {
        RuleFor(x => x.UserEmail).NotEmpty().EmailAddress();
        RuleFor(x => x.Items).NotEmpty()
            .WithMessage("An order must contain at least one item.");
        RuleForEach(x => x.Items).ChildRules(item =>
            item.RuleFor(i => i.Quantity).GreaterThanOrEqualTo(1));
    }
}
