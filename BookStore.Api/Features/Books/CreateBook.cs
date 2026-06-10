using Ardalis.ApiEndpoints;
using BookStore.Helpers;
using BookStore.Models.Data;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Features.Books;

public class CreateBook
    : EndpointBaseAsync
        .WithRequest<CreateBook.CreateBookRequest>
        .WithActionResult<CreateBook.BookDto>
{
    private readonly AppDbContext _db;
    private readonly IValidator<CreateBookRequest> _validator;

    public CreateBook(AppDbContext db, IValidator<CreateBookRequest> validator)
    {
        _db = db;
        _validator = validator;
    }

    [HttpPost("api/books")]
    public override async Task<ActionResult<BookDto>> HandleAsync(
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

        var dto = new BookDto(book.Id, book.Title, book.Author, book.Genre, book.Description, book.Price);

        return Created($"api/books/{book.Id}", dto);
    }

    public record CreateBookRequest(
        string Title,
        string Author,
        string? Genre,
        string? Description,
        decimal Price);

    public record BookDto(
        int Id,
        string Title,
        string Author,
        string Genre,
        string Description,
        decimal Price);

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
