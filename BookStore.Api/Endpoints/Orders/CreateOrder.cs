using Ardalis.ApiEndpoints;
using BookStore.Helpers;
using BookStore.Models.Data;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Endpoints.Orders;

public class CreateOrder
    : EndpointBaseAsync
        .WithRequest<CreateOrderRequest>
        .WithActionResult<CreateOrderResponse>
{
    private readonly AppDbContext _db;
    private readonly IValidator<CreateOrderRequest> _validator;

    public CreateOrder(AppDbContext db, IValidator<CreateOrderRequest> validator)
    {
        _db = db;
        _validator = validator;
    }

    [HttpPost("api/orders")]
    public override async Task<ActionResult<CreateOrderResponse>> HandleAsync(
        [FromBody] CreateOrderRequest request,
        CancellationToken ct = default)
    {
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            validation.AddToModelState(ModelState);
            return ValidationProblem(ModelState);
        }

        var bookIds = request.Items.Select(i => i.BookId).Distinct().ToList();

        var books = await _db.Books
            .Where(b => bookIds.Contains(b.Id))
            .ToDictionaryAsync(b => b.Id, ct);

        var missing = bookIds.Where(id => !books.ContainsKey(id)).ToList();
        if (missing.Count > 0)
        {
            return BadRequest($"The following book ids do not exist: {string.Join(", ", missing)}.");
        }

        var order = new Order
        {
            UserEmail = request.UserEmail,
            Items = request.Items.Select(i => new OrderItem
            {
                BookId = i.BookId,
                Quantity = i.Quantity,
                UnitPrice = books[i.BookId].Price
            }).ToList()
        };

        order.TotalPrice = order.Items.Sum(i => i.UnitPrice * i.Quantity);

        _db.Orders.Add(order);
        await _db.SaveChangesAsync(ct);

        var dto = new CreateOrderResponse(
            order.Id,
            order.UserEmail,
            order.TotalPrice,
            order.Items.Select(i => new CreateOrderResponseItem(
                i.BookId,
                books[i.BookId].Title,
                i.Quantity,
                i.UnitPrice,
                i.UnitPrice * i.Quantity)).ToList());

        return Created($"api/orders/{order.Id}", dto);
    }

    public class Validator : AbstractValidator<CreateOrderRequest>
    {
        public Validator()
        {
            RuleFor(x => x.UserEmail).NotEmpty().EmailAddress();
            RuleFor(x => x.Items).NotEmpty()
                .WithMessage("An order must contain at least one item.");
            RuleForEach(x => x.Items).ChildRules(item =>
                item.RuleFor(i => i.Quantity).GreaterThanOrEqualTo(1));
        }
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