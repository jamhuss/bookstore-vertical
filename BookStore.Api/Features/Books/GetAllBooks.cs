using Ardalis.ApiEndpoints;
using BookStore.Helpers;
using BookStore.Models.Data;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Features.Books;

public class GetAllBooks
    : EndpointBaseAsync
        .WithRequest<ListRequest>
        .WithActionResult<PagedResult<GetAllBooks.BookDto>>
{
    private readonly AppDbContext _db;

    public GetAllBooks(AppDbContext db) => _db = db;

    [HttpGet("api/books")]
    public override async Task<ActionResult<PagedResult<BookDto>>> HandleAsync(
        [FromQuery] ListRequest request,
        CancellationToken ct = default)
    {
        var query = _db.Books
            .OrderBy(b => b.Title)
            .Select(b => new BookDto(
                b.Id,
                b.Title,
                b.Author,
                b.Genre,
                b.Description,
                b.Price));

        var result = await query.ToPagedResultAsync(request.Page, request.PageSize, ct);

        return Ok(result);
    }

    public record BookDto(
        int Id,
        string Title,
        string Author,
        string Genre,
        string Description,
        decimal Price);
}
