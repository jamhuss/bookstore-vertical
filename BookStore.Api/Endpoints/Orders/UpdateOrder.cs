using Ardalis.ApiEndpoints;
using BookStore.Helpers;
using BookStore.Models.Data;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Endpoints.Orders;

public class UpdateOrder
    : EndpointBaseAsync
        .WithRequest<UpdateOrder.UpdateOrderRequest>
        .WithActionResult<UpdateOrder.OrderDto>
{
    private readonly AppDbContext _db;
    private readonly IValidator<UpdateOrderRequest> _validator;

    public UpdateOrder(AppDbContext db, IValidator<UpdateOrderRequest> validator)
    {
        _db = db;
        _validator = validator;
    }

    [HttpPut("api/orders/{id:int}")]
    public override async Task<ActionResult<OrderDto>> HandleAsync(
        [FromBody] UpdateOrderRequest request,
        CancellationToken ct = default)
    {
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            validation.AddToModelState(ModelState);
            return ValidationProblem(ModelState);
        }

        var order = await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.Id, ct);

        if (order is null)
        {
            return NotFound();
        }

        var bookIds = request.Items.Select(i => i.BookId).Distinct().ToList();

        var books = await _db.Books
            .Where(b => bookIds.Contains(b.Id))
            .ToDictionaryAsync(b => b.Id, ct);

        var missing = bookIds.Where(bid => !books.ContainsKey(bid)).ToList();
        if (missing.Count > 0)
        {
            return BadRequest($"The following book ids do not exist: {string.Join(", ", missing)}.");
        }

        _db.OrderItems.RemoveRange(order.Items);

        order.UserEmail = request.UserEmail;
        order.Items = request.Items.Select(i => new OrderItem
        {
            OrderId = order.Id,
            BookId = i.BookId,
            Quantity = i.Quantity,
            UnitPrice = books[i.BookId].Price
        }).ToList();
        order.TotalPrice = order.Items.Sum(i => i.UnitPrice * i.Quantity);

        await _db.SaveChangesAsync(ct);

        var dto = new OrderDto(
            order.Id,
            order.UserEmail,
            order.TotalPrice,
            order.Items.Select(i => new OrderItemDto(
                i.BookId,
                books[i.BookId].Title,
                i.Quantity,
                i.UnitPrice,
                i.UnitPrice * i.Quantity)).ToList());

        return Ok(dto);
    }

    public record UpdateOrderRequest(
        int Id,
        string UserEmail,
        List<UpdateOrderItem> Items);

    public record UpdateOrderItem(
        int BookId,
        int Quantity);

    public record OrderDto(
        int Id,
        string UserEmail,
        decimal TotalPrice,
        List<OrderItemDto> Items);

    public record OrderItemDto(
        int BookId,
        string Title,
        int Quantity,
        decimal UnitPrice,
        decimal LineTotal);

    public class Validator : AbstractValidator<UpdateOrderRequest>
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
