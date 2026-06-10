using Ardalis.ApiEndpoints;
using BookStore.Models.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Features.Books;

public class DeleteBook
    : EndpointBaseAsync
        .WithRequest<int>
        .WithActionResult
{
    private readonly AppDbContext _db;

    public DeleteBook(AppDbContext db) => _db = db;

    [HttpDelete("api/books/{id:int}")]
    public override async Task<ActionResult> HandleAsync(
        int id,
        CancellationToken ct = default)
    {
        var book = await _db.Books.FirstOrDefaultAsync(b => b.Id == id, ct);

        if (book is null)
        {
            return NotFound();
        }

        _db.Books.Remove(book);
        await _db.SaveChangesAsync(ct);

        return NoContent();
    }
}
