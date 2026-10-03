using System.Net;
using System.Net.Http.Json;
using BookStore.Features.Books;
using BookStore.IntegrationTests.Infrastructure;

namespace BookStore.IntegrationTests.Features.Books;

public class CreateBookTests : IClassFixture<BookStoreWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CreateBookTests(BookStoreWebApplicationFactory factory)
        => _client = factory.CreateClient();

    [Fact]
    public async Task Should_Create_Book_When_Request_Is_Valid()
    {
        var request = new CreateBookRequest
        {
            Title = "Clean Code",
            Author = "Robert C. Martin",
            Genre = "Programming",
            Description = "Software craftsmanship",
            Price = 499
        };

        var response = await _client.PostAsJsonAsync("/api/books", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK); // Send.OkAsync
        var book = await response.Content.ReadFromJsonAsync<CreateBookResponse>();
        book.Should().NotBeNull();
        book!.Title.Should().Be("Clean Code");
        book.Author.Should().Be("Robert C. Martin");
    }

    [Fact]
    public async Task Should_Return_BadRequest_When_Title_Is_Missing()
    {
        var request = new CreateBookRequest
        {
            Title = string.Empty,
            Author = "Robert C. Martin",
            Price = 499
        };

        var response = await _client.PostAsJsonAsync("/api/books", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest); // CreateBookValidator
    }
}
