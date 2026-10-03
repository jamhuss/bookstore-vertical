using BookStore.Models.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Features.Books;

[HttpDelete("api/books/{id:int}")]
[AllowAnonymous]
public class DeleteBook
    : Endpoint<DeleteBookRequest>
{
    private readonly AppDbContext _db;

    public DeleteBook(AppDbContext db) => _db = db;

    
    public override async Task HandleAsync(
        DeleteBookRequest request,
        CancellationToken ct = default)
    {
        var book = await _db.Books.FirstOrDefaultAsync(b => b.Id == request.Id, ct);

        if (book is null)
        {
            await Send.NotFoundAsync(cancellation: ct);
            return;
        }

        _db.Books.Remove(book);
        await _db.SaveChangesAsync(ct);

        await Send.NoContentAsync(cancellation: ct);
    }
}
public record DeleteBookRequest
(
    int Id
);