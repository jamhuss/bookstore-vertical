using BookStore.Helpers;
using BookStore.Models.Data;
using Microsoft.AspNetCore.Authorization;

namespace BookStore.Endpoints.Books;

[HttpGet("api/books")]
[AllowAnonymous]
public class GetAllBooks
    : Endpoint<ListRequest, PagedResult<GetAllBooksResponseItem>>
{
    private readonly AppDbContext _db;

    public GetAllBooks(AppDbContext db) => _db = db;

    public override async Task HandleAsync(ListRequest request, CancellationToken ct = default)
    {
        var query = _db.Books
            .OrderBy(b => b.Title)
            .Select(b => new GetAllBooksResponseItem(
                b.Id,
                b.Title,
                b.Author,
                b.Genre,
                b.Description,
                b.Price
            ));

        var result = await query.ToPagedResultAsync(request.Page, request.PageSize, ct);

        await Send.OkAsync(result, cancellation: ct);
    }
}

public record GetAllBooksResponseItem(
    int Id,
    string Title,
    string Author,
    string Genre,
    string Description,
    decimal Price
);
