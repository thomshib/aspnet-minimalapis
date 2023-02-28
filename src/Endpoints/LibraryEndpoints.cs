
using System.Net;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;

public class LibraryEndpoints : IEndpoints
{
    public static void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IBookService, BookService>();
    }

    public static void AddEndpoints(IEndpointRouteBuilder app)
    {
        //create books
        app.MapPost("books", CreatBookAsync).WithName("CreateBook")
        .Accepts<Book>("application/json")
        .Produces<Book>(201)
        .Produces<ValidationFailure>(400)
        .WithTags("Books");

        // Get all and search by title
        app.MapGet("books", GetAllBooksAsync)
        .WithName("GetBooks")
        .Produces<IEnumerable<Book>>(200)
        .WithTags("Books");

        app.MapGet("books/{isbn}", GetBookByIsbnAsync).WithName("GetBook")
        .Produces<Book>(200)
        .Produces(404)
        .WithTags("Books");

        // update book
        app.MapPut("books/{isbn}", UpdateBookAsync)
        .WithName("UpdateBook")
        .Accepts<Book>("application/json")
        .Produces<Book>(200)
        .Produces<ValidationFailure>(400)
        .WithTags("Books");


        //Delete Book

        app.MapDelete("books/{isbn}", DeleteBookAsync)
        .WithName("DeleteBook")
        .Produces(404)
        .Produces(204)
        .WithTags("Books");


        app.MapGet("status", () =>
        {

            return Results.Extensions.Html(@"<!doctype html>
        <html>
        <head> <title> Status page</title></head>
        <body>
        <h1>Status</h1>
        <p> The server is working fine</p>
        </body>
        </html>
    
    ");
        }).RequireCors("AnyOrigin")
        .ExcludeFromDescription();
    }

    #region internal methods
    internal async static Task<IResult> CreatBookAsync(
        Book book, IBookService bookService, IValidator<Book> validator,
        LinkGenerator linker, HttpContext context)
    {


        var validationResult = await validator.ValidateAsync(book);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.Errors);
        }

        var created = await bookService.CreateAsync(book);

        if (!created)
        {
            return Results.BadRequest(new List<ValidationFailure>
                {
            new ("Isbn","A book with this ISBN already exists")
                });

        }

        var path = linker.GetPathByName("GetBook", new { isbn = book.Isbn });
        var locationUri = linker.GetUriByName(context, "GetBook", new { isbn = book.Isbn })!;
        return Results.Created(locationUri, book);

    }

    internal async static Task<IResult> GetAllBooksAsync(IBookService bookService, string? searchTerm)
    {
        if (searchTerm is not null && !string.IsNullOrWhiteSpace(searchTerm))
        {

            var matchedBooks = await bookService.SearchByTitleAsync(searchTerm);
            return Results.Ok(matchedBooks);
        }

        var books = await bookService.GetAllAsync();
        return Results.Ok(books);
    }

    internal async static Task<IResult> GetBookByIsbnAsync(string isbn, IBookService bookService)
    {
        var book = await bookService.GetByIsbnAsync(isbn);
        return book is not null ? Results.Ok(book) : Results.NotFound();

    }

     internal async static Task<IResult> UpdateBookAsync(Book book, IBookService bookService, IValidator<Book> validator, string isbn)
        {

            book.Isbn = isbn;
            var validationResult = await validator.ValidateAsync(book);
            if (!validationResult.IsValid)
            {
                return Results.BadRequest(validationResult.Errors);
            }

            var updated = await bookService.UpdateAsync(book);

            return updated ? Results.Ok(book) : Results.NotFound();
        }

        internal async static Task<IResult> DeleteBookAsync(IBookService bookService, string isbn) 
        {
            var deleted = await bookService.DeleteAsync(isbn);

            return deleted ? Results.NoContent() : Results.NotFound();
        }



    #endregion internal methods
}