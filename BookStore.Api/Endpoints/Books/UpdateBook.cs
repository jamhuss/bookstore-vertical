using Ardalis.ApiEndpoints;
using BookStore.Helpers;
using BookStore.Models.Data;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Endpoints.Books;

public class UpdateBook
    : EndpointBaseAsync
        .WithRequest<UpdateBookRequest>
        .WithActionResult<UpdateBookResponse>
{
    private readonly AppDbContext _db;
    private readonly IValidator<UpdateBookRequest> _validator;

    public UpdateBook(AppDbContext db, IValidator<UpdateBookRequest> validator)
    {
        _db = db;
        _validator = validator;
    }

    [HttpPut("api/books/{id:int}")]
    public override async Task<ActionResult<UpdateBookResponse>> HandleAsync(
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

        var dto = new UpdateBookResponse
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author,
            Genre = book.Genre,
            Description = book.Description,
            Price = book.Price
        };

        return Ok(dto);
    }

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

public class UpdateBookRequest
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string? Genre { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; } = 0;
}

public class UpdateBookResponse : UpdateBookRequest { }
