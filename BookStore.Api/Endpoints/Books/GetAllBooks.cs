using Ardalis.ApiEndpoints;
using BookStore.Helpers;
using BookStore.Models.Data;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Endpoints.Books;

public class GetAllBooks
    : EndpointBaseAsync
        .WithRequest<ListRequest>
        .WithActionResult<PagedResult<GetAllBooksResponseItem>>
{
    private readonly AppDbContext _db;

    public GetAllBooks(AppDbContext db) => _db = db;

    [HttpGet("api/books")]
    public override async Task<ActionResult<PagedResult<GetAllBooksResponseItem>>> HandleAsync(
        [FromQuery] ListRequest request,
        CancellationToken ct = default)
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

        return Ok(result);
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
