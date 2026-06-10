using Ardalis.ApiEndpoints;
using BookStore.Helpers;
using BookStore.Models.Data;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Features.Books;

public class UpdateBook
    : EndpointBaseAsync
        .WithRequest<UpdateBook.UpdateBookRequest>
        .WithActionResult<UpdateBook.BookDto>
{
    private readonly AppDbContext _db;
    private readonly IValidator<UpdateBookRequest> _validator;

    public UpdateBook(AppDbContext db, IValidator<UpdateBookRequest> validator)
    {
        _db = db;
        _validator = validator;
    }

    [HttpPut("api/books/{id:int}")]
    public override async Task<ActionResult<BookDto>> HandleAsync(
        [FromBody] UpdateBookRequest request,
        CancellationToken ct = default)
    {
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            validation.AddToModelState(ModelState);
            return ValidationProblem(ModelState);
        }

        var book = await _db.Books.FirstOrDefaultAsync(b => b.Id == request.Id, ct);

        if (book is null)
        {
            return NotFound();
        }

        book.Title = request.Title;
        book.Author = request.Author;
        book.Genre = request.Genre ?? string.Empty;
        book.Description = request.Description ?? string.Empty;
        book.Price = request.Price;

        await _db.SaveChangesAsync(ct);

        var dto = new BookDto(book.Id, book.Title, book.Author, book.Genre, book.Description, book.Price);

        return Ok(dto);
    }

    public record UpdateBookRequest(
        int Id,
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

    public class Validator : AbstractValidator<UpdateBookRequest>
    {
        public Validator()
        {
            RuleFor(x => x.Title).NotEmpty();
            RuleFor(x => x.Author).NotEmpty();
            RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        }
    }
}
