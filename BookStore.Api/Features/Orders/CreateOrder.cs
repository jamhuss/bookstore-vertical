using BookStore.Models.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

using Order = BookStore.Models.Data.Order;

namespace BookStore.Features.Orders;

[HttpPost("api/orders")]
[AllowAnonymous]
public class CreateOrder
    : Endpoint<CreateOrderRequest, CreateOrderResponse, CreateOrderMapper>
{
    private readonly AppDbContext _db;

    public CreateOrder(AppDbContext db) => _db = db;

    public override async Task HandleAsync(CreateOrderRequest request, CancellationToken ct = default)
    {
        var bookIds = request.Items.Select(i => i.BookId).Distinct().ToList();

        var books = await _db.Books
            .Where(b => bookIds.Contains(b.Id))
            .ToDictionaryAsync(b => b.Id, ct);

        var missing = bookIds.Where(id => !books.ContainsKey(id)).ToList();
        if (missing.Count > 0)
        {
            await Send.ResultAsync(Results.BadRequest($"The following book ids do not exist: {string.Join(", ", missing)}."));
            return;
        }

        var order = new Order
        {
            UserEmail = request.UserEmail,
            Items = request.Items.Select(i => new OrderItem
            {
                BookId = i.BookId,
                Book = books[i.BookId],
                Quantity = i.Quantity,
                UnitPrice = books[i.BookId].Price
            }).ToList()
        };

        order.TotalPrice = order.Items.Sum(i => i.UnitPrice * i.Quantity);

        _db.Orders.Add(order);
        await _db.SaveChangesAsync(ct);

        var dto = Map.FromEntity(order);

        await Send.CreatedAtAsync<GetOrderById>(new { id = order.Id }, dto, cancellation: ct);
    }
}

public class CreateOrderValidator : Validator<CreateOrderRequest>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.UserEmail).NotEmpty().EmailAddress();
        RuleFor(x => x.Items).NotEmpty()
            .WithMessage("An order must contain at least one item.");
        RuleForEach(x => x.Items).ChildRules(item =>
            item.RuleFor(i => i.Quantity).GreaterThanOrEqualTo(1));
    }
}

public record CreateOrderRequest(
    string UserEmail,
    List<CreateOrderItem> Items
);

public record CreateOrderItem(
    int BookId,
    int Quantity
);

public record CreateOrderResponse(
    int Id,
    string UserEmail,
    decimal TotalPrice,
    List<CreateOrderResponseItem> Items
);

public record CreateOrderResponseItem(
    int BookId,
    string Title,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal
);

public class CreateOrderMapper : Mapper<CreateOrderRequest, CreateOrderResponse, Order>
{
    public override CreateOrderResponse FromEntity(Order e) => new(
        e.Id,
        e.UserEmail,
        e.TotalPrice,
        e.Items.Select(i => new CreateOrderResponseItem(
            i.BookId,
            i.Book!.Title,
            i.Quantity,
            i.UnitPrice,
            i.LineTotal)).ToList());
}