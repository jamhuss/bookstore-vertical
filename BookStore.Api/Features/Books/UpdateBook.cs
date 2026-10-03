using BookStore.Models.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Features.Books;

[HttpPut("api/books/{id:int}")]
[AllowAnonymous]
public class UpdateBook : Endpoint<UpdateBookRequest, UpdateBookResponse, UpdateBookMapper>
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

        Map.UpdateEntity(request, book);

        await _db.SaveChangesAsync(ct);

        var dto = Map.FromEntity(book);

        await Send.OkAsync(dto, cancellation: ct);
    }
}

#region Validators
public class UpdateBookValidator : Validator<UpdateBookRequest>
{
    public UpdateBookValidator()
    {
        RuleFor(x => x.Title).NotEmpty();
        RuleFor(x => x.Author).NotEmpty();
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
    }
}
#endregion

#region Models and Mappers
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

public class UpdateBookMapper : Mapper<UpdateBookRequest, UpdateBookResponse, Book>
{
    public override Book UpdateEntity(UpdateBookRequest r, Book e)
    {
        e.Title = r.Title;
        e.Author = r.Author;
        e.Genre = r.Genre ?? string.Empty;
        e.Description = r.Description ?? string.Empty;
        e.Price = r.Price;
        return e;
    }

    public override UpdateBookResponse FromEntity(Book e) => new()
    {
        Id = e.Id,
        Title = e.Title,
        Author = e.Author,
        Genre = e.Genre,
        Description = e.Description,
        Price = e.Price
    };
}
#endregion
