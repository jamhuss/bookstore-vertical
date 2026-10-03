using BookStore.Models.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Features.Books;

[HttpGet("api/books/{id:int}")]
[AllowAnonymous]
public class GetBookById
    : Endpoint<GetBookByIdRequest, GetBookByIdResponse>
{
    private readonly AppDbContext _db;

    public GetBookById(AppDbContext db) => _db = db;

    public override async Task HandleAsync(GetBookByIdRequest request, CancellationToken ct = default)
    {
        var book = await _db.Books
            .Where(b => b.Id == request.Id)
            .Select(b => new GetBookByIdResponse
            (
                b.Id,
                b.Title,
                b.Author,
                b.Genre,
                b.Description,
                b.Price
            )).FirstOrDefaultAsync(ct);

        if (book is null)
        {
            await Send.NotFoundAsync(cancellation: ct);
            return;
        }

        await Send.OkAsync(book, cancellation: ct);
    }
}

public class GetBookByIdRequest
{
    public int Id { get; set; }
}

public record GetBookByIdResponse(
    int Id,
    string Title,
    string Author,
    string Genre,
    string Description,
    decimal Price
);