using Ardalis.ApiEndpoints;
using BookStore.Models.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Endpoints.Books;

public class GetBookById
    : EndpointBaseAsync
        .WithRequest<int>
        .WithActionResult<GetBookByIdResponse>
{
    private readonly AppDbContext _db;

    public GetBookById(AppDbContext db) => _db = db;

    [HttpGet("api/books/{id:int}")]
    public override async Task<ActionResult<GetBookByIdResponse>> HandleAsync(
        int id,
        CancellationToken ct = default)
    {
        var book = await _db.Books
            .Where(b => b.Id == id)
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
            return NotFound();
        }

        return Ok(book);
    }
}

public record GetBookByIdResponse(
    int Id,
    string Title,
    string Author,
    string Genre,
    string Description,
    decimal Price
);