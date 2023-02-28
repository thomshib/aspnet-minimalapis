using Microsoft.AspNetCore.Mvc.Testing;
using FluentAssertions;
using System.Net;
public class LibraryEndpointTests: IClassFixture<WebApplicationFactory<IApiMarker>> , IAsyncLifetime{


    // Empty interface IApiMarker to avoid type scans 
    private readonly WebApplicationFactory<IApiMarker> _factory;

    List<string> _createdIsbns = new();
    public LibraryEndpointTests(WebApplicationFactory<IApiMarker> factory)
    {
        _factory = factory;      

    }

    [Fact]
    public async Task CreateBook_CreatesBook_WhenDataIsCorrect()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
    
        // Act

        var result = await httpClient.PostAsJsonAsync("/books", book);
        var createdBook = await result.Content.ReadFromJsonAsync<Book>();
        _createdIsbns.Add(book.Isbn);

        // Assert

        result.StatusCode.Should().Be(HttpStatusCode.Created);
        createdBook.Should().BeEquivalentTo(book);
        result.Headers.Location.Should().Be($"http://localhost/books/{book.Isbn}");




        
    }

    [Fact]

    public async Task CreateBook_Fails_WhenIsbnIsInvalid(){

         // Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
        book.Isbn = "INVALID";
    
        // Act

        var result = await httpClient.PostAsJsonAsync("/books", book);
        var errors = await result.Content.ReadFromJsonAsync<IEnumerable<ValidationError>>();
        var error = errors!.Single();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.PropertyName.Should().Be("Isbn");
        error.ErrorMessage.Should().Be("Value was not a valid ISBN-13");



        


        

    } 

     [Fact]

    public async Task CreateBook_Fails_WhenBookExists(){

         // Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
        
    
        // Act
        await httpClient.PostAsJsonAsync("/books", book);
         _createdIsbns.Add(book.Isbn);
        var result = await httpClient.PostAsJsonAsync("/books", book);
        var errors = await result.Content.ReadFromJsonAsync<IEnumerable<ValidationError>>();
        var error = errors!.Single();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.PropertyName.Should().Be("Isbn");
        error.ErrorMessage.Should().Be("A book with this ISBN already exists"); 

      

    } 

     [Fact]
    public async Task GetBook_ReturnsBook_WhenBookExists(){
        // Arrange

        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
         await httpClient.PostAsJsonAsync("/books", book);
         _createdIsbns.Add(book.Isbn);

        // Act

        var result = await httpClient.GetAsync($"/books/{book.Isbn}");
        var exisingBook = await result.Content.ReadFromJsonAsync<Book>();


        // Assert
        exisingBook.Should().BeEquivalentTo(book);
        result.StatusCode.Should().Be(HttpStatusCode.OK);


    }

     [Fact]
    public async Task GetBook_ReturnsNotFound_WhenBookDoesNotExists(){
        // Arrange

        var httpClient = _factory.CreateClient();
        var isbn = GenerateIsbn();

        // Act

        var result = await httpClient.GetAsync($"/books/{isbn}");
        


        // Assert
      
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);


    }

    [Fact]
    public async Task GetAllBooks_ReturnsAllBooks_WhenBooksExist(){

        //Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
        await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);
        var books = new List<Book>{book};
        
        //Acty

        var result = await httpClient.GetAsync("/books");
        var returnedBooks = await result.Content.ReadFromJsonAsync<IEnumerable<Book>>();


        //Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedBooks.Should().BeEquivalentTo(books);



    }

     [Fact]
    public async Task GetAllBooks_ReturnsNoBooks_WhenNoBooksExist(){

        //Arrange
        var httpClient = _factory.CreateClient();

        //Act

        var result = await httpClient.GetAsync("/books");
        var returnedBooks = await result.Content.ReadFromJsonAsync<IEnumerable<Book>>();

          //Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);

        returnedBooks.Should().BeEmpty();

    }

     [Fact]
    public async Task SearchBooks_ReturnsBooks_WhenTitleMatches(){
         //Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
        await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);
        var books = new List<Book>{book};
        
        //Act

        var result = await httpClient.GetAsync("/books?searchTerm=oder");
        var returnedBooks = await result.Content.ReadFromJsonAsync<IEnumerable<Book>>();


        //Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedBooks.Should().BeEquivalentTo(books);

    }

    [Fact]
    public async Task UpdateBook_UpdatesBook_WhenDataIsCorrect(){

          //Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
        await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);
        
        //Act 
        book.PageCount = 999;
        var result = await httpClient.PutAsJsonAsync<Book>($"/books/{book.Isbn}", book);

        var updatedBook = await result.Content.ReadFromJsonAsync<Book>();

        //Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        updatedBook.Should().BeEquivalentTo(book);

    }


     [Fact]
    public async Task UpdateBook_DoesNotUpdateBook_WhenDataIsIncorrect(){

          //Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
        await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);
        
        //Act 
        book.Title = string.Empty;
        var result = await httpClient.PutAsJsonAsync<Book>($"/books/{book.Isbn}", book);

        var errors = await result.Content.ReadFromJsonAsync<IEnumerable<ValidationError>>();
        var error = errors!.Single();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.PropertyName.Should().Be("Title");
        error.ErrorMessage.Should().Be("'Title' must not be empty."); 




    }

      [Fact]
    public async Task UpdateBook_ReturnsNotFound_WhenBookDoesNotExist(){

          //Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();        
        
        //Act 
        
        var result = await httpClient.PutAsJsonAsync<Book>($"/books/{book.Isbn}", book);


        //Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
       

    }

     [Fact]
    public async Task DeleteBooks_ReturnsNoContent_WhenBooksDoesExist(){

        //Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
        await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);

        //Act

        var result = await httpClient.DeleteAsync($"/books/{book.Isbn}");       

          //Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);

        

    }



    [Fact]
    public async Task DeleteBooks_ReturnsNotFound_WhenBooksDoesNotExist(){

        //Arrange
        var httpClient = _factory.CreateClient();
        var isbn = GenerateIsbn();

        //Act

        var result = await httpClient.DeleteAsync($"/books/{isbn}");       

          //Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);

        

    }


    private Book GenerateBook(string title = " The Clean Coder"){
        return new Book{
            Isbn = GenerateIsbn(),
            Title = title,
            Author = "Shibu Thomas",
            ShortDescription = "All the tricks in one book",
            PageCount = 420,
            ReleaseDate = new DateTime(2010,1,1)        

        };
    }

   

    private string GenerateIsbn(){
        return $"{Random.Shared.Next(100,999)}-" +
            $"{Random.Shared.Next(1000000000,2100999999)}";
    }

    public Task InitializeAsync() => Task.CompletedTask;
   

    public async Task DisposeAsync()
    {
        var httpClient = _factory.CreateClient();

        foreach(var isbn in _createdIsbns){
            await httpClient.DeleteAsync($"/books/{isbn}");
        }
    }

}
