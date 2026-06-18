using BookStore.Models.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Endpoints.Books;

[HttpPut("api/books/{id:int}")]
[AllowAnonymous]
public class UpdateBook : Endpoint<UpdateBookRequest, UpdateBookResponse>
{
    private readonly AppDbContext _db;

    public UpdateBook(AppDbContext db) => _db = db;

    public override async Task HandleAsync(UpdateBookRequest request, CancellationToken ct = default)
    {
        var book = await _db.Books.FirstOrDefaultAsync(b => b.Id == request.Id, ct);

        if (book is null)
        {
            await Send.NotFoundAsync(cancellation: ct);
            return;
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

        await Send.OkAsync(dto, cancellation: ct);
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

public class UpdateBookValidator : Validator<UpdateBookRequest>
{
    public UpdateBookValidator()
    {
        RuleFor(x => x.Title).NotEmpty();
        RuleFor(x => x.Author).NotEmpty();
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
    }
}
