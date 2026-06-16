using Ardalis.ApiEndpoints;
using BookStore.Helpers;
using BookStore.Models.Data;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Endpoints.Books;

public class CreateBook
    : EndpointBaseAsync
        .WithRequest<CreateBookRequest>
        .WithActionResult<CreateBookResponse>
{
    private readonly AppDbContext _db;
    private readonly IValidator<CreateBookRequest> _validator;

    public CreateBook(AppDbContext db, IValidator<CreateBookRequest> validator)
    {
        _db = db;
        _validator = validator;
    }

    [HttpPost("api/books")]
    public override async Task<ActionResult<CreateBookResponse>> HandleAsync(
        [FromBody] CreateBookRequest request,
        CancellationToken ct = default)
    {
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            validation.AddToModelState(ModelState);
            return ValidationProblem(ModelState);
        }

        var book = new Book
        {
            Title = request.Title,
            Author = request.Author,
            Genre = request.Genre ?? string.Empty,
            Description = request.Description ?? string.Empty,
            Price = request.Price
        };

        _db.Books.Add(book);
        await _db.SaveChangesAsync(ct);

        var dto = new CreateBookResponse
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author,
            Genre = book.Genre,
            Description = book.Description,
            Price = book.Price
        };

        return Created($"api/books/{book.Id}", dto);
    }

    public class Validator : AbstractValidator<CreateBookRequest>
    {
        public Validator()
        {
            RuleFor(x => x.Title).NotEmpty();
            RuleFor(x => x.Author).NotEmpty();
            RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        }
    }
}
public record CreateBookRequest
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string? Genre { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; } = 0;
}
public record CreateBookResponse : CreateBookRequest
{
    public int Id { get; set; }
}