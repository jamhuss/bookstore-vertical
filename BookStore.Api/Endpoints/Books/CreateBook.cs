using BookStore.Models.Data;
using Microsoft.AspNetCore.Authorization;

namespace BookStore.Endpoints.Books;

[HttpPost("api/books")]
[AllowAnonymous]
public class CreateBook
    : Endpoint<CreateBookRequest, CreateBookResponse>
{
    private readonly AppDbContext _db;

    public CreateBook(AppDbContext db)
    {
        _db = db;
    }

    public override async Task HandleAsync(CreateBookRequest request, CancellationToken ct = default)
    {

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

       await Send.OkAsync(dto, cancellation: ct);
    }

    
}
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
public class CreateBookValidator : Validator<CreateBookRequest>
{
    public CreateBookValidator()
    {
        RuleFor(x => x.Title).NotEmpty();
        RuleFor(x => x.Author).NotEmpty();
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
    }
}