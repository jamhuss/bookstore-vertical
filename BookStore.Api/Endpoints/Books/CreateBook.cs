using BookStore.Models.Data;
using Microsoft.AspNetCore.Authorization;

namespace BookStore.Endpoints.Books;

[AllowAnonymous]
[HttpPost("api/books")]
public class CreateBook
    : Endpoint<CreateBookRequest, CreateBookResponse, BookMapper>
{
    private readonly AppDbContext _db;

    public CreateBook(AppDbContext db)
    {
        _db = db;
    }

    public override async Task HandleAsync(CreateBookRequest request, CancellationToken ct = default)
    {

        var book = Map.ToEntity(request);

        _db.Books.Add(book);

        await _db.SaveChangesAsync(ct);

        var dto = Map.FromEntity(book);

        await Send.OkAsync(dto, cancellation: ct);
    }


}

#region Validators
public class CreateBookValidator : Validator<CreateBookRequest>
{
    public CreateBookValidator()
    {
        RuleFor(x => x.Title).NotEmpty();
        RuleFor(x => x.Author).NotEmpty();
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
    }
}
#endregion

#region Models and Mappers
public class CreateBookRequest
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string? Genre { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; } = 0;
}
public class CreateBookResponse : CreateBookRequest
{
    public int Id { get; set; }
}

public class BookMapper : Mapper<CreateBookRequest, CreateBookResponse, Book>
{
    public override Book ToEntity(CreateBookRequest r) => new()
    {
        Title = r.Title,
        Author = r.Author,
        Genre = r.Genre ?? string.Empty,
        Description = r.Description ?? string.Empty,
        Price = r.Price
    };

    public override CreateBookResponse FromEntity(Book e) => new()
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